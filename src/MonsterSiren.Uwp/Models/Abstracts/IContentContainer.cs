namespace MonsterSiren.Uwp.Models.Abstracts;

/// <summary>
/// 表示内容的容器。
/// </summary>
public interface IContentContainer
{
    /// <summary>
    /// 指示容器是否为空的值。
    /// </summary>
    bool IsEmpty { get; }
}
