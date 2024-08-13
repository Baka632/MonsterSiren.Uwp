using System.Net.Http;

namespace MonsterSiren.Uwp.Helpers;

partial class MsrModelsHelper
{
    /// <summary>
    /// 通过专辑的 CID 获取专辑详细信息
    /// </summary>
    /// <param name="cid">专辑的 CID</param>
    /// <returns>包含专辑详细信息的 <see cref="AlbumDetail"/></returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<AlbumDetail> GetAlbumDetailAsync(string cid)
    {
        if (MemoryCacheHelper<AlbumDetail>.Default.TryGetData(cid, out AlbumDetail detail))
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
                    for (int i = 0; i < songs.Count; i++)
                    {
                        SongInfo songInfo = songs[i];
                        if (songInfo.Artists is null || songInfo.Artists.Any() != true)
                        {
                            songs[i] = songInfo with { Artists = ["MSR".GetLocalized()] };
                        }
                    }

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
}
