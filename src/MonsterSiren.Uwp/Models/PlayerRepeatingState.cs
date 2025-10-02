namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 指示播放器循环播放状态的枚举。
/// </summary>
public enum PlayerRepeatingState
{
    /// <summary>
    /// 播放器未启用循环播放。
    /// </summary>
    None,
    /// <summary>
    /// 播放器启用了全部循环播放。
    /// </summary>
    RepeatAll,
    /// <summary>
    /// 播放器启用了单曲循环。
    /// </summary>
    RepeatSingle
}
