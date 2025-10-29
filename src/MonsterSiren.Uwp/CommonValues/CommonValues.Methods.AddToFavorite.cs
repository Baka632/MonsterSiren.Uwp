using System.Net.Http;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 将一个 <see cref="SongInfo"/> 添加到收藏夹中。
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToFavorite(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            await FavoriteService.AddSongToFavoriteAsync(songDetail, albumDetail);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="PlaylistItem"/> 添加到收藏夹中。
    /// </summary>
    /// <param name="item">一个 <see cref="PlaylistItem"/> 实例。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToFavorite(PlaylistItem item)
    {
        try
        {
            await FavoriteService.AddSongToFavoriteAsync(item);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="SongInfo"/> 序列添加到收藏夹中。
    /// </summary>
    /// <param name="songInfos"><see cref="SongInfo"/> 序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToFavorite(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (!songInfos.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs([.. songInfos], albumDetail, box);
            await FavoriteService.AddSongsToFavoriteAsync(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    public static async Task<bool> AddToFavorite(IEnumerable<PlaylistItem> playlistItems)
    {
        if (!playlistItems.Any())
        {
            return false;
        }

        try
        {
            PlaylistItem[] items = [.. playlistItems];
            await FavoriteService.AddSongsToFavoriteAsync(items);
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="SongInfo"/> 表示的歌曲从收藏夹中移除。
    /// </summary>
    /// <param name="songInfo">指定的 <see cref="SongInfo"/>。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> RemoveFromFavorite(SongInfo songInfo)
    {
        return await FavoriteService.RemoveSongFromFavoriteAsync(songInfo.Cid);
    }

    /// <summary>
    /// 将 <see cref="PlaylistItem"/> 表示的歌曲从收藏夹中移除。
    /// </summary>
    /// <param name="item">指定的 <see cref="PlaylistItem"/>。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> RemoveFromFavorite(PlaylistItem item)
    {
        return await FavoriteService.RemoveSongFromFavoriteAsync(item.SongCid);
    }

    /// <summary>
    /// 将 <see cref="SongFavoriteItem"/> 表示的歌曲从收藏夹中移除。
    /// </summary>
    /// <param name="item">指定的 <see cref="SongFavoriteItem"/>。</param>
    /// <returns>指示操作是否成功的值。</returns>
    public static async Task<bool> RemoveFromFavorite(SongFavoriteItem item)
    {
        return await FavoriteService.RemoveSongFromFavoriteAsync(item);
    }
}
