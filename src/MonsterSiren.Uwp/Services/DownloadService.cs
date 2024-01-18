using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
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
    private static readonly BackgroundDownloader Downloader = new()
    {
        CostPolicy = BackgroundTransferCostPolicy.Always
    };

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
            string name = op.ResultFile is StorageFile file ? file.DisplayName : op.ResultFile.Name;
            _ = HandleDownloadOperation(op, name, false);
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 下载单个歌曲
    /// </summary>
    /// <param name="albumDetail">歌曲所属专辑信息</param>
    /// <param name="songDetail">歌曲详细信息</param>
    /// <exception cref="InvalidOperationException">未调用 <see cref="Initialize"/> 方法</exception>
    public static async Task DownloadSong(AlbumDetail albumDetail, SongDetail songDetail)
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
            char[] invalidFileChars = Path.GetInvalidFileNameChars();
            string musicName = songDetail.Name;
            foreach (char item in invalidFileChars)
            {
                if (musicName.Contains(item))
                {
                    musicName = musicName.Replace(item.ToString(), string.Empty);
                }
            }

            StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(DownloadPath);
            StorageFolder albumFolder = await downloadFolder.CreateFolderAsync(albumDetail.Name, CreationCollisionOption.OpenIfExists);
            StorageFile musicFile = await albumFolder.CreateFileAsync($"{musicName}.wav.tmp", CreationCollisionOption.ReplaceExisting);

            StorageFile infoFile = await albumFolder.CreateFileAsync($"{musicName}.json.tmp", CreationCollisionOption.ReplaceExisting);
            SongDetailAndAlbumDetailPack pack = new(songDetail, albumDetail);
            Stream infoFileStream = await infoFile.OpenStreamForWriteAsync();
            infoFileStream.Seek(0, SeekOrigin.Begin);
            await JsonSerializer.SerializeAsync(infoFileStream, pack);
            infoFileStream.Dispose();

            DownloadOperation musicDownload = Downloader.CreateDownload(new Uri(songDetail.SourceUrl, UriKind.Absolute), musicFile);
            await HandleDownloadOperation(musicDownload, musicName, true);

            if (DownloadLyric && Uri.TryCreate(songDetail.LyricUrl, UriKind.Absolute, out Uri lrcUri))
            {
                StorageFile lrcFile = await albumFolder.CreateFileAsync($"{musicName}.lrc.tmp", CreationCollisionOption.ReplaceExisting);
                DownloadOperation lrcDownload = Downloader.CreateDownload(lrcUri, lrcFile);
                await HandleDownloadOperation(lrcDownload, $"{musicName} - {"LyricFile".GetLocalized()}", true);
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
            await operation.ResultFile.RenameAsync(operation.ResultFile.Name.Replace(".tmp", string.Empty), NameCollisionOption.ReplaceExisting);
            await WriteTagsToFile((StorageFile)operation.ResultFile);
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

    private static async Task WriteTagsToFile(StorageFile musicFile)
    {
        if (Path.GetExtension(musicFile.Name) == ".lrc")
        {
            return;
        }

        string albumFolderPath = Path.GetDirectoryName(musicFile.Path);
        string infoFilePath = Path.Combine(albumFolderPath, $"{musicFile.DisplayName}.json.tmp");

        if (File.Exists(infoFilePath))
        {
            StorageFile infoFile = await StorageFile.GetFileFromPathAsync(infoFilePath);
            using Stream infoFileStream = await infoFile.OpenStreamForReadAsync();
            SongDetailAndAlbumDetailPack pack = await JsonSerializer.DeserializeAsync<SongDetailAndAlbumDetailPack>(infoFileStream);
            AlbumDetail albumDetail = pack.AlbumDetail;
            SongDetail songDetail = pack.SongDetail;
            UwpStorageFileAbstraction uwpStorageFile = new(musicFile);
            using TagLib.File file = TagLib.File.Create(uwpStorageFile);
            IRandomAccessStream coverStream = await FileCacheHelper.GetAlbumCoverStreamAsync(albumDetail);

            try
            {
                List<SongInfo> songs = albumDetail.Songs.ToList();
                file.Tag.Performers = songDetail.Artists.ToArray();
                file.Tag.Title = songDetail.Name;
                file.Tag.Album = albumDetail.Name;
                file.Tag.AlbumArtists = [songDetail.Artists.FirstOrDefault() ?? "MSR".GetLocalized()];
                file.Tag.Track = (uint)songs.FindIndex(info => info.Cid == songDetail.Cid) + 1;

                TagLib.Picture picture;
                if (coverStream is null)
                {
                    Uri coverUri = new(albumDetail.CoverUrl, UriKind.Absolute);
                    using Windows.Web.Http.HttpClient httpClient = new();
                    using Windows.Web.Http.HttpResponseMessage result = await httpClient.GetAsync(coverUri);

                    coverStream = new InMemoryRandomAccessStream();
                    await result.Content.WriteToStreamAsync(coverStream);
                }

                coverStream.Seek(0);
                Stream stream = coverStream.AsStreamForRead();
                picture = new(TagLib.ByteVector.FromStream(stream))
                {
                    MimeType = "image/jpeg"
                };
                file.Tag.Pictures = [picture];
            }
            finally
            {
                file.Save();
                coverStream.Dispose();
                await infoFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
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

    private static async Task TranscodeFile(StorageFile sourceFile, StorageFile destinationFile, MediaEncodingProfile profile)
    {
        MediaTranscoder transcoder = new();
        PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(sourceFile, destinationFile, profile);

        if (prepareOp.CanTranscode)
        {
            await prepareOp.TranscodeAsync();
        }
    }
}

internal class UwpStorageFileAbstraction : TagLib.File.IFileAbstraction, IDisposable
{
    private bool disposedValue;

    public UwpStorageFileAbstraction(IStorageFile file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        Name = file.Name;
        ReadStream = file.OpenStreamForReadAsync().GetAwaiter().GetResult();
        WriteStream = file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
    }

    public string Name { get; }

    public Stream ReadStream { get; }

    public Stream WriteStream { get; }

    public void CloseStream(Stream stream)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ReadStream?.Dispose();
                WriteStream?.Dispose();
            }
            disposedValue = true;
        }
    }
    ~UwpStorageFileAbstraction()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
