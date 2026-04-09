using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="AlbumInfo"/> 序列提供服务的适配器。
/// </summary>
/// <param name="albumInfos">指定的 <see cref="AlbumInfo"/> 实例。</param>
public sealed class AlbumInfoSequenceAdapter(IEnumerable<AlbumInfo> albumInfos) : ISongCidProvider, IFavoriteAddable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        int albumCount = 0;
        AggregateExceptionHelper helper = new();

        foreach (AlbumInfo albumInfo in albumInfos.ToArray())
        {
            AlbumDetail detail;
            try
            {
                detail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
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
        IEnumerable<(string Key, object Value)> data = AggregateExceptionHelper.GetDataForCommonUsage(allFailed, albumInfos);
        box.InboxException = helper.TryGetException(data);
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        await FavoriteService.AddAlbumsToFavoriteAsync(GetAsyncEnumerable());
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveAlbumsFromFavoriteAsync(GetAsyncEnumerable());
    }

    private async IAsyncEnumerable<AlbumFavoriteItem> GetAsyncEnumerable()
    {
        foreach (AlbumInfo info in albumInfos.ToArray())
        {
            yield return new(info.Cid, info.Name, info.Artistes);
        }
    }
}

/// <summary>
/// 为 <see cref="AlbumInfoSequenceAdapter"/> 提供扩展方法的类。
/// </summary>
public static class AlbumInfoSequenceAdapterExtensions
{
    extension(IEnumerable<AlbumInfo> infos)
    {
        /// <summary>
        /// 使用 <see cref="AlbumInfo"/> 序列获得一个 <see cref="AlbumInfoSequenceAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="AlbumInfoSequenceAdapter"/>。</returns>
        public AlbumInfoSequenceAdapter ToAdapter() => new(infos);
    }
}