using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为应用程序数据提供文件中缓存的类
/// </summary>
internal static class FileCacheHelper
{
    private const string DefaultAlbumCoverCacheFolderName = "AlbumCover";
    private const string DefaultMusicInfoCacheFolderName = "MusicInfo";
    private const string DefaultSongDurationCacheFolderName = "SongDuration";
    private static readonly StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;

    /// <summary>
    /// 使用指定的 <see cref="AlbumDetail"/> 实例，在专辑封面缓存文件夹创建专辑封面文件
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    public static async Task StoreAlbumCoverAsync(AlbumDetail albumDetail)
    {
        await StoreAlbumByUriAndCid(albumDetail.CoverUrl, albumDetail.Cid);
    }

    /// <summary>
    /// 使用指定的 <see cref="AlbumInfo"/> 实例，在专辑封面缓存文件夹创建专辑封面文件
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例</param>
    public static async Task StoreAlbumCoverAsync(AlbumInfo albumInfo)
    {
        await StoreAlbumByUriAndCid(albumInfo.CoverUrl, albumInfo.Cid);
    }

    /// <summary>
    /// 使用指定的 Uri 字符串与 CID 字符串，在专辑封面缓存文件夹创建专辑封面文件
    /// </summary>
    /// <param name="uri">专辑封面的 Uri</param>
    /// <param name="cid">专辑的 CID</param>
    public static async Task StoreAlbumByUriAndCid(string uri, string cid)
    {
        SemaphoreSlim semaphore = LockerHelper<string>.GetOrCreateLocker(cid);

        try
        {
            await semaphore.WaitAsync();
            Uri coverUri = new(uri, UriKind.Absolute);

            try
            {
                string fileName = $"{cid}.jpg";
                StorageFolder coverFolder = await tempFolder.CreateFolderAsync(DefaultAlbumCoverCacheFolderName, CreationCollisionOption.OpenIfExists);
                IStorageItem coverFile = await coverFolder.TryGetItemAsync(fileName);

                if (coverFile is not null && coverFile.IsOfType(StorageItemTypes.File))
                {
                    BasicProperties props = await coverFile.GetBasicPropertiesAsync();

                    if (props.Size != 0)
                    {
                        return;
                    }
                }

                using HttpClient httpClient = new();
                using HttpResponseMessage result = await httpClient.GetAsync(coverUri);

                using InMemoryRandomAccessStream stream = new();
                await result.Content.WriteToStreamAsync(stream);
                stream.Seek(0);

                StorageFile file = await coverFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                using StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync();
                await RandomAccessStream.CopyAsync(stream, transaction.Stream);
                await transaction.CommitAsync();
            }
            catch (COMException)
            {
                return;
            }
        }
        finally
        {
            semaphore.Release();
            LockerHelper<string>.ReturnLocker(cid);
        }
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumDetail"/> 实例获取专辑封面的随机访问流
    /// </summary>
    /// <param name="albumDetail"><see cref="AlbumDetail"/> 的实例</param>
    /// <returns>包含专辑封面数据的 <see cref="IRandomAccessStream"/></returns>
    public static async Task<IRandomAccessStream> GetAlbumCoverStreamAsync(AlbumDetail albumDetail)
    {
        string fileName = $"{albumDetail.Cid}.jpg";
        return await GetAlbumCoverStreamAsync(fileName);
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumInfo"/> 实例获取专辑封面的随机访问流
    /// </summary>
    /// <param name="albumInfo"><see cref="AlbumInfo"/> 的实例</param>
    /// <returns>包含专辑封面数据的 <see cref="IRandomAccessStream"/></returns>
    public static async Task<IRandomAccessStream> GetAlbumCoverStreamAsync(AlbumInfo albumInfo)
    {
        string fileName = $"{albumInfo.Cid}.jpg";
        return await GetAlbumCoverStreamAsync(fileName);
    }

    /// <summary>
    /// 通过指定的文件名获取专辑封面的随机访问流
    /// </summary>
    /// <param name="fileName">专辑封面的文件名</param>
    /// <returns>包含专辑封面数据的 <see cref="IRandomAccessStream"/></returns>
    private static async Task<IRandomAccessStream> GetAlbumCoverStreamAsync(string fileName)
    {
        StorageFolder coverFolder = await tempFolder.CreateFolderAsync(DefaultAlbumCoverCacheFolderName, CreationCollisionOption.OpenIfExists);

        if (coverFolder != null)
        {
            StorageFile file = await coverFolder.GetFileAsync(fileName);
            return await file?.OpenReadAsync();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumDetail"/> 实例获取指向专辑封面的 Uri
    /// </summary>
    /// <param name="albumDetail"><see cref="AlbumDetail"/> 的实例</param>
    /// <returns>指向专辑封面的 <see cref="Uri"/></returns>
    public static async Task<Uri> GetAlbumCoverUriAsync(AlbumDetail albumDetail)
    {
        return await GetAlbumCoverUriAsync(albumDetail.Cid);
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumInfo"/> 实例获取指向专辑封面的 Uri
    /// </summary>
    /// <param name="albumInfo"><see cref="AlbumInfo"/> 的实例</param>
    /// <returns>指向专辑封面的 <see cref="Uri"/></returns>
    public static async Task<Uri> GetAlbumCoverUriAsync(AlbumInfo albumInfo)
    {
        return await GetAlbumCoverUriAsync(albumInfo.Cid);
    }

    /// <summary>
    /// 通过指定的专辑 CID 获取指向专辑封面的 Uri
    /// </summary>
    /// <param name="cid">专辑的 CID</param>
    /// <returns>指向专辑封面的 <see cref="Uri"/></returns>
    public static async Task<Uri> GetAlbumCoverUriAsync(string cid)
    {
        try
        {
            string fileName = $"{cid}.jpg";

            StorageFolder coverFolder = await tempFolder.CreateFolderAsync(DefaultAlbumCoverCacheFolderName, CreationCollisionOption.OpenIfExists);

            if (coverFolder != null && await coverFolder.FileExistsAsync(fileName))
            {
                StorageFile file = await coverFolder.GetFileAsync(fileName);
                BasicProperties fileBasicProps = await file.GetBasicPropertiesAsync();

                if (fileBasicProps.Size != 0)
                {
                    return new Uri($"ms-appdata:///temp/{DefaultAlbumCoverCacheFolderName}/{fileName}", UriKind.Absolute);
                }
                else
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }

                // 移除下载事务操作遗留下来的临时文件
                if (await coverFolder.FileExistsAsync($"{file.Name}.~tmp"))
                {
                    StorageFile tempFile = await coverFolder.GetFileAsync($"{file.Name}.~tmp");
                    await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException or IOException)
        {
            // ;-)
        }

        return null;
    }

    /// <summary>
    /// 从缓存中获取歌曲时长
    /// </summary>
    /// <param name="songCid">歌曲 CID</param>
    /// <returns>表示歌曲时长的 <see cref="System.TimeSpan"/>。如果缓存中不存在指定项的时长信息，则返回 <see langword="null"/></returns>
    public static async Task<TimeSpan?> GetSongDurationAsync(string songCid)
    {
        StorageFolder durationFolder = await tempFolder.CreateFolderAsync(DefaultSongDurationCacheFolderName, CreationCollisionOption.OpenIfExists);

        string fileName = $"{songCid}.json";
        if (durationFolder != null && await durationFolder.FileExistsAsync(fileName))
        {
            try
            {
                StorageFile file = await durationFolder.GetFileAsync(fileName);
                using Stream utf8Json = await file.OpenStreamForReadAsync();

                TimeSpan duration = JsonSerializer.Deserialize<TimeSpan>(utf8Json);
                return duration;
            }
            catch (JsonException)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 通过指定的参数，将歌曲的时长信息保存在缓存文件夹中
    /// </summary>
    /// <param name="songCid">歌曲的 CID</param>
    /// <param name="timeSpan">歌曲时长</param>
    public static async Task StoreSongDurationAsync(string songCid, TimeSpan timeSpan)
    {
        StorageFolder durationFolder = await tempFolder.CreateFolderAsync(DefaultSongDurationCacheFolderName, CreationCollisionOption.OpenIfExists);
        StorageFile file = await durationFolder.CreateFileAsync($"{songCid}.json", CreationCollisionOption.ReplaceExisting);

        using Stream stream = await file.OpenStreamForWriteAsync();
        await JsonSerializer.SerializeAsync(stream, timeSpan);
    }

    [Obsolete("暂时不使用此方法")]
    public static async Task StoreAlbumInfoAsync(AlbumInfo info)
    {
        StorageFolder musicInfoFolder = await tempFolder.CreateFolderAsync(DefaultMusicInfoCacheFolderName, CreationCollisionOption.OpenIfExists);
        StorageFile file = await musicInfoFolder.CreateFileAsync($"{info.Cid}.json", CreationCollisionOption.ReplaceExisting);

        using StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync();
        using Stream utf8Json = transaction.Stream.AsStreamForWrite();
        await JsonSerializer.SerializeAsync(utf8Json, info);
        await transaction.CommitAsync();
    }
}
