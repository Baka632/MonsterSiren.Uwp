using System.Net.Http;
using MonsterSiren.Uwp.Models.Favorites;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 播放 <see cref="AlbumInfo"/> 中的歌曲。
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(AlbumInfo albumInfo)
    {
        try
        {
            ExceptionBox box = new();
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="AlbumInfo"/> 专辑序列。
    /// </summary>
    /// <param name="albumInfos">一个 <see cref="AlbumInfo"/> 序列。</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems([.. albumInfos], box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="AlbumDetail"/> 中的歌曲。
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> StartPlay(AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="SongInfo"/> 所表示的歌曲。
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 的实例。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(songDetail, albumDetail);

            MusicService.ReplaceMusic(item);

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放一个 <see cref="SongInfo"/> 序列，其中的歌曲应当属于 <paramref name="albumDetail"/> 所表示的专辑。
    /// </summary>
    /// <param name="songInfos">一个歌曲序列。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> StartPlay(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (songInfos.Any() != true)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems([.. songInfos], albumDetail, box);
            await MusicService.ReplaceMusic(items);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="PlaylistItem"/> 所表示的播放列表项。
    /// </summary>
    /// <param name="playlistItem"><see cref="PlaylistItem"/> 所表示的播放列表项。</param>
    /// <param name="playlist">播放列表项所属的播放列表，此参数用于在 <paramref name="playlistItem"/> 无效时对播放列表进行更新。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(PlaylistItem playlistItem, Playlist playlist)
    {
        try
        {
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(playlistItem.SongCid);
            MusicService.ReplaceMusic(item);

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();

            int targetIndex = playlist.Items.IndexOf(playlistItem);
            if (targetIndex != -1)
            {
                playlist.Items[targetIndex] = playlistItem with { IsCorruptedItem = true };
            }

            await DisplaySongOrAlbumCidCorruptDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放一个 <see cref="PlaylistItem"/> 序列。
    /// </summary>
    /// <param name="playlistItems">一个 <see cref="PlaylistItem"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(IEnumerable<PlaylistItem> playlistItems)
    {
        if (!playlistItems.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems([.. playlistItems], box);
            await MusicService.ReplaceMusic(items);
            box.Unbox();

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="Playlist"/> 所表示的播放列表。
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(Playlist playlist)
    {
        if (playlist.SongCount == 0)
        {
            await DisplayPlaylistEmptyDialog();
        }
        else
        {
            try
            {
                await PlaylistService.PlayForPlaylistAsync(playlist);

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
    /// 播放 <see cref="Playlist"/> 序列。
    /// </summary>
    /// <param name="playlists">一个 <see cref="Playlist"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(IEnumerable<Playlist> playlists)
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
            await PlaylistService.PlayForPlaylistsAsync(playlists);

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
    /// 播放歌曲收藏夹中的歌曲。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlaySongFavorites()
    {
        if (FavoriteService.SongFavoriteList.SongCount == 0)
        {
            await DisplayPlaylistEmptyDialog();
        }
        else
        {
            try
            {
                await FavoriteService.PlaySongFavoriteListAsync();

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
    /// 播放一个 <see cref="SongFavoriteItem"/> 序列。
    /// </summary>
    /// <param name="songFavoriteItems">一个 <see cref="SongFavoriteItem"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(IEnumerable<SongFavoriteItem> songFavoriteItems)
    {
        if (!songFavoriteItems.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems([.. songFavoriteItems], box);
            await MusicService.ReplaceMusic(items);
            box.Unbox();

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="SongFavoriteItem"/> 所表示的收藏夹项。
    /// </summary>
    /// <param name="favoriteItem"><see cref="SongFavoriteItem"/> 所表示的收藏夹项。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartPlay(SongFavoriteItem favoriteItem)
    {
        try
        {
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(favoriteItem.SongCid);
            MusicService.ReplaceMusic(item);

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayInternetErrorDialog();
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();

            int targetIndex = FavoriteService.SongFavoriteList.Items.IndexOf(favoriteItem);
            if (targetIndex != -1)
            {
                FavoriteService.SongFavoriteList.Items[targetIndex] = favoriteItem with { IsCorruptedItem = true };
            }

            await DisplaySongOrAlbumCidCorruptDialog();
        }

        return false;
    }
}
