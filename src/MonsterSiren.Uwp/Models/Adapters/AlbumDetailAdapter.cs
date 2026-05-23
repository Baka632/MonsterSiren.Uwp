using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="AlbumDetail"/> 提供服务的适配器。
/// </summary>
/// <param name="albumDetail">指定的 <see cref="AlbumDetail"/> 实例。</param>
public sealed class AlbumDetailAdapter(AlbumDetail albumDetail) : ISongCidProvider, IFavoriteAddable, INameProvider
{
    public string Name => albumDetail.Name;

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        if (albumDetail.Songs is null)
        {
            yield break;
        }

        foreach (SongInfo song in albumDetail.Songs)
        {
            yield return song.Cid;
        }
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        AlbumFavoriteItem item;

        try
        {
            AlbumInfo albumInfo = (await CommonValues.GetOrFetchAlbums()).CollectionSource
                .Single(info => info.Cid == albumDetail.Cid);

            item = new(
                albumInfo.Cid,
                albumInfo.Name,
                albumInfo.Artistes);
        }
        catch (Exception ex)
        {
            box.InboxException = ex;
            return;
        }

        await FavoriteService.AddAlbumToFavoriteAsync(item);
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveAlbumFromFavoriteAsync(albumDetail.Cid);
    }
}

/// <summary>
/// 为 <see cref="AlbumDetailAdapter"/> 提供扩展方法的类。
/// </summary>
public static class AlbumDetailAdapterExtensions
{
    extension(AlbumDetail detail)
    {
        /// <summary>
        /// 使用 <see cref="AlbumDetail"/> 获得一个 <see cref="AlbumDetailAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="AlbumDetailAdapter"/>。</returns>
        public AlbumDetailAdapter ToAdapter() => new(detail);
    }
}