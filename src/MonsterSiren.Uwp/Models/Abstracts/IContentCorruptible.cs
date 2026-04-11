namespace MonsterSiren.Uwp.Models.Abstracts;

/// <summary>
/// 表示其内容可能出现损坏的内容。
/// </summary>
/// <typeparam name="T">内容的类型。</typeparam>
public interface IContentCorruptible
{
    /// <summary>
    /// 将对象内容中的一个项目标记为损坏。
    /// </summary>
    /// <param name="cid">可标为损坏项目的 CID。CID 的含义由使用场景而定。</param>
    void MarkItemAsCorrupted(string cid);
}
