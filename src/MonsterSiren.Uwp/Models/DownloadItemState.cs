namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示 <see cref="DownloadItem"/> 状态的枚举
/// </summary>
public enum DownloadItemState
{
    /// <summary>
    /// 下载中
    /// </summary>
    Downloading,
    /// <summary>
    /// 转码中
    /// </summary>
    Transcoding,
    /// <summary>
    /// 已暂停
    /// </summary>
    Paused,
    /// <summary>
    /// 已成功完成
    /// </summary>
    Done,
    /// <summary>
    /// 出现错误
    /// </summary>
    Error,
    /// <summary>
    /// 已取消
    /// </summary>
    Canceled
}
