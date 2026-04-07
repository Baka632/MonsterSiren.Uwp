using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="SongInfo"/> 序列提供服务的适配器。
/// </summary>
/// <param name="songInfos">指定的 <see cref="SongInfo"/> 实例。</param>
public sealed class SongInfoSequenceAdapter(IEnumerable<SongInfo> songInfos) : IPlayable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        foreach (SongInfo info in songInfos.ToArray())
        {
            yield return info.Cid;
        }
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