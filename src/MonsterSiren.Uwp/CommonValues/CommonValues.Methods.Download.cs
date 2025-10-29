using System.Net.Http;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 启动下载 <see cref="AlbumInfo"/> 中歌曲的操作。
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartDownload(AlbumInfo albumInfo)
    {
        try
        {
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);

            foreach (SongInfo songInfo in albumDetail.Songs)
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="AlbumInfo"/> 序列中歌曲的操作。
    /// </summary>
    /// <param name="albumInfos">一个 <see cref="AlbumInfo"/> 序列。</param>
    /// <returns>指示下载操作是否成功开始的值。</returns>
    public static async Task<bool> StartDownload(IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            foreach (AlbumInfo albumInfo in albumInfos)
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                    _ = DownloadService.DownloadSong(albumDetail, songDetail);
                }
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="AlbumDetail"/> 中歌曲的操作。
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 的实例。</param>
    /// <returns>指示下载操作是否成功开始的值。</returns>
    public static async Task<bool> StartDownload(AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            foreach (SongInfo item in albumDetail.Songs)
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="SongInfo"/> 所表示歌曲的操作。
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 的实例。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartDownload(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            _ = DownloadService.DownloadSong(albumDetail, songDetail);
            // TODO: 此处异常被吞噬，需要使用 W 的异常盒子

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="SongInfo"/> 序列的操作。
    /// </summary>
    /// <param name="songInfos"><see cref="SongInfo"/> 序列。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartDownload(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (!songInfos.Any())
        {
            return false;
        }

        try
        {
            foreach (SongInfo songInfo in songInfos.ToArray())
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="PlaylistItem"/> 所表示播放列表项的操作。
    /// </summary>
    /// <param name="playlistItem">一个 <see cref="PlaylistItem"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartDownload(PlaylistItem playlistItem)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(playlistItem.SongCid);
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(playlistItem.AlbumCid);
            _ = DownloadService.DownloadSong(albumDetail, songDetail);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }
        catch (ArgumentOutOfRangeException)
        {
            await DisplaySongOrAlbumCidCorruptDialog();
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="Playlist"/> 中歌曲的操作。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownload(Playlist playlist)
    {
        if (playlist.Items.Count <= 0)
        {
            return false;
        }

        bool isAllSuccess = await StartDownload(playlist.Items);
        return isAllSuccess;
    }

    /// <summary>
    /// 启动下载 <see cref="PlaylistItem"/> 序列的操作。
    /// </summary>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列。</param>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownload(IEnumerable<PlaylistItem> playlistItems)
    {
        if (!playlistItems.Any())
        {
            return false;
        }

        bool isAllSuccess = true;

        foreach (PlaylistItem playlistItem in playlistItems.ToArray())
        {
            try
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(playlistItem.SongCid);
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(playlistItem.AlbumCid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }
            catch (HttpRequestException)
            {
                await DisplayInternetErrorDialog();
                isAllSuccess = false;
            }
            catch (ArgumentOutOfRangeException)
            {
                await DisplaySongOrAlbumCidCorruptDialog();
                isAllSuccess = false;
            }
        }

        return isAllSuccess;
    }

    /// <summary>
    /// 启动下载收藏夹中歌曲的操作。
    /// </summary>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownloadSongFavorites()
    {
        if (FavoriteService.SongFavoriteList.Items.Count <= 0)
        {
            return false;
        }

        bool isAllSuccess = await StartDownload(FavoriteService.SongFavoriteList.Items);
        return isAllSuccess;
    }

    /// <summary>
    /// 启动下载 <see cref="SongFavoriteItem"/> 序列的操作。
    /// </summary>
    /// <param name="songFavoriteItems"><see cref="SongFavoriteItem"/> 序列。</param>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownload(IEnumerable<SongFavoriteItem> songFavoriteItems)
    {
        if (!songFavoriteItems.Any())
        {
            return false;
        }

        bool isAllSuccess = true;

        foreach (SongFavoriteItem playlistItem in songFavoriteItems.ToArray())
        {
            try
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(playlistItem.SongCid);
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(playlistItem.AlbumCid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }
            catch (HttpRequestException)
            {
                await DisplayInternetErrorDialog();
                isAllSuccess = false;
            }
            catch (ArgumentOutOfRangeException)
            {
                await DisplaySongOrAlbumCidCorruptDialog();
                isAllSuccess = false;
            }
        }

        return isAllSuccess;
    }

    /// <summary>
    /// 启动下载 <see cref="SongFavoriteItem"/> 所表示播放列表项的操作。
    /// </summary>
    /// <param name="item">一个 <see cref="SongFavoriteItem"/> 实例。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> StartDownload(SongFavoriteItem item)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
            _ = DownloadService.DownloadSong(albumDetail, songDetail);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }
        catch (ArgumentOutOfRangeException)
        {
            await DisplaySongOrAlbumCidCorruptDialog();
        }

        return false;
    }
}
