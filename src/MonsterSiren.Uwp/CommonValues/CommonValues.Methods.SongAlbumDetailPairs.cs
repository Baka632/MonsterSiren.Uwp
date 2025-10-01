namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 根据 <see cref="AlbumInfo"/> 序列获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。
    /// </summary>
    /// <param name="albumInfos">一个 <see cref="AlbumInfo"/> 序列。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(IEnumerable<AlbumInfo> albumInfos, ExceptionBox box)
    {
        foreach (AlbumInfo albumInfo in albumInfos)
        {
            AlbumDetail albumDetail;
            try
            {
                albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            foreach (SongInfo songInfo in albumDetail.Songs)
            {
                SongDetail songDetail;

                try
                {
                    songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                }
                catch (Exception ex)
                {
                    box.InboxException = ex;
                    yield break;
                }

                yield return (songDetail, albumDetail);
            }
        }
    }

    /// <summary>
    /// 根据 <see cref="AlbumDetail"/> 实例获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(AlbumDetail albumDetail, ExceptionBox box)
    {
        foreach (SongInfo item in albumDetail.Songs)
        {
            SongDetail songDetail;

            try
            {
                songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return (songDetail, albumDetail);
        }
    }

    /// <summary>
    /// 根据 <see cref="SongInfo"/> 序列获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。
    /// </summary>
    /// <param name="songInfos"><see cref="SongInfo"/> 数组。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(SongInfo[] songInfos, AlbumDetail albumDetail, ExceptionBox box)
    {
        foreach (SongInfo item in songInfos)
        {
            SongDetail songDetail;

            try
            {
                songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return (songDetail, albumDetail);
        }
    }

    /// <summary>
    /// 根据 <see cref="SongInfoAndAlbumDetailPack"/> 序列获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。
    /// </summary>
    /// <param name="packs"><see cref="SongInfoAndAlbumDetailPack"/> 序列。</param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列。</returns>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(IEnumerable<SongInfoAndAlbumDetailPack> packs, ExceptionBox box)
    {
        foreach ((SongInfo songInfo, AlbumDetail albumDetail) in packs)
        {
            SongDetail songDetail;

            try
            {
                songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return (songDetail, albumDetail);
        }
    }
}
