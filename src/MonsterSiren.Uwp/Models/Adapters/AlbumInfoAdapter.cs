using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="AlbumInfo"/> 提供服务的适配器。
/// </summary>
/// <param name="albumInfo">指定的 <see cref="AlbumInfo"/> 实例。</param>
public sealed class AlbumInfoAdapter(AlbumInfo albumInfo) : IPlayable, IFavoriteAddable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        AlbumDetail detail;
        try
        {
            detail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
        }
        catch (Exception ex)
        {
            box.InboxException = ex;
            yield break;
        }

        if (detail.Songs is null)
        {
            yield break;
        }

        foreach (SongInfo song in detail.Songs)
        {
            yield return song.Cid;
        }
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        AlbumFavoriteItem item = new(
            albumInfo.Cid,
            albumInfo.Name,
            albumInfo.Artistes);

        await FavoriteService.AddAlbumToFavoriteAsync(item);
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveAlbumFromFavoriteAsync(albumInfo.Cid);
    }
}

/// <summary>
/// 为 <see cref="AlbumInfoAdapter"/> 提供扩展方法的类。
/// </summary>
public static class AlbumInfoAdapterExtensions
{
    extension(AlbumInfo info)
    {
        /// <summary>
        /// 使用 <see cref="AlbumInfo"/> 获得一个 <see cref="AlbumInfoAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="AlbumInfoAdapter"/>。</returns>
        public AlbumInfoAdapter ToAdapter() => new(info);
    }
}