namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 音乐播放操作的枚举。
/// </summary>
public enum MusicPlayOperation
{
    /// <summary>
    /// 将正在播放列表的内容替换为新音乐。
    /// </summary>
    Replace,
    /// <summary>
    /// 将新音乐追加到正在播放列表末尾。
    /// </summary>
    Add,
    /// <summary>
    /// 将新音乐插入到当前正在播放音乐的后面。
    /// </summary>
    AddNext
}
