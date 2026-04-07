using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="AlbumFavoriteItem"/> 序列提供服务的适配器。
/// </summary>
/// <param name="albumFavoriteItems">指定的 <see cref="AlbumFavoriteItem"/> 序列实例。</param>
public sealed class AlbumFavoriteItemSequenceAdapter(IEnumerable<AlbumFavoriteItem> albumFavoriteItems) : IPlayable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        int albumCount = 0;
        AggregateExceptionHelper helper = new();

        foreach (AlbumFavoriteItem item in albumFavoriteItems.ToArray())
        {
            AlbumDetail detail;
            try
            {
                detail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                albumCount++;
            }
            catch (Exception ex)
            {
                helper.Record(ex);
                continue;
            }

            if (detail.Songs is null)
            {
                continue;
            }

            foreach (SongInfo song in detail.Songs)
            {
                yield return song.Cid;
            }
        }

        bool allFailed = albumCount == helper.ExceptionCount;
        IEnumerable<(string Key, object Value)> data = AggregateExceptionHelper.GetDataForCommonUsage(allFailed, albumFavoriteItems);
        box.InboxException = helper.TryGetException(data);
    }
}

/// <summary>
/// 为 <see cref="AlbumFavoriteItemSequenceAdapter"/> 提供扩展方法的类。
/// </summary>
public static class AlbumFavoriteItemSequenceAdapterExtensions
{
    extension(IEnumerable<AlbumFavoriteItem> albumFavoriteItems)
    {
        /// <summary>
        /// 使用 <see cref="AlbumFavoriteItem"/> 序列获得一个 <see cref="AlbumFavoriteItemSequenceAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="AlbumFavoriteItemSequenceAdapter"/>。</returns>
        public AlbumFavoriteItemSequenceAdapter ToAdapter() => new(albumFavoriteItems);
    }
}