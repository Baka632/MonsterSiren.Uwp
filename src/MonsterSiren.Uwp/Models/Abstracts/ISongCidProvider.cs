namespace MonsterSiren.Uwp.Models.Abstracts;

/// <summary>
/// 表示可提供歌曲 CID 的内容。
/// </summary>
public interface ISongCidProvider
{
    /// <summary>
    /// 获取歌曲的 CID 信息。
    /// </summary>
    /// <param name="box">记录枚举过程中异常信息的 <see cref="ExceptionBox"/> 实例。</param>
    /// <returns>一个可异步枚举的 <see cref="IAsyncEnumerable{T}"/> 序列，其中记录了歌曲 CID 数据。</returns>
    IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box);
}
