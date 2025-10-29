using System.Net.Http;
using MonsterSiren.Uwp.Models.Favorites;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 将 <see cref="AlbumInfo"/> 中的歌曲加入到正在播放列表中。
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(AlbumInfo albumInfo)
    {
        try
        {
            ExceptionBox box = new();
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);

            await MusicService.AddMusic(items);

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
    /// 将 <see cref="AlbumInfo"/> 序列加入到正在播放列表中。
    /// </summary>
    /// <param name="albumInfos">一个 <see cref="AlbumInfo"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems([.. albumInfos], box);

            await MusicService.AddMusic(items);

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
    /// 将 <see cref="AlbumDetail"/> 中的歌曲加入到正在播放列表中。
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToNowPlaying(AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);
            await MusicService.AddMusic(items);
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
    /// 将一个 <see cref="SongInfo"/> 加入到正在播放列表中。
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToNowPlaying(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(songDetail, albumDetail);

            MusicService.AddMusic(item);

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
    /// 将一个 <see cref="SongInfo"/> 序列添加到正在播放列表中，其中的歌曲应当属于 <paramref name="albumDetail"/> 所表示的专辑。
    /// </summary>
    /// <param name="songInfos">一个歌曲序列。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (!songInfos.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems([.. songInfos], albumDetail, box);
            await MusicService.AddMusic(items);
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
    /// 将一个 <see cref="PlaylistItem"/> 添加到正在播放列表中。
    /// </summary>
    /// <param name="playlistItem">一个 <see cref="PlaylistItem"/> 实例。</param>
    /// <param name="playlist">播放列表项所属的播放列表，此参数用于在 <paramref name="playlistItem"/> 无效时对播放列表进行更新。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(PlaylistItem playlistItem, Playlist playlist)
    {
        try
        {
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(playlistItem.SongCid);
            MusicService.AddMusic(item);

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
    /// 将一个 <see cref="Playlist"/> 添加到正在播放列表中。
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(Playlist playlist)
    {
        if (playlist.Items.Count <= 0)
        {
            await DisplayPlaylistEmptyDialog();
            return false;
        }

        try
        {
            await PlaylistService.AddPlaylistToNowPlayingAsync(playlist);

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
    /// 将一个 <see cref="Playlist"/> 序列添加到正在播放列表中。
    /// </summary>
    /// <param name="playlists">一个 <see cref="Playlist"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<Playlist> playlists)
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
            await PlaylistService.AddPlaylistsToNowPlayingAsync(playlists);

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
    /// 将一个 <see cref="SongInfoAndAlbumDetailPack"/> 序列添加到正在播放列表中。
    /// </summary>
    /// <param name="packs">一个 <see cref="SongInfoAndAlbumDetailPack"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<SongInfoAndAlbumDetailPack> packs)
    {
        if (!packs.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems([.. packs], box);
            await MusicService.AddMusic(items);
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
    /// 将 <see cref="PlaylistItem"/> 序列添加到正在播放列表中。
    /// </summary>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<PlaylistItem> playlistItems)
    {
        PlaylistItem[] playlistItemsArray = [.. playlistItems];

        if (playlistItemsArray.Length <= 0)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(playlistItemsArray, box);
            await MusicService.AddMusic(items);
            box.Unbox();
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

            if (playlistItemsArray.Length == 1)
            {
                await DisplaySongOrAlbumCidCorruptDialog();
            }
            else
            {
                await DisplaySomeSongOrAlbumCidCorruptDialog();
            }
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="SongFavoriteItem"/> 添加到正在播放列表中。
    /// </summary>
    /// <param name="favoriteItem">一个 <see cref="SongFavoriteItem"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(SongFavoriteItem favoriteItem)
    {
        try
        {
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(favoriteItem.SongCid);
            MusicService.AddMusic(item);

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

    /// <summary>
    /// 将 <see cref="SongFavoriteItem"/> 序列添加到正在播放列表中。
    /// </summary>
    /// <param name="favoriteItems"><see cref="SongFavoriteItem"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<SongFavoriteItem> favoriteItems)
    {
        SongFavoriteItem[] playlistItemsArray = [.. favoriteItems];

        if (playlistItemsArray.Length <= 0)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(playlistItemsArray, box);
            await MusicService.AddMusic(items);
            box.Unbox();
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

            if (playlistItemsArray.Length == 1)
            {
                await DisplaySongOrAlbumCidCorruptDialog();
            }
            else
            {
                await DisplaySomeSongOrAlbumCidCorruptDialog();
            }
        }

        return false;
    }

    /// <summary>
    /// 将歌曲收藏夹添加到正在播放列表中。
    /// </summary>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddSongFavoriteToNowPlaying()
    {
        if (FavoriteService.SongFavoriteList.Items.Count <= 0)
        {
            await DisplayPlaylistEmptyDialog();
            return false;
        }

        try
        {
            await FavoriteService.AddSongFavoriteListToNowPlayingAsync();

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
