using System.Net.Http;
using System.Text.Json;
using System.Threading;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
using Windows.Media.Playback;
using Windows.Storage;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 为歌曲、专辑等内容的收藏提供服务的类。
/// </summary>
public static class FavoriteService
{
    private const string DefaultFavoriteListFolderName = "Favorite";
    private const string DefaultSongFavoriteListFileName = "songs.json";
    private const string DefaultAlbumFavoriteListFileName = "albums.json";

    private static readonly StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
    private static StorageFile songFavoriteFile;
    private static StorageFile albumFavoriteFile;

    private static readonly SemaphoreSlim songFavoriteFileSemaphore = new(1);
    private static readonly SemaphoreSlim albumFavoriteFileSemaphore = new(1);

    public static SongFavoriteList SongFavoriteList { get; private set; }
    public static AlbumFavoriteList AlbumFavoriteList { get; private set; }

    /// <summary>
    /// 初始化收藏服务。
    /// </summary>
    public static async Task Initialize()
    {
        await songFavoriteFileSemaphore.WaitAsync();
        await albumFavoriteFileSemaphore.WaitAsync();

        try
        {
            // TODO: 添加收藏备份功能。
            SongFavoriteList = await InitializeFavoriteList<SongFavoriteList>(FavoriteType.Song);
            AlbumFavoriteList = await InitializeFavoriteList<AlbumFavoriteList>(FavoriteType.Album);
        }
        finally
        {
            songFavoriteFileSemaphore.Release();
            albumFavoriteFileSemaphore.Release();
        }
    }

    /// <summary>
    /// 确定指定的 <see cref="SongFavoriteItem"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="item">指定的 <see cref="SongFavoriteItem"/> 实例。</param>
    /// <returns>指示收藏项是否包含在收藏夹中的值。</returns>
    public static bool ContainsItem(SongFavoriteItem item) => SongFavoriteList.Items.Contains(item);

    /// <summary>
    /// 确定指定的 <see cref="AlbumFavoriteItem"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="item">指定的 <see cref="AlbumFavoriteItem"/> 实例。</param>
    /// <returns>指示收藏项是否包含在收藏夹中的值。</returns>
    public static bool ContainsItem(AlbumFavoriteItem item) => AlbumFavoriteList.Items.Contains(item);

    /// <summary>
    /// 确定 CID 所表示的歌曲是否包含在收藏夹中。
    /// </summary>
    /// <param name="songCid">歌曲 CID。</param>
    /// <returns>指示歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsSong(string songCid)
        => SongFavoriteList.Items.Any(item => item.SongCid == songCid);

    /// <summary>
    /// 确定 CID 所表示的专辑是否包含在收藏夹中。
    /// </summary>
    /// <param name="albumCid">专辑 CID。</param>
    /// <returns>指示专辑是否包含在收藏夹中的值。</returns>
    public static bool ContainsAlbum(string albumCid)
        => AlbumFavoriteList.Items.Any(item => item.AlbumCid == albumCid);

    /// <summary>
    /// 确定指定的 <see cref="AlbumDetail"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="albumDetail">指定的 <see cref="AlbumDetail"/> 实例。</param>
    /// <returns>指示专辑是否包含在收藏夹中的值。</returns>
    public static bool ContainsAlbum(AlbumDetail albumDetail) => ContainsAlbum(albumDetail.Cid);

    /// <summary>
    /// 确定指定的 <see cref="AlbumInfo"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="albumInfo">指定的 <see cref="AlbumInfo"/> 实例。</param>
    /// <returns>指示专辑是否包含在收藏夹中的值。</returns>
    public static bool ContainsAlbum(AlbumInfo albumInfo) => ContainsAlbum(albumInfo.Cid);

    /// <summary>
    /// 确定指定的 <see cref="SongInfo"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="songInfo">指定的 <see cref="SongInfo"/> 实例。</param>
    /// <returns>指示指定歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsSong(SongInfo songInfo) => ContainsSong(songInfo.Cid);

    /// <summary>
    /// 确定指定的 <see cref="SongDetail"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="songInfo">指定的 <see cref="SongDetail"/> 实例。</param>
    /// <returns>指示指定歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsSong(SongDetail songDetail) => ContainsSong(songDetail.Cid);

    /// <summary>
    /// 确定指定的 <see cref="PlaylistItem"/> 所表示的歌曲是否包含在收藏夹中。
    /// </summary>
    /// <param name="playlistItem">指定的 <see cref="PlaylistItem"/> 实例。</param>
    /// <returns>指示指定歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsSong(PlaylistItem playlistItem) => ContainsSong(playlistItem.SongCid);

    /// <summary>
    /// 保存收藏列表。
    /// </summary>
    /// <param name="favoriteType">要保存收藏内容的类型。</param>
    public static async Task SaveFavoriteList(FavoriteType favoriteType)
    {
        SemaphoreSlim semaphore = favoriteType switch
        {
            FavoriteType.Song => songFavoriteFileSemaphore,
            FavoriteType.Album => albumFavoriteFileSemaphore,
            _ => throw GetFavoriteTypeNotImplementedException()
        };

        await semaphore.WaitAsync();

        try
        {
            StorageFile file = await GetFavoriteListFile(favoriteType);
            using StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync();
            Stream fileStream = transaction.Stream.AsStream();
            fileStream.SetLength(0);
            fileStream.Seek(0, SeekOrigin.Begin);

            object value = favoriteType switch
            {
                FavoriteType.Song => SongFavoriteList,
                FavoriteType.Album => AlbumFavoriteList,
                _ => throw new NotImplementedException("尚未实现指定的收藏内容。")
            };
            await JsonSerializer.SerializeAsync(fileStream, value);

            fileStream.Seek(0, SeekOrigin.Begin);
            await transaction.CommitAsync();
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// 将 <see cref="SongFavoriteItem"/> 添加到歌曲收藏夹。
    /// </summary>
    /// <param name="item">一个 <see cref="SongFavoriteItem"/> 实例。</param>
    public static async Task AddSongToFavoriteAsync(SongFavoriteItem item)
        => await AddItemToFavoriteAsync(item, FavoriteType.Song);

    /// <summary>
    /// 将 <see cref="AlbumFavoriteItem"/> 添加到专辑收藏夹。
    /// </summary>
    /// <param name="item">一个 <see cref="AlbumFavoriteItem"/> 实例。</param>
    public static async Task AddAlbumToFavoriteAsync(AlbumFavoriteItem item)
        => await AddItemToFavoriteAsync(item, FavoriteType.Album);

    /// <summary>
    /// 将 <see cref="SongFavoriteItem"/> 序列添加到歌曲收藏夹。
    /// </summary>
    /// <param name="items"><see cref="SongFavoriteItem"/> 序列。</param>
    public static async Task AddSongsToFavoriteAsync(IAsyncEnumerable<SongFavoriteItem> items)
        => await AddItemsToFavoriteAsync(items, FavoriteType.Song);

    /// <summary>
    /// 将 <see cref="AlbumFavoriteItem"/> 序列添加到专辑收藏夹。
    /// </summary>
    /// <param name="items"><see cref="AlbumFavoriteItem"/> 序列。</param>
    public static async Task AddAlbumsToFavoriteAsync(IAsyncEnumerable<AlbumFavoriteItem> items)
        => await AddItemsToFavoriteAsync(items, FavoriteType.Album);

    /// <summary>
    /// 从收藏夹移除专辑。
    /// </summary>
    public static async Task<bool> RemoveAlbumFromFavoriteAsync(string albumCid)
    {
        AlbumFavoriteItem target = AlbumFavoriteList.Items.FirstOrDefault(item => item.AlbumCid == albumCid);
        if (target.Equals(default)) return false;

        return await RemoveAlbumFromFavoriteAsync(target);
    }

    /// <summary>
    /// 从歌曲收藏夹移除歌曲。
    /// </summary>
    /// <param name="songCid">歌曲的 CID。</param>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    /// <returns>指示是否成功移除歌曲的值。</returns>
    public static async Task<bool> RemoveSongFromFavoriteAsync(string songCid)
    {
        SongFavoriteItem target = SongFavoriteList.Items.FirstOrDefault(item => item.SongCid == songCid);
        if (target.Equals(default)) return false;

        return await RemoveSongFromFavoriteAsync(target);
    }

    /// <summary>
    /// 从歌曲收藏夹移除歌曲。
    /// </summary>
    /// <param name="item">指定的 <see cref="SongFavoriteItem"/>。</param>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    /// <returns>指示是否成功移除歌曲的值。</returns>
    public static async Task<bool> RemoveSongFromFavoriteAsync(SongFavoriteItem item)
        => await UIThreadHelper.RunOnUIThread(() => SongFavoriteList.Items.Remove(item));

    /// <summary>
    /// 从收藏夹移除专辑。
    /// </summary>
    public static async Task<bool> RemoveAlbumFromFavoriteAsync(AlbumFavoriteItem item)
        => await UIThreadHelper.RunOnUIThread(() => AlbumFavoriteList.Items.Remove(item));

    /// <summary>
    /// 从歌曲收藏夹移除歌曲序列。
    /// </summary>
    /// <param name="items">歌曲序列。</param>
    public static async Task RemoveSongsFromFavoriteAsync(IAsyncEnumerable<SongFavoriteItem> items)
        => await RemoveItemsFromFavoriteAsync(items, FavoriteType.Song);

    /// <summary>
    /// 批量移除专辑。
    /// </summary>
    /// <param name="items">专辑序列。</param>
    public static async Task RemoveAlbumsFromFavoriteAsync(IAsyncEnumerable<AlbumFavoriteItem> items)
        => await RemoveItemsFromFavoriteAsync(items, FavoriteType.Album);

    /// <summary>
    /// 从歌曲收藏夹移除歌曲序列。
    /// </summary>
    /// <param name="songCids">歌曲 CID 序列。</param>
    public static async Task RemoveSongsFromFavoriteAsync(IAsyncEnumerable<string> songCids)
        => await RemoveItemsFromFavoriteAsync<SongFavoriteItem>(songCids, FavoriteType.Song);

    /// <summary>
    /// 批量移除专辑。
    /// </summary>
    /// <param name="albumCids">专辑 CID 序列。</param>
    public static async Task RemoveAlbumsFromFavoriteAsync(IAsyncEnumerable<string> albumCids)
        => await RemoveItemsFromFavoriteAsync<AlbumFavoriteItem>(albumCids, FavoriteType.Album);

    /// <summary>
    /// 播放收藏夹中的歌曲。
    /// </summary>
    /// <param name="favoriteType">收藏内容类型。</param>
    /// <exception cref="AggregateException">包含一个或多个异常信息的 <see cref="AggregateException"/>。</exception>
    public static async Task PlayFavoriteListAsync(FavoriteType favoriteType)
    {
        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = GetFavoriteListMediaPlaybackItems(favoriteType, box);
        await MusicService.ReplaceMusic(items);
        box.Unbox();
    }

    /// <summary>
    /// 将收藏夹添加到正在播放列表中。
    /// </summary>
    /// <param name="favoriteType">收藏内容类型。</param>
    /// <exception cref="AggregateException">包含一个或多个异常信息的 <see cref="AggregateException"/>。</exception>
    public static async Task AddFavoriteListToNowPlayingAsync(FavoriteType favoriteType)
    {
        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = GetFavoriteListMediaPlaybackItems(favoriteType, box);
        await MusicService.AddMusic(items);
        box.Unbox();
    }

    /// <summary>
    /// 将收藏夹设为下一项播放。
    /// </summary>
    /// <param name="favoriteType">收藏内容类型。</param>
    /// <exception cref="AggregateException">包含一个或多个异常信息的 <see cref="AggregateException"/>。</exception>
    public static async Task PlayNextForFavoriteListAsync(FavoriteType favoriteType)
    {
        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = GetFavoriteListMediaPlaybackItems(favoriteType, box);
        await MusicService.PlayNext(items);
        box.Unbox();
    }

    private static async Task<T> InitializeFavoriteList<T>(FavoriteType favoriteType) where T : new()
    {
        StorageFile file = await GetFavoriteListFile(favoriteType);
        using StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync();
        Stream fileStream = transaction.Stream.AsStream();
        fileStream.Seek(0, SeekOrigin.Begin);

        T list;

        if (fileStream.Length == 0)
        {
            list = await CreateAndWriteNewList<T>(fileStream);
        }
        else
        {
            try
            {
                list = await JsonSerializer.DeserializeAsync<T>(fileStream)
                    ?? await CreateAndWriteNewList<T>(fileStream);
            }
            catch (JsonException)
            {
                list = await CreateAndWriteNewList<T>(fileStream);
            }
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        await transaction.CommitAsync();

        return list;
    }

    private async static Task<T> CreateAndWriteNewList<T>(Stream stream) where T : new()
    {
        T target = new();

        stream.SetLength(0);
        stream.Seek(0, SeekOrigin.Begin);
        await JsonSerializer.SerializeAsync(stream, target);

        return target;
    }

    private static async Task<StorageFile> GetFavoriteListFile(FavoriteType type)
    {
        string fileName;
        StorageFile targetFile;

        switch (type)
        {
            case FavoriteType.Song:
                fileName = DefaultSongFavoriteListFileName;
                targetFile = songFavoriteFile;
                break;
            case FavoriteType.Album:
                fileName = DefaultAlbumFavoriteListFileName;
                targetFile = albumFavoriteFile;
                break;
            default:
                throw GetFavoriteTypeNotImplementedException();
        }

        if (targetFile is null)
        {
            StorageFolder folder = await localCacheFolder.CreateFolderAsync(DefaultFavoriteListFolderName, CreationCollisionOption.OpenIfExists);
            IStorageItem storageItem = await folder.TryGetItemAsync(fileName);

            targetFile = storageItem is StorageFile file
                ? file
                : await folder.CreateFileAsync(fileName);
        }

        switch (type)
        {
            case FavoriteType.Song:
                songFavoriteFile = targetFile;
                break;
            case FavoriteType.Album:
                albumFavoriteFile = targetFile;
                break;
            default:
                throw GetFavoriteTypeNotImplementedException();
        }

        return targetFile;
    }

    private static IAsyncEnumerable<MediaPlaybackItem> GetFavoriteListMediaPlaybackItems(FavoriteType favoriteType, ExceptionBox box)
    {
        return favoriteType switch
        {
            FavoriteType.Song => CommonValues.GetMediaPlaybackItems(SongFavoriteList, box),
            FavoriteType.Album => CommonValues.GetMediaPlaybackItems(AlbumFavoriteList, box),
            _ => throw GetFavoriteTypeNotImplementedException()
        };
    }

    /// <summary>
    /// 获得一个收藏内容类型尚未实现的异常。
    /// </summary>
    /// <returns>返回一个表示收藏内容类型尚未实现的 <see cref="NotImplementedException"/> 异常。</returns>
    private static NotImplementedException GetFavoriteTypeNotImplementedException()
        => new("尚未实现指定的收藏内容。");

    private static FavoriteList<T> GetFavoriteList<T>(FavoriteType favoriteType)
    {
        return favoriteType switch
        {
            FavoriteType.Song => SongFavoriteList as FavoriteList<T>,
            FavoriteType.Album => AlbumFavoriteList as FavoriteList<T>,
            _ => throw GetFavoriteTypeNotImplementedException()
        };
    }

    private static async Task RemoveItemsFromFavoriteAsync<T>(IAsyncEnumerable<T> items, FavoriteType favoriteType)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        FavoriteList<T> favoriteList = GetFavoriteList<T>(favoriteType);

        try
        {
            favoriteList.BlockInfoUpdate();

            await UIThreadHelper.RunOnUIThread(async () =>
            {
                await foreach (T item in items)
                {
                    favoriteList.Items.Remove(item);
                }
            });
        }
        finally
        {
            await favoriteList.RestoreInfoUpdateAsync();
        }
    }

    private static async Task RemoveItemsFromFavoriteAsync<T>(IAsyncEnumerable<string> cids, FavoriteType favoriteType)
    {
        if (cids is null)
        {
            throw new ArgumentNullException(nameof(cids));
        }

        FavoriteList<T> favoriteList = GetFavoriteList<T>(favoriteType);

        try
        {
            favoriteList.BlockInfoUpdate();

            await UIThreadHelper.RunOnUIThread(async () =>
            {
                await foreach (string cid in cids)
                {
                    switch (favoriteType)
                    {
                        case FavoriteType.Song:
                            await RemoveSongFromFavoriteAsync(cid);
                            break;
                        case FavoriteType.Album:
                            await RemoveAlbumFromFavoriteAsync(cid);
                            break;
                        default:
                            throw GetFavoriteTypeNotImplementedException();
                    }
                }
            });
        }
        finally
        {
            await favoriteList.RestoreInfoUpdateAsync();
        }
    }

    private static async Task AddItemToFavoriteAsync<T>(T item, FavoriteType favoriteType)
    {
        FavoriteList<T> favoriteList = GetFavoriteList<T>(favoriteType);

        if (favoriteList.Items.Contains(item))
        {
            return;
        }

        await UIThreadHelper.RunOnUIThread(() =>
        {
            favoriteList.Items.Add(item);
        });
    }

    private static async Task AddItemsToFavoriteAsync<T>(IAsyncEnumerable<T> items, FavoriteType favoriteType)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        FavoriteList<T> favoriteList = GetFavoriteList<T>(favoriteType);

        try
        {
            favoriteList.BlockInfoUpdate();
            await UIThreadHelper.RunOnUIThread(async () =>
            {
                await foreach (T item in items)
                {
                    if (favoriteList.Items.Contains(item))
                    {
                        continue;
                    }
                    favoriteList.Items.Add(item);
                }
            });
        }
        finally
        {
            await favoriteList.RestoreInfoUpdateAsync();
        }
    }
}
