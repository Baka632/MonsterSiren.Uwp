using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Windows.Media.Core;
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
    private static bool _keepWavFileAfterTranscode;
    private static bool _downloadLyric = true;
    private static bool _transcodeDownloadedItem = true;
    private static CodecInfo _transcodeEncoderInfo;
    private static AudioEncodingQuality _transcodeQuality = AudioEncodingQuality.High;
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
    /// 指示是否转码下载项的值
    /// </summary>
    public static bool TranscodeDownloadedItem
    {
        get => _transcodeDownloadedItem;
        set
        {
            SettingsHelper.Set(CommonValues.MusicTranscodeDownloadedItemSettingsKey, value);
            _transcodeDownloadedItem = value;
        }
    }

    /// <summary>
    /// 获取或设置转码操作要使用的编码器信息
    /// </summary>
    public static CodecInfo TranscodeEncoderInfo
    {
        get => _transcodeEncoderInfo;
        set
        {
            SettingsHelper.Set(CommonValues.MusicTranscodeEncoderGuidSettingsKey, value.Subtypes[0]);
            _transcodeEncoderInfo = value;
        }
    }

    /// <summary>
    /// 获取或设置转码质量
    /// </summary>
    public static AudioEncodingQuality TranscodeQuality
    {
        get => _transcodeQuality;
        set
        {
            SettingsHelper.Set(CommonValues.MusicTranscodeQualitySettingsKey, value.ToString());
            _transcodeQuality = value;
        }
    }

    /// <summary>
    /// 指示在转码后是否保留原始 WAV 文件的值
    /// </summary>
    public static bool KeepWavFileAfterTranscode
    {
        get => _keepWavFileAfterTranscode;
        set
        {
            SettingsHelper.Set(CommonValues.MusicTranscodeKeepWavFileSettingsKey, value);
            _keepWavFileAfterTranscode = value;
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
    /// 指示设备是否支持常用的转码操作
    /// </summary>
    public static bool IsSupportCommonTranscode { get; private set; }

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

        KeepWavFileAfterTranscode = SettingsHelper.TryGet(CommonValues.MusicTranscodeKeepWavFileSettingsKey, out bool keepWav)
                                    && keepWav;

        if (CodecQueryHelper.TryGetCommonEncoders(out IEnumerable<CodecInfo> commonEncoders))
        {
            TranscodeEncoderInfo = SettingsHelper.TryGet(CommonValues.MusicTranscodeEncoderGuidSettingsKey, out string encoderGuid) && commonEncoders.Any(info => CodecQueryHelper.IsCodecInfoHasTargetEncoder(info, encoderGuid))
                ? commonEncoders.First(info => CodecQueryHelper.IsCodecInfoHasTargetEncoder(info, encoderGuid))
                : commonEncoders.First();

            TranscodeDownloadedItem = SettingsHelper.TryGet(CommonValues.MusicTranscodeDownloadedItemSettingsKey, out bool transcodeItem)
                ? transcodeItem
                : true;

            IsSupportCommonTranscode = true;
        }
        else
        {
            TranscodeDownloadedItem = false;
            IsSupportCommonTranscode = false;
        }

        if (SettingsHelper.TryGet(CommonValues.MusicTranscodeQualitySettingsKey, out string qualityString) && Enum.TryParse(qualityString, out AudioEncodingQuality quality) && quality != AudioEncodingQuality.Auto)
        {
            TranscodeQuality = quality;
        }
        else
        {
            TranscodeQuality = AudioEncodingQuality.High;
        }

        if (SettingsHelper.TryGet(CommonValues.MusicDownloadPathSettingsKey, out string dlPath) && await IsFolderExist(dlPath))
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
            foreach (char invalidChar in invalidFileChars)
            {
                if (musicName.Contains(invalidChar))
                {
                    musicName = musicName.Replace(invalidChar.ToString(), string.Empty);
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
            item.State = DownloadItemState.Downloading;
            Progress<DownloadOperation> progressCallback = new(OnDownloadProgress);
            if (isNew)
            {
                await operation.StartAsync().AsTask(cts.Token, progressCallback);
            }
            else
            {
                await operation.AttachAsync().AsTask(cts.Token, progressCallback);
            }

            await HandleDownloadedFile(item);
            await RemoveFromList(item);
        }
        catch (TaskCanceledException)
        {
            if (operation.ResultFile is not null)
            {
                await operation.ResultFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            await RemoveFromList(item);
        }
        catch (Exception ex)
        {
            Exception exception;
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);

            if (error != WebErrorStatus.Unknown)
            {
                HttpRequestException httpRequestException = new(error.ToString(), ex);
                exception = httpRequestException;
            }
            else
            {
                exception = ex;
            }

            item.ErrorException = exception;
            item.State = DownloadItemState.Error;
        }
        finally
        {
            // 防止临时文件残留

            StorageFile musicFile = (StorageFile)operation.ResultFile;
            string albumFolderPath = Path.GetDirectoryName(musicFile.Path);
            string infoFilePath = Path.Combine(albumFolderPath, $"{musicFile.DisplayName}.json.tmp");

            if (File.Exists(infoFilePath))
            {
                StorageFile infoFile = await StorageFile.GetFileFromPathAsync(infoFilePath);
                await infoFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
    }

    private static async Task HandleDownloadedFile(DownloadItem dlItem)
    {
        StorageFile sourceFile = (StorageFile)dlItem.Operation.ResultFile;
        await sourceFile.RenameAsync(sourceFile.Name.Replace(".tmp", string.Empty), NameCollisionOption.ReplaceExisting);

        if (sourceFile.ContentType.Contains("audio"))
        {
            if (TranscodeDownloadedItem)
            {
                dlItem.Progress = 0d;
                dlItem.State = DownloadItemState.Transcoding;

                MediaEncodingProfile profile = GetEncodingProfile();

                StorageFolder destinationFolder = await sourceFile.GetParentAsync();
                string desiredName = sourceFile.Name.Replace(sourceFile.FileType, $".{profile.Audio.Subtype.ToLower()}");
                StorageFile destinationFile = await destinationFolder.CreateFileAsync(desiredName, CreationCollisionOption.ReplaceExisting);

                await TranscodeFile(sourceFile, destinationFile, profile, dlItem);
                await WriteTagsToFile(destinationFile, dlItem);

                if (KeepWavFileAfterTranscode != true)
                {
                    await sourceFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            else
            {
                await WriteTagsToFile(sourceFile, dlItem);
            }
        }

        dlItem.State = DownloadItemState.Done;
    }

    private static MediaEncodingProfile GetEncodingProfile()
    {
        if (TranscodeEncoderInfo is null)
        {
            throw new InvalidOperationException($"{TranscodeEncoderInfo} 为 null");
        }
        else if (TranscodeEncoderInfo.Kind != CodecKind.Audio || TranscodeEncoderInfo.Category != CodecCategory.Encoder)
        {
            throw new InvalidOperationException($"{TranscodeEncoderInfo} 不是音频编码器");
        }
        else if (TranscodeQuality == AudioEncodingQuality.Auto)
        {
            throw new InvalidOperationException($"{TranscodeQuality} 被设为 '{AudioEncodingQuality.Auto}'");
        }

        IReadOnlyList<string> subtypes = TranscodeEncoderInfo.Subtypes;
        if (subtypes.Any(IsTargetEncoder(CodecSubtypes.AudioFormatMP3)))
        {
            return MediaEncodingProfile.CreateMp3(TranscodeQuality);
        }
        else if (subtypes.Any(IsTargetEncoder(CodecSubtypes.AudioFormatFlac)))
        {
            return MediaEncodingProfile.CreateFlac(TranscodeQuality);
        }

        throw new NotImplementedException("尚未实现对目标编码器的支持");

        static Func<string, bool> IsTargetEncoder(string targetEncoderGuid)
        {
            return encoderGuid => encoderGuid == targetEncoderGuid;
        }
    }

    private static async Task WriteTagsToFile(StorageFile musicFile, DownloadItem dlItem)
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

            if (musicFile.ContentType == "audio/wav")
            {
                // 给 WAV 写音乐信息会出现奇奇怪怪的问题
                await infoFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                return;
            }

            dlItem.State = DownloadItemState.WritingTag;

            using Stream infoFileStream = await infoFile.OpenStreamForReadAsync();
            SongDetailAndAlbumDetailPack pack = await JsonSerializer.DeserializeAsync<SongDetailAndAlbumDetailPack>(infoFileStream);
            AlbumDetail albumDetail = pack.AlbumDetail;
            SongDetail songDetail = pack.SongDetail;
            using UwpStorageFileAbstraction uwpStorageFile = new(musicFile);
            using TagLib.File file = TagLib.File.Create(uwpStorageFile);
            IRandomAccessStream coverStream = await FileCacheHelper.GetAlbumCoverStreamAsync(albumDetail);

            try
            {
                List<SongInfo> songs = albumDetail.Songs.ToList();
                file.Tag.Performers = songDetail.Artists.Any() ? songDetail.Artists.ToArray() : ["MSR".GetLocalized()];
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

    private static async Task TranscodeFile(IStorageFile sourceFile, IStorageFile destinationFile, MediaEncodingProfile profile, DownloadItem dlItem)
    {
        MediaTranscoder transcoder = new();
        PrepareTranscodeResult prepareOp = await transcoder.PrepareFileTranscodeAsync(sourceFile, destinationFile, profile);

        if (prepareOp.CanTranscode)
        {
            Progress<double> progressCallback = new(progress =>
            {
                dlItem.Progress = progress / 100;
            });

            await prepareOp.TranscodeAsync().AsTask(dlItem.CancelToken.Token, progressCallback);
        }
        else
        {
            throw new InvalidOperationException($"转码操作失败，原因：{prepareOp.FailureReason}");
        }
    }

    private static async Task<bool> IsFolderExist(string dlPath)
    {
        try
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(dlPath);
            return folder != null;
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException or ArgumentException)
        {
            return false;
        }
    }
}

// Source: https://github.com/HyPlayer/HyPlayer/blob/8f7cd2c26c4176353a95ea37e671cc710a7e4dd3/HyPlayer/HyPlayControl/DownloadManager.cs#L552
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
