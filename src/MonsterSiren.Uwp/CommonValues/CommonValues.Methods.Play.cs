using Windows.Media.Playback;
using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    #region IPlayable
    /// <summary>
    /// 播放 <see cref="IPlayable"/> 所表示的内容。
    /// </summary>
    /// <param name="playable">可播放对象的实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(IPlayable playable)
        => await WorkOnNowPlayingAsync(playable, MusicPlayOperation.Replace);

    /// <summary>
    /// 将 <see cref="IPlayable"/> 所表示的内容加入到正在播放列表中。
    /// </summary>
    /// <param name="playable">可播放对象的实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(IPlayable playable)
        => await WorkOnNowPlayingAsync(playable, MusicPlayOperation.Add);

    /// <summary>
    /// 将 <see cref="IPlayable"/> 所表示的内容设为下一项播放。
    /// </summary>
    /// <param name="playable">可播放对象的实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNext(IPlayable playable)
        => await WorkOnNowPlayingAsync(playable, MusicPlayOperation.AddNext);
    #endregion

    #region Playlist
    /// <summary>
    /// 播放 <see cref="Playlist"/> 所表示的播放列表。
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(Playlist playlist)
        => await WorkOnNowPlayingAsync(playlist, MusicPlayOperation.Replace);

    /// <summary>
    /// 将一个 <see cref="Playlist"/> 添加到正在播放列表中。
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(Playlist playlist)
        => await WorkOnNowPlayingAsync(playlist, MusicPlayOperation.Add);

    /// <summary>
    /// 将一个 <see cref="Playlist"/> 设为下一项播放。
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNext(Playlist playlist)
        => await WorkOnNowPlayingAsync(playlist, MusicPlayOperation.AddNext);

    /// <summary>
    /// 播放 <see cref="Playlist"/> 序列。
    /// </summary>
    /// <param name="playlists">一个 <see cref="Playlist"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(IEnumerable<Playlist> playlists)
        => await WorkOnNowPlayingAsync(playlists, MusicPlayOperation.Replace);

    /// <summary>
    /// 将 <see cref="Playlist"/> 序列添加到正在播放。
    /// </summary>
    /// <param name="playlists">一个 <see cref="Playlist"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<Playlist> playlists)
        => await WorkOnNowPlayingAsync(playlists, MusicPlayOperation.Add);

    /// <summary>
    /// 将一个 <see cref="Playlist"/> 序列设为下一项播放。
    /// </summary>
    /// <param name="playlists">一个 <see cref="Playlist"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNext(IEnumerable<Playlist> playlists)
        => await WorkOnNowPlayingAsync(playlists, MusicPlayOperation.AddNext);
    #endregion

    #region Favorites
    /// <summary>
    /// 播放歌曲收藏夹中的歌曲。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlaySongFavorites()
        => await WorkOnNowPlayingForSongFavoritesAsync(MusicPlayOperation.Replace);

    /// <summary>
    /// 将歌曲收藏夹添加到正在播放列表中。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddSongFavoriteToNowPlaying()
        => await WorkOnNowPlayingForSongFavoritesAsync(MusicPlayOperation.Add);

    /// <summary>
    /// 将歌曲收藏夹中的歌曲设为下一项播放。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNextForSongFavorite()
        => await WorkOnNowPlayingForSongFavoritesAsync(MusicPlayOperation.AddNext);

    /// <summary>
    /// 播放专辑收藏夹中的歌曲。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlayAlbumFavorites()
        => await WorkOnNowPlayingForAlbumFavoritesAsync(MusicPlayOperation.Replace);

    /// <summary>
    /// 将专辑收藏夹添加到正在播放列表中。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddAlbumFavoriteToNowPlaying()
        => await WorkOnNowPlayingForAlbumFavoritesAsync(MusicPlayOperation.Add);

    /// <summary>
    /// 将专辑收藏夹中的歌曲设为下一项播放。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNextForAlbumFavorite()
        => await WorkOnNowPlayingForAlbumFavoritesAsync(MusicPlayOperation.AddNext);
    #endregion

    /// <summary>
    /// 对正在播放列表进行操作。
    /// </summary>
    /// <param name="playable">可播放对象的实例。</param>
    /// <param name="operation">指示要对正在播放列表进行的操作。</param>
    /// <returns>指示操作是否成功的值。</returns>
    private static async Task<bool> WorkOnNowPlayingAsync(IPlayable playable, MusicPlayOperation operation)
    {
        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(playable, box);

            switch (operation)
            {
                case MusicPlayOperation.Replace:
                    await MusicService.ReplaceMusic(items);
                    break;
                case MusicPlayOperation.Add:
                    await MusicService.AddMusic(items);
                    break;
                case MusicPlayOperation.AddNext:
                    await MusicService.PlayNext(items);
                    break;
                default:
                    throw new NotImplementedException("尚未实现更多播放操作。");
            }

            box.Unbox();
            return true;
        }
        catch (AggregateException ex)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();

            if (ex.Flatten().InnerExceptions.Any(ex => ex is ArgumentOutOfRangeException))
            {
                if (playable is ICorruptible corruptible)
                {
                    corruptible.MarkAsCorrupted();
                }
            }

            await DisplayAggregateExceptionErrorDialog(ex);
        }

        return false;
    }

    /// <summary>
    /// 对正在播放列表进行操作。
    /// </summary>
    /// <param name="playlist">播放列表的实例。</param>
    /// <param name="operation">指示要对正在播放列表进行的操作。</param>
    /// <returns>指示操作是否成功的值。</returns>
    private static async Task<bool> WorkOnNowPlayingAsync(Playlist playlist, MusicPlayOperation operation)
    {
        if (playlist.SongCount == 0)
        {
            await DisplayPlaylistEmptyDialog();
        }
        else
        {
            try
            {
                switch (operation)
                {
                    case MusicPlayOperation.Replace:
                        await PlaylistService.PlayForPlaylistAsync(playlist);
                        break;
                    case MusicPlayOperation.Add:
                        await PlaylistService.AddPlaylistToNowPlayingAsync(playlist);
                        break;
                    case MusicPlayOperation.AddNext:
                        await PlaylistService.PlayNextForPlaylistAsync(playlist);
                        break;
                    default:
                        throw new NotImplementedException("尚未实现更多播放操作。");
                }

                return true;
            }
            catch (AggregateException ex)
            {
                MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
                await DisplayAggregateExceptionErrorDialog(ex);
            }
        }

        return false;
    }

    /// <summary>
    /// 对正在播放列表进行操作。
    /// </summary>
    /// <param name="playlists">播放列表序列。</param>
    /// <param name="operation">指示要对正在播放列表进行的操作。</param>
    /// <returns>指示操作是否成功的值。</returns>
    private static async Task<bool> WorkOnNowPlayingAsync(IEnumerable<Playlist> playlists, MusicPlayOperation operation)
    {
        if (!playlists.Any())
        {
            return false;
        }

        bool noSongInPlaylists = true;
        foreach (Playlist playlist in playlists)
        {
            if (playlist.SongCount > 0)
            {
                noSongInPlaylists = false;
                break;
            }
        }

        if (noSongInPlaylists)
        {
            await DisplayPlaylistEmptyDialog();
            return false;
        }

        try
        {
            switch (operation)
            {
                case MusicPlayOperation.Replace:
                    await PlaylistService.PlayForPlaylistsAsync(playlists);
                    break;
                case MusicPlayOperation.Add:
                    await PlaylistService.AddPlaylistsToNowPlayingAsync(playlists);
                    break;
                case MusicPlayOperation.AddNext:
                    await PlaylistService.PlayNextForPlaylistsAsync(playlists);
                    break;
                default:
                    throw new NotImplementedException("尚未实现更多播放操作。");
            }

            return true;
        }
        catch (AggregateException ex)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayAggregateExceptionErrorDialog(ex);
        }

        return false;
    }

    /// <summary>
    /// 使用歌曲收藏夹的内容对正在播放列表进行操作。
    /// </summary>
    /// <param name="operation">指示要对正在播放列表进行的操作。</param>
    /// <returns>指示操作是否成功的值。</returns>
    private static async Task<bool> WorkOnNowPlayingForSongFavoritesAsync(MusicPlayOperation operation)
    {
        if (FavoriteService.SongFavoriteList.SongCount == 0)
        {
            await DisplayPlaylistEmptyDialog();
        }
        else
        {
            try
            {
                switch (operation)
                {
                    case MusicPlayOperation.Replace:
                        await FavoriteService.PlaySongFavoriteListAsync();
                        break;
                    case MusicPlayOperation.Add:
                        await FavoriteService.AddSongFavoriteListToNowPlayingAsync();
                        break;
                    case MusicPlayOperation.AddNext:
                        await FavoriteService.PlayNextForSongFavoriteListAsync();
                        break;
                    default:
                        throw new NotImplementedException("尚未实现更多播放操作。");
                }

                return true;
            }
            catch (AggregateException ex)
            {
                MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
                await DisplayAggregateExceptionErrorDialog(ex);
            }
        }

        return false;
    }

    /// <summary>
    /// 使用专辑收藏夹的内容对正在播放列表进行操作。
    /// </summary>
    /// <param name="operation">指示要对正在播放列表进行的操作。</param>
    /// <returns>指示操作是否成功的值。</returns>
    private static async Task<bool> WorkOnNowPlayingForAlbumFavoritesAsync(MusicPlayOperation operation)
    {
        if (FavoriteService.AlbumFavoriteList.AlbumCount == 0)
        {
            await DisplayPlaylistEmptyDialog();
        }
        else
        {
            try
            {
                switch (operation)
                {
                    case MusicPlayOperation.Replace:
                        await FavoriteService.PlayAlbumFavoriteListAsync();
                        break;
                    case MusicPlayOperation.Add:
                        await FavoriteService.AddAlbumFavoriteListToNowPlayingAsync();
                        break;
                    case MusicPlayOperation.AddNext:
                        await FavoriteService.PlayNextForAlbumFavoriteListAsync();
                        break;
                    default:
                        throw new NotImplementedException("尚未实现更多播放操作。");
                }
                return true;
            }
            catch (AggregateException ex)
            {
                MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
                await DisplayAggregateExceptionErrorDialog(ex);
            }
        }

        return false;
    }
}
