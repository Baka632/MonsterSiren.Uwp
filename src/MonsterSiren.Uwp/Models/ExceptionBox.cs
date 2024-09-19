using System.Runtime.ExceptionServices;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 收集被吞噬掉的异常的盒子，里面的异常随君自取哦❤️
/// </summary>
public sealed class ExceptionBox
{
    /// <summary>
    /// <see cref="ExceptionBox"/> 保存的异常实例
    /// </summary>
    public Exception InboxException { get; set; }

    /// <summary>
    /// 盒子里面究竟有没有异常呢？嘿嘿......
    /// </summary>
    public void Unbox()
    {
        if (InboxException != null)
        {
            ExceptionDispatchInfo.Capture(InboxException).Throw();
        }
    }
}