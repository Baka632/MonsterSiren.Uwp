using System.Net.Http;

namespace MonsterSiren.Uwp.Helpers;

partial class MsrModelsHelper
{
    /// <summary>
    /// 通过专辑的 CID 获取专辑详细信息
    /// </summary>
    /// <param name="cid">专辑的 CID</param>
    /// <param name="refresh">指示是否要跳过缓存来获得最新版本的 <see cref="AlbumDetail"/> 的值</param>
    /// <returns>包含专辑详细信息的 <see cref="AlbumDetail"/></returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<AlbumDetail> GetAlbumDetailAsync(string cid, bool refresh = false)
    {
        if (!refresh && MemoryCacheHelper<AlbumDetail>.Default.TryGetData(cid, out AlbumDetail detail))
        {
            return detail;
        }
        else
        {
            await Task.Run(async () =>
            {
                detail = await AlbumService.GetAlbumDetailedInfoAsync(cid);

                bool shouldUpdate = false;
                foreach (SongInfo item in detail.Songs)
                {
                    if (item.Artists is null || item.Artists.Any() != true)
                    {
                        shouldUpdate = true;
                        break;
                    }
                }

                if (shouldUpdate)
                {
                    List<SongInfo> songs = detail.Songs.ToList();
                    TryFillArtistForSongs(songs);

                    detail = detail with { Songs = songs };
                }
            });

            if (detail.Songs.Any())
            {
                MemoryCacheHelper<AlbumDetail>.Default.Store(cid, detail);
            }

            return detail;
        }
    }

    /// <summary>
    /// 通过专辑 CID 获取专辑封面
    /// </summary>
    /// <param name="albumCid">专辑 CID</param>
    /// <returns>指向专辑封面的 <see cref="Uri"/></returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<Uri> GetAlbumCoverAsync(string albumCid)
    {
        Uri uri = await FileCacheHelper.GetAlbumCoverUriAsync(albumCid);

        if (uri is null)
        {
            AlbumDetail albumDetail = await GetAlbumDetailAsync(albumCid);
            uri = new Uri(albumDetail.CoverUrl, UriKind.Absolute);
        }

        return uri;
    }

    /// <summary>
    /// 尝试为 <see cref="AlbumInfo"/> 填充艺术家信息，并使用缓存的专辑 Uri
    /// </summary>
    /// <param name="albumInfo"><see cref="AlbumInfo"/> 的实例</param>
    /// <returns>一个二元组，第一项是指示是否修改了 <see cref="AlbumInfo"/> 的布尔值，第二项是 <see cref="AlbumInfo"/> 实例。若第一项为 <see langword="false"/> ，则表示没有必要对 <see cref="AlbumInfo"/> 进行修改</returns>
    public static async Task<ValueTuple<bool, AlbumInfo>> TryFillArtistAndCachedCoverForAlbum(AlbumInfo albumInfo)
    {
        bool isModify = false;

        if (albumInfo.Artistes is null || albumInfo.Artistes.Any() != true)
        {
            albumInfo = albumInfo with { Artistes = ["MSR".GetLocalized()] };
            isModify = true;
        }

        Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumInfo);
        if (fileCoverUri != null)
        {
            albumInfo = albumInfo with { CoverUrl = fileCoverUri.ToString() };
            isModify = true;
        }

        return (isModify, albumInfo);
    }

    /// <summary>
    /// 尝试为 <see cref="AlbumInfo"/> 列表填充艺术家信息，并使用缓存的专辑 Uri
    /// </summary>
    /// <param name="albumList"><see cref="AlbumInfo"/> 的列表</param>
    /// <returns>指示是否修改了 <see cref="AlbumInfo"/> 的值。<see langword="false"/> 表示没有必要对 <see cref="AlbumInfo"/> 进行修改</returns>
    public static async Task<bool> TryFillArtistAndCachedCoverForAlbum(IList<AlbumInfo> albumList)
    {
        bool isModify = false;

        for (int i = 0; i < albumList.Count; i++)
        {
            (bool modifySuccess, AlbumInfo albumInfo) = await TryFillArtistAndCachedCoverForAlbum(albumList[i]);

            if (modifySuccess)
            {
                albumList[i] = albumInfo;
                isModify = true;
            }
        }

        return isModify;
    }
}
