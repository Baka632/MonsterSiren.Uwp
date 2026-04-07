namespace MonsterSiren.Uwp.Models.Abstracts;

/// <summary>
/// 表示可能出现损坏情况的对象。
/// </summary>
public interface ICorruptible
{
    /// <summary>
    /// 将对象标记为损坏。
    /// </summary>
    void MarkAsCorrupted();
}
