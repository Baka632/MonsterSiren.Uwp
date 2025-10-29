using System.Net.Http;
using System.Text.Json;
using System.Threading;
using MonsterSiren.Uwp.Models.Favorites;
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

    private static readonly SemaphoreSlim favoriteFileSemaphore = new(1);

    public static SongFavoriteList SongFavoriteList { get; private set; }

    public static async Task Initialize()
    {
        await favoriteFileSemaphore.WaitAsync();

        try
        {
            // TODO: 加上专辑
            if (SongFavoriteList is null)
            {
                await InitializeSongFavoriteList();
            }
        }
        finally
        {
            favoriteFileSemaphore.Release();
        }
    }

    private static async Task InitializeSongFavoriteList()
    {
        StorageFile file = await GetSongFavoriteListFile();
        using StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync();
        Stream fileStream = transaction.Stream.AsStream();
        fileStream.Seek(0, SeekOrigin.Begin);

        SongFavoriteList list;

        if (fileStream.Length == 0)
        {
            list = await CreateAndWriteNewList(fileStream);
        }
        else
        {
            try
            {
                list = await JsonSerializer.DeserializeAsync<SongFavoriteList>(fileStream)
                    ?? await CreateAndWriteNewList(fileStream);
            }
            catch (JsonException)
            {
                list = await CreateAndWriteNewList(fileStream);
            }
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        await transaction.CommitAsync();

        SongFavoriteList = list;

        async static Task<SongFavoriteList> CreateAndWriteNewList(Stream stream)
        {
            SongFavoriteList songFavorites = new();

            stream.SetLength(0);
            stream.Seek(0, SeekOrigin.Begin);
            await JsonSerializer.SerializeAsync(stream, songFavorites);

            return songFavorites;
        }
    }

    /// <summary>
    /// 确定 CID 所表示的歌曲是否包含在收藏夹中。
    /// </summary>
    /// <param name="songCid">歌曲 CID。</param>
    /// <returns>指示歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsSong(string songCid)
    {
        return SongFavoriteList.Items.Any(item => item.SongCid == songCid);
    }

    /// <summary>
    /// 确定指定的 <see cref="SongInfo"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="songInfo">指定的 <see cref="SongInfo"/> 实例。</param>
    /// <returns>指示指定歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsItem(SongInfo songInfo)
    {
        return ContainsSong(songInfo.Cid);
    }

    /// <summary>
    /// 确定指定的 <see cref="SongDetail"/> 是否包含在收藏夹中。
    /// </summary>
    /// <param name="songInfo">指定的 <see cref="SongDetail"/> 实例。</param>
    /// <returns>指示指定歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsItem(SongDetail songDetail)
    {
        return ContainsSong(songDetail.Cid);
    }

    /// <summary>
    /// 确定指定的 <see cref="PlaylistItem"/> 所表示的歌曲是否包含在收藏夹中。
    /// </summary>
    /// <param name="playlistItem">指定的 <see cref="PlaylistItem"/> 实例。</param>
    /// <returns>指示指定歌曲是否包含在收藏夹中的值。</returns>
    public static bool ContainsItem(PlaylistItem playlistItem)
    {
        return ContainsSong(playlistItem.SongCid);
    }

    /// <summary>
    /// 保存歌曲收藏列表。
    /// </summary>
    public static async Task SaveSongFavoriteList()
    {
        await favoriteFileSemaphore.WaitAsync();

        try
        {
            StorageFile file = await GetSongFavoriteListFile();
            using StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync();
            Stream fileStream = transaction.Stream.AsStream();
            fileStream.SetLength(0);
            fileStream.Seek(0, SeekOrigin.Begin);

            await JsonSerializer.SerializeAsync(fileStream, SongFavoriteList);

            fileStream.Seek(0, SeekOrigin.Begin);
            await transaction.CommitAsync();
        }
        finally
        {
            favoriteFileSemaphore.Release();
        }
    }

    private static async Task<StorageFile> GetSongFavoriteListFile()
    {
        if (songFavoriteFile is null)
        {
            StorageFolder folder = await localCacheFolder.CreateFolderAsync(DefaultFavoriteListFolderName, CreationCollisionOption.OpenIfExists);
            IStorageItem storageItem = await folder.TryGetItemAsync(DefaultSongFavoriteListFileName);

            if (storageItem is StorageFile file)
            {
                songFavoriteFile = file;
            }
            else
            {
                songFavoriteFile = await folder.CreateFileAsync(DefaultSongFavoriteListFileName);
            }
        }

        return songFavoriteFile;
    }

    /// <summary>
    /// 向歌曲收藏夹添加歌曲。
    /// </summary>
    /// <param name="songDetail">表示歌曲详细信息的 <see cref="SongDetail"/> 实例。</param>
    /// <param name="albumDetail">表示歌曲所属专辑详细信息的 <see cref="AlbumDetail"/> 实例。</param>
    /// <exception cref="ArgumentException"><paramref name="songDetail"/> 中所属专辑的 CID 和 <paramref name="albumDetail"/> 中的 CID 不符。</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    public static async Task AddSongToFavoriteAsync(SongDetail songDetail, AlbumDetail albumDetail)
    {
        if (songDetail.AlbumCid != albumDetail.Cid)
        {
            throw new ArgumentException("歌曲信息中所属专辑的 CID 和专辑信息中的 CID 不符。");
        }

        if (ContainsItem(songDetail))
        {
            return;
        }

        TimeSpan? duration = await MsrModelsHelper.GetSongDurationAsync(songDetail);
        SongFavoriteItem item = new(songDetail.Cid, albumDetail.Cid, songDetail.Name, albumDetail.Name, duration ?? TimeSpan.Zero);

        await UIThreadHelper.RunOnUIThread(() =>
        {
            SongFavoriteList.Items.Add(item);
        });
    }

    /// <summary>
    /// 向歌曲收藏夹添加歌曲。
    /// </summary>
    /// <param name="playlistItem">表示播放列表项的 <see cref="PlaylistItem"/> 实例。</param>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    public static async Task AddSongToFavoriteAsync(PlaylistItem playlistItem)
    {
        SongFavoriteItem item = ToSongFavoriteItem(playlistItem);

        if (SongFavoriteList.Items.Contains(item))
        {
            return;
        }

        await UIThreadHelper.RunOnUIThread(() =>
        {
            SongFavoriteList.Items.Add(item);
        });
    }

    /// <summary>
    /// 向歌曲收藏夹添加歌曲序列。
    /// </summary>
    /// <param name="tuples">歌曲序列。</param>
    /// <exception cref="ArgumentNullException"><paramref name="tuples"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">歌曲序列中，存在某个 SongDetail 所属专辑的 CID 和专辑信息 AlbumDetail 中的 CID 不符的情况。</exception>
    public static async Task AddSongsToFavoriteAsync(IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> tuples)
    {
        if (tuples is null)
        {
            throw new ArgumentNullException(nameof(tuples));
        }

        try
        {
            SongFavoriteList.BlockInfoUpdate();

            await foreach ((SongDetail songDetail, AlbumDetail albumDetail) in tuples)
            {
                if (songDetail.AlbumCid != albumDetail.Cid)
                {
                    throw new ArgumentException($"歌曲信息中，{songDetail.Name} 所属专辑的 CID 和专辑信息 {albumDetail.Name} 中的 CID 不符。");
                }

                TimeSpan? duration = await MsrModelsHelper.GetSongDurationAsync(songDetail);
                SongFavoriteItem item = new(songDetail.Cid, albumDetail.Cid, songDetail.Name, albumDetail.Name, duration ?? TimeSpan.Zero);

                await UIThreadHelper.RunOnUIThread(() =>
                {
                    if (!SongFavoriteList.Items.Contains(item))
                    {
                        SongFavoriteList.Items.Add(item);
                    }
                });
            }
        }
        finally
        {
            await SongFavoriteList.RestoreInfoUpdateAsync();
        }
    }

    /// <summary>
    /// 向歌曲收藏夹添加播放列表序列。
    /// </summary>
    /// <param name="items">包含 <see cref="PlaylistItem"/> 项的集合。</param>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> 为 <see langword="null"/>。</exception>
    public static async Task AddSongsToFavoriteAsync(IEnumerable<PlaylistItem> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        try
        {
            SongFavoriteList.BlockInfoUpdate();
            await UIThreadHelper.RunOnUIThread(() =>
            {
                foreach (PlaylistItem playlistItem in items)
                {
                    SongFavoriteItem item = ToSongFavoriteItem(playlistItem);
                    if (SongFavoriteList.Items.Contains(item))
                    {
                        continue;
                    }

                    SongFavoriteList.Items.Add(item);
                }
            });
        }
        finally
        {
            await SongFavoriteList.RestoreInfoUpdateAsync();
        }
    }

    /// <summary>
    /// 从歌曲收藏夹移除歌曲。
    /// </summary>
    /// <param name="item">指定的 <see cref="SongFavoriteItem"/>。</param>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    /// <returns>指示是否成功移除歌曲的值。</returns>
    public static async Task<bool> RemoveSongFromFavoriteAsync(SongFavoriteItem item)
    {
        return await UIThreadHelper.RunOnUIThread(() => SongFavoriteList.Items.Remove(item));
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

        return await RemoveSongFromFavoriteAsync(target);
    }

    /// <summary>
    /// 从歌曲收藏夹移除歌曲序列。
    /// </summary>
    /// <param name="items">歌曲序列。</param>
    public static async Task RemoveSongsFromFavoriteAsync(IEnumerable<SongFavoriteItem> items)
    {
        try
        {
            SongFavoriteList.BlockInfoUpdate();

            await UIThreadHelper.RunOnUIThread(() =>
            {
                foreach (SongFavoriteItem item in items)
                {
                    SongFavoriteList.Items.Remove(item);
                }
            });
        }
        finally
        {
            await SongFavoriteList.RestoreInfoUpdateAsync();
        }
    }

    /// <summary>
    /// 播放歌曲收藏夹中的歌曲。
    /// </summary>
    public static async Task PlaySongFavoriteListAsync()
    {
        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(SongFavoriteList, box);
        await MusicService.ReplaceMusic(items);
        box.Unbox();
    }

    /// <summary>
    /// 将歌曲收藏夹添加到正在播放列表中。
    /// </summary>
    /// <exception cref="AggregateException">包含一个或多个异常信息的 <see cref="AggregateException"/>。</exception>
    public static async Task AddSongFavoriteListToNowPlayingAsync()
    {
        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(SongFavoriteList, box);
        await MusicService.AddMusic(items);
        box.Unbox();
    }

    /// <summary>
    /// 将歌曲收藏设为下一项播放。
    /// </summary>
    /// <exception cref="AggregateException">包含一个或多个异常信息的 <see cref="AggregateException"/>。</exception>
    public static async Task PlayNextForSongFavoriteListAsync()
    {
        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(SongFavoriteList, box);
        await MusicService.PlayNext(items);
        box.Unbox();
    }

    private static SongFavoriteItem ToSongFavoriteItem(PlaylistItem playlistItem)
    {
        return new(playlistItem.SongCid,
                   playlistItem.AlbumCid,
                   playlistItem.SongTitle,
                   playlistItem.AlbumTitle,
                   playlistItem.SongDuration);
    }
}
