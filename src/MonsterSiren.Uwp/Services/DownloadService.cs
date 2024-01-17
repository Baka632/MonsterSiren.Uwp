﻿using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 为歌曲及其相关内容的下载提供服务
/// </summary>
public static class DownloadService
{
    private static bool _isInitialized;
    private static string _downloadPath;
    private static bool _downloadLyric = true;

    /// <summary>
    /// 获取或设置下载路径
    /// </summary>
    public static string DownloadPath
    {
        get => _downloadPath;
        set
        {
            SettingsHelper.Set(CommonValues.MusicDownloadPathSettingsKey, value);
            _downloadPath = value;
        }
    }

    /// <summary>
    /// 指示是否下载歌词的值
    /// </summary>
    public static bool DownloadLyric
    {
        get => _downloadLyric;
        set
        {
            SettingsHelper.Set(CommonValues.MusicDownloadLyricSettingsKey, value);
            _downloadLyric = value;
        }
    }

    /// <summary>
    /// 获取下载列表
    /// </summary>
    public static ObservableCollection<DownloadItem> DownloadList { get; } = [];

    /// <summary>
    /// 指示应用是否因某种原因而改变下载文件夹的默认路径
    /// </summary>
    public static bool DownloadPathRedirected { get; private set; }

    /// <summary>
    /// 初始化下载服务
    /// </summary>
    public static async Task Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        DownloadLyric = !SettingsHelper.TryGet(CommonValues.MusicDownloadLyricSettingsKey, out bool dlLyric)
                        || dlLyric;

        if (SettingsHelper.TryGet(CommonValues.MusicDownloadPathSettingsKey, out string dlPath) && Directory.Exists(dlPath))
        {
            DownloadPath = dlPath;
        }
        else
        {
            if (dlPath != null)
            {
                DownloadPathRedirected = true;
            }

            try
            {
                StorageLibrary musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                StorageFolder storageFolder = await musicLib.SaveFolder.CreateFolderAsync(App.AppDisplayName, CreationCollisionOption.OpenIfExists);
                dlPath = storageFolder.Path;
            }
            catch (UnauthorizedAccessException)
            {
                StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
                StorageFolder dlFolder = await localCacheFolder.CreateFolderAsync("Downloads", CreationCollisionOption.OpenIfExists);
                dlPath = dlFolder.Path;
            }

            DownloadPath = dlPath;
            SettingsHelper.Set(CommonValues.MusicDownloadPathSettingsKey, dlPath);
        }

        IReadOnlyList<DownloadOperation> downloadsItem = await BackgroundDownloader.GetCurrentDownloadsAsync();
        foreach (DownloadOperation op in downloadsItem)
        {
            _ = HandleDownloadOperation(op, op.ResultFile.Name, false);
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 下载单个歌曲
    /// </summary>
    /// <param name="albumDetail">歌曲所属专辑信息</param>
    /// <param name="songDetail">歌曲详细信息</param>
    /// <param name="downloadLyric">指示是否在可能的情况下下载歌词的值</param>
    /// <exception cref="InvalidOperationException">未调用 <see cref="Initialize"/> 方法</exception>
    public static async Task DownloadSong(AlbumDetail albumDetail, SongDetail songDetail, bool downloadLyric = true)
    {
        if (_isInitialized != true)
        {
            throw new InvalidOperationException($"请先调用 {nameof(Initialize)} 方法");
        }

        if (DownloadList.Any(item => item.Operation.RequestedUri.ToString() == songDetail.SourceUrl))
        {
            return;
        }

        await Task.Run(async () =>
        {
            StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(DownloadPath);
            StorageFolder albumFolder = await downloadFolder.CreateFolderAsync(albumDetail.Name, CreationCollisionOption.OpenIfExists);
            StorageFile musicFile = await albumFolder.CreateFileAsync($"{songDetail.Name}.wav", CreationCollisionOption.ReplaceExisting);

            BackgroundDownloader downloader = new()
            {
                CostPolicy = BackgroundTransferCostPolicy.Always
            };

            DownloadOperation musicDownload = downloader.CreateDownload(new Uri(songDetail.SourceUrl, UriKind.Absolute), musicFile);
            await HandleDownloadOperation(musicDownload, songDetail.Name, true);

            if (downloadLyric && Uri.TryCreate(songDetail.LyricUrl, UriKind.Absolute, out Uri lrcUri))
            {
                StorageFile lrcFile = await albumFolder.CreateFileAsync($"{songDetail.Name}.lrc", CreationCollisionOption.ReplaceExisting);
                DownloadOperation lrcDownload = downloader.CreateDownload(lrcUri, lrcFile);
                await HandleDownloadOperation(lrcDownload, $"{songDetail.Name} - {"LyricFile".GetLocalized()}", true);
            }
        });
    }

    private static async Task HandleDownloadOperation(DownloadOperation operation, string name, bool isNew)
    {
        CancellationTokenSource cts = new();
        DownloadItem item = new(operation, name, cts);
        await AddToList(item);

        try
        {
            Progress<DownloadOperation> progressCallback = new(OnDownloadProgress);
            if (isNew)
            {
                await operation.StartAsync().AsTask(cts.Token, progressCallback);
            }
            else
            {
                await operation.AttachAsync().AsTask(cts.Token, progressCallback);
            }
        }
        catch (TaskCanceledException)
        {
            if (operation.ResultFile is not null)
            {
                await operation.ResultFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
        catch (Exception ex)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);

            if (error != WebErrorStatus.Unknown)
            {
                HttpRequestException httpRequestException = new(error.ToString(), ex);
                throw httpRequestException;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            await RemoveFromList(item);
        }
    }

    private static async Task AddToList(DownloadItem item)
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            DownloadList.Add(item);
        });
    }

    private static async Task RemoveFromList(DownloadItem item)
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            DownloadList.Remove(item);
        });
    }

    private static void OnDownloadProgress(DownloadOperation op)
    {
        DownloadItem item = DownloadList.FirstOrDefault(x => x.Operation == op);

        if (item is null)
        {
            return;
        }

        BackgroundDownloadProgress progress = op.Progress;
        item.Progress = op.Progress.TotalBytesToReceive == 0
            ? 0d
            : (double)progress.BytesReceived / op.Progress.TotalBytesToReceive;
    }
}
