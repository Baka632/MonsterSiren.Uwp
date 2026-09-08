using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 根据 <see cref="ISongCidProvider"/> 获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。
    /// </summary>
    /// <param name="provider"><see cref="ISongCidProvider"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。</returns>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(ISongCidProvider provider, ExceptionBox box)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (box is null)
        {
            throw new ArgumentNullException(nameof(box));
        }

        ExceptionBox innerBox = new();
        AggregateExceptionHelper aggregateHelper = new();
        AllFailedHelper allFailedHelper = new();

        await foreach (string songCid in provider.GetSongCidsAsync(innerBox))
        {
            allFailedHelper.Start();

            SongDetail songDetail;
            AlbumDetail albumDetail;

            try
            {
                songDetail = await MsrModelsHelper.GetSongDetailAsync(songCid);
                albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(songDetail.AlbumCid);

                allFailedHelper.Succeed();
            }
            catch (Exception ex)
            {
                aggregateHelper.Record(ex);

                if (ex is ArgumentOutOfRangeException && provider is IContentCorruptible contentCorruptible)
                {
                    contentCorruptible.MarkItemAsCorrupted(songCid);
                }

                continue;
            }

            yield return (songDetail, albumDetail);
        }

        if (innerBox.InboxException is not null)
        {
            aggregateHelper.Record(innerBox.InboxException);
        }

        if (aggregateHelper.HasException)
        {
            bool allFailed = allFailedHelper.IsAllFailed();
            box.InboxException = aggregateHelper.TryGetException(AggregateExceptionHelper.GetDataForCommonUsage(allFailed, provider));
        }
    }
}
