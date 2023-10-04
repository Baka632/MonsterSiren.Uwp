﻿namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示应用内错误信息的结构
/// </summary>
internal record class ErrorInfo
{
    /// <summary>
    /// 错误标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 错误信息
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 导致错误的异常
    /// </summary>
    public Exception Exception { get; set; }
}
