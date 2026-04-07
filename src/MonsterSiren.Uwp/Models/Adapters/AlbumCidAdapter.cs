using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为专辑 CID 提供服务的适配器。
/// </summary>
/// <param name="albumCid">专辑 CID。</param>
public sealed class AlbumCidAdapter(string albumCid) : IPlayable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        AlbumDetail detail;
        try
        {
            detail = await MsrModelsHelper.GetAlbumDetailAsync(albumCid);
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
}
