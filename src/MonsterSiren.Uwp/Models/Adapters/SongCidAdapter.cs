using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为歌曲 CID 提供服务的适配器。
/// </summary>
/// <param name="songCid">歌曲 CID。</param>
public sealed class SongCidAdapter(string songCid) : ISongCidProvider
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        yield return songCid;
    }
}
