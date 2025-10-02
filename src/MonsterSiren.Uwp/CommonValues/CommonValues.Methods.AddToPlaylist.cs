using System.Net.Http;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 将 <see cref="AlbumInfo"/> 中的歌曲添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="albumInfo"><see cref="AlbumInfo"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, AlbumInfo albumInfo)
    {
        try
        {
            ExceptionBox box = new();

            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(albumDetail, box);

            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="AlbumInfo"/> 序列添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="albumInfos"><see cref="AlbumInfo"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            ExceptionBox box = new();

            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(albumInfos, box);

            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="AlbumDetail"/> 中的歌曲添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(albumDetail, box);
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="SongInfo"/> 添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, albumDetail);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="SongInfo"/> 序列添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="songInfos"><see cref="SongInfo"/> 序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (!songInfos.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs([.. songInfos], albumDetail, box);
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="PlaylistItem"/> 序列添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<PlaylistItem> playlistItems)
    {
        if (!playlistItems.Any())
        {
            return false;
        }

        try
        {
            PlaylistItem[] items = [.. playlistItems];
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="SongInfoAndAlbumDetailPack"/> 序列添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="packs"><see cref="SongInfoAndAlbumDetailPack"/> 序列。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<SongInfoAndAlbumDetailPack> packs)
    {
        if (!packs.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(packs, box);
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }
}
