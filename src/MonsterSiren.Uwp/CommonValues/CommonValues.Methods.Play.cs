using Windows.Media.Playback;
using MonsterSiren.Uwp.Models.Playlists;
using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    #region IPlayable
    /// <summary>
    /// 播放 <see cref="ISongCidProvider"/> 所表示的内容。
    /// </summary>
    /// <param name="playable">可提供歌曲 CID 对象的实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(ISongCidProvider playable)
        => await WorkOnNowPlayingAsync(playable, MusicPlayOperation.Replace);

    /// <summary>
    /// 将 <see cref="ISongCidProvider"/> 所表示的内容加入到正在播放列表中。
    /// </summary>
    /// <param name="playable">可提供歌曲 CID 对象的实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(ISongCidProvider playable)
        => await WorkOnNowPlayingAsync(playable, MusicPlayOperation.Add);

    /// <summary>
    /// 将 <see cref="ISongCidProvider"/> 所表示的内容设为下一项播放。
    /// </summary>
    /// <param name="playable">可提供歌曲 CID 对象的实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNext(ISongCidProvider playable)
        => await WorkOnNowPlayingAsync(playable, MusicPlayOperation.AddNext);
    #endregion

    #region Playlist
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
    public static async Task<bool> StartPlaySongFavorite()
        => await WorkOnNowPlayingAsync(FavoriteService.SongFavoriteList.ToAdapter(), MusicPlayOperation.Replace);

    /// <summary>
    /// 将歌曲收藏夹添加到正在播放列表中。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddSongFavoriteToNowPlaying()
        => await WorkOnNowPlayingAsync(FavoriteService.SongFavoriteList.ToAdapter(), MusicPlayOperation.Add);

    /// <summary>
    /// 将歌曲收藏夹中的歌曲设为下一项播放。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNextForSongFavorite()
        => await WorkOnNowPlayingAsync(FavoriteService.SongFavoriteList.ToAdapter(), MusicPlayOperation.AddNext);

    /// <summary>
    /// 播放专辑收藏夹中的歌曲。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlayAlbumFavorite()
        => await WorkOnNowPlayingAsync(FavoriteService.AlbumFavoriteList.ToAdapter(), MusicPlayOperation.Replace);

    /// <summary>
    /// 将专辑收藏夹添加到正在播放列表中。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddAlbumFavoriteToNowPlaying()
        => await WorkOnNowPlayingAsync(FavoriteService.AlbumFavoriteList.ToAdapter(), MusicPlayOperation.Add);

    /// <summary>
    /// 将专辑收藏夹中的歌曲设为下一项播放。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> PlayNextForAlbumFavorite()
        => await WorkOnNowPlayingAsync(FavoriteService.AlbumFavoriteList.ToAdapter(), MusicPlayOperation.AddNext);
    #endregion

    /// <summary>
    /// 对正在播放列表进行操作。
    /// </summary>
    /// <param name="provider">可提供歌曲 CID 对象的实例。</param>
    /// <param name="operation">指示要对正在播放列表进行的操作。</param>
    /// <returns>指示操作是否成功的值。</returns>
    private static async Task<bool> WorkOnNowPlayingAsync(ISongCidProvider provider, MusicPlayOperation operation)
    {
        if (provider is IContentContainer container && container.IsEmpty)
        {
            await DisplayPlaylistEmptyDialog();
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(provider, box);

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
                if (provider is ICorruptible corruptible)
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
}
