using System.Threading;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为检查序列操作是否全部失败提供帮助的结构。
/// </summary>
public struct AllFailedHelper
{
    private int totalCount;
    private int successCount;

    /// <summary>
    /// 开始一次循环迭代。
    /// </summary>
    public void Start()
    {
        Interlocked.Increment(ref totalCount);
    }

    /// <summary>
    /// 标记本次循环迭代成功。
    /// </summary>
    public void Succeed()
    {
        Interlocked.Increment(ref successCount);
    }

    /// <summary>
    /// 检查序列操作是否成功。
    /// </summary>
    /// <returns>指示序列操作是否成功的值。</returns>
    public readonly bool IsAllFailed()
    {
        return successCount == 0 && totalCount > 0;
    }
}
