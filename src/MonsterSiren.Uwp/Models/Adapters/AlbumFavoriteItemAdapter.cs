using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="AlbumFavoriteItem"/> 提供服务的适配器。
/// </summary>
/// <param name="albumFavoriteItem">指定的 <see cref="AlbumFavoriteItem"/> 实例。</param>
public sealed class AlbumFavoriteItemAdapter(AlbumFavoriteItem albumFavoriteItem) : ISongCidProvider, ICorruptible, IFavoriteAddable, INameProvider
{
    public string Name => albumFavoriteItem.AlbumName;

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        AlbumDetail detail;
        try
        {
            detail = await MsrModelsHelper.GetAlbumDetailAsync(albumFavoriteItem.AlbumCid);
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

    public void MarkAsCorrupted()
    {
        int targetIndex = FavoriteService.AlbumFavoriteList.Items.IndexOf(albumFavoriteItem);
        if (targetIndex != -1)
        {
            FavoriteService.AlbumFavoriteList.Items[targetIndex] = albumFavoriteItem with { IsCorruptedItem = true };
        }
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        await FavoriteService.AddAlbumToFavoriteAsync(albumFavoriteItem);
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveAlbumFromFavoriteAsync(albumFavoriteItem);
    }
}

/// <summary>
/// 为 <see cref="AlbumFavoriteItemAdapter"/> 提供扩展方法的类。
/// </summary>
public static class AlbumFavoriteItemAdapterExtensions
{
    extension(AlbumFavoriteItem albumFavoriteItem)
    {
        /// <summary>
        /// 使用 <see cref="AlbumFavoriteItem"/> 获得一个 <see cref="AlbumFavoriteItemAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="AlbumFavoriteItemAdapter"/>。</returns>
        public AlbumFavoriteItemAdapter ToAdapter() => new(albumFavoriteItem);
    }
}