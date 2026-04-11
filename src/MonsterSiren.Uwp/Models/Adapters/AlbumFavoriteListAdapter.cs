using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="AlbumFavoriteList"/> 提供服务的适配器。
/// </summary>
/// <param name="albumFavorite">指定的 <see cref="AlbumFavoriteList"/> 实例。</param>
public sealed class AlbumFavoriteListAdapter(AlbumFavoriteList albumFavorite) : ISongCidProvider, IContentContainer
{
    public bool IsEmpty => albumFavorite.Count == 0;

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        AggregateExceptionHelper helper = new();

        AlbumFavoriteItem[] items = [.. albumFavorite.Items];

        AllFailedHelper allFailedHelper = new();

        foreach (AlbumFavoriteItem item in items)
        {
            allFailedHelper.Start();

            AlbumDetail detail;
            try
            {
                detail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                allFailedHelper.Succeed();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    int i = albumFavorite.Items.IndexOf(item);
                    if (i != -1)
                    {
                        albumFavorite.Items[i] = item with { IsCorruptedItem = true };
                    }
                }

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

        bool allFailed =  allFailedHelper.IsAllFailed();
        IEnumerable<(string Key, object Value)> data = AggregateExceptionHelper.GetDataForCommonUsage(allFailed, albumFavorite);
        box.InboxException = helper.TryGetException(data);
    }
}

/// <summary>
/// 为 <see cref="AlbumFavoriteListAdapter"/> 提供扩展方法的类。
/// </summary>
public static class AlbumFavoriteListAdapterExtensions
{
    extension(AlbumFavoriteList albumFavoriteItems)
    {
        /// <summary>
        /// 使用 <see cref="AlbumFavoriteList"/> 获得一个 <see cref="AlbumFavoriteListAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="AlbumFavoriteListAdapter"/>。</returns>
        public AlbumFavoriteListAdapter ToAdapter() => new(albumFavoriteItems);
    }
}
