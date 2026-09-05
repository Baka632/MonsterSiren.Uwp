using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="SongInfo"/> 序列提供服务的适配器。
/// </summary>
/// <param name="songInfos">指定的 <see cref="SongInfo"/> 实例。</param>
public sealed class SongInfoSequenceAdapter(IEnumerable<SongInfo> songInfos) : ISongCidProvider, IFavoriteAddable, IContentContainer
{
    public bool IsEmpty => !songInfos.Any();
    public int Count => songInfos.Count();

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        foreach (SongInfo info in songInfos.ToArray())
        {
            yield return info.Cid;
        }
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        await FavoriteService.AddSongsToFavoriteAsync(GetAsyncEnumerable(box));
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveSongsFromFavoriteAsync(GetCids());
    }

    private async IAsyncEnumerable<string> GetCids()
    {
        foreach (SongInfo songInfo in songInfos.ToArray())
        {
            yield return songInfo.Cid;
        }
    }

    private async IAsyncEnumerable<SongFavoriteItem> GetAsyncEnumerable(ExceptionBox box)
    {
        AllFailedHelper allFailedHelper = new();
        AggregateExceptionHelper helper = new();

        foreach (SongInfo info in songInfos)
        {
            allFailedHelper.Start();

            SongFavoriteItem songFavoriteItem;
            try
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(info.Cid);
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(songDetail.AlbumCid);

                TimeSpan duration = await MsrModelsHelper.GetSongDurationAsync(songDetail) ?? TimeSpan.Zero;

                songFavoriteItem = new(songDetail.Cid, albumDetail.Cid, songDetail.Name, albumDetail.Name, duration);
                allFailedHelper.Succeed();
            }
            catch (Exception ex)
            {
                helper.Record(ex);
                continue;
            }

            yield return songFavoriteItem;
        }

        bool allFailed = allFailedHelper.IsAllFailed();
        IEnumerable<(string Key, object Value)> data = AggregateExceptionHelper.GetDataForCommonUsage(allFailed, songInfos);
        box.InboxException = helper.TryGetException(data);
    }
}

/// <summary>
/// 为 <see cref="SongInfoSequenceAdapter"/> 提供扩展方法的类。
/// </summary>
public static class SongInfoSequenceAdapterExtensions
{
    extension(IEnumerable<SongInfo> songInfos)
    {
        /// <summary>
        /// 使用 <see cref="SongInfo"/> 序列获得一个 <see cref="SongInfoSequenceAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="SongInfoSequenceAdapter"/>。</returns>
        public SongInfoSequenceAdapter ToAdapter() => new(songInfos);
    }
}