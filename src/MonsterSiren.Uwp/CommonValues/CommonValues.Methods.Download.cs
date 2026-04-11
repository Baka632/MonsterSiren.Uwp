using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 启动下载 <see cref="ISongCidProvider"/> 中表示歌曲的操作。
    /// </summary>
    /// <param name="songCidProvider">可提供歌曲 CID 对象的实例。</param>
    /// <returns>指示下载操作是否成功开始的值。</returns>
    public static async Task<bool> StartDownload(ISongCidProvider songCidProvider)
    {
        if (songCidProvider is null)
        {
            throw new ArgumentNullException(nameof(songCidProvider));
        }

        AggregateExceptionHelper aggregateHelper = new();
        ExceptionBox box = new();
        IAsyncEnumerable<string> cids = songCidProvider.GetSongCidsAsync(box);

        int songCount = 0;

        await foreach (string songCid in cids)
        {
            try
            {
                songCount++;

                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songCid);
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(songDetail.AlbumCid);

                DownloadService.EnqueueSongDownload(albumDetail, songDetail);
            }
            catch (Exception ex)
            {
                aggregateHelper.Record(ex);
            }
        }

        if (box.InboxException is not null)
        {
            aggregateHelper.Record(box.InboxException);
        }

        if (aggregateHelper.HasException)
        {
            bool allFailed = songCount == aggregateHelper.ExceptionCount;
            AggregateException aggregate = aggregateHelper.TryGetException(AggregateExceptionHelper.GetDataForCommonUsage(allFailed, songCidProvider));

            if (aggregate.Flatten().InnerExceptions.Any(ex => ex is ArgumentOutOfRangeException))
            {
                if (songCidProvider is ICorruptible corruptible)
                {
                    corruptible.MarkAsCorrupted();
                }
            }

            await DisplayAggregateExceptionErrorDialog(aggregate);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 启动下载收藏夹中歌曲的操作。
    /// </summary>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownloadSongFavorites()
        => await DownloadForFavorites(FavoriteType.Song);

    /// <summary>
    /// 启动下载专辑收藏夹中所有歌曲的操作。
    /// </summary>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownloadAlbumFavorites()
        => await DownloadForFavorites(FavoriteType.Album);

    private static async Task<bool> DownloadForFavorites(FavoriteType favoriteType)
    {
        ISongCidProvider provider;
        int count;

        switch (favoriteType)
        {
            case FavoriteType.Song:
                count = FavoriteService.SongFavoriteList.Count;
                provider = FavoriteService.SongFavoriteList.Items.ToAdapter();
                break;
            case FavoriteType.Album:
                count = FavoriteService.AlbumFavoriteList.Count;
                provider = FavoriteService.AlbumFavoriteList.Items.ToAdapter();
                break;
            default:
                throw new NotImplementedException("尚未实现这种收藏类型。");
        }

        if (count == 0)
        {
            return false;
        }

        bool isAllSuccess = await StartDownload(provider);
        return isAllSuccess;
    }
}
