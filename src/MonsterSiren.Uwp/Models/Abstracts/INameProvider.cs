namespace MonsterSiren.Uwp.Models.Abstracts;

/// <summary>
/// 表示可提供名称的内容。
/// </summary>
public interface INameProvider
{
    /// <summary>
    /// 内容名称。
    /// </summary>
    string Name { get; }
}
