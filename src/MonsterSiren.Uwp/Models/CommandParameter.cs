using System.Windows.Input;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 为命令参数传递提供帮助的结构。
/// </summary>
/// <typeparam name="TParameter">命令参数类型。</typeparam>
/// <param name="Parameter">参数的实例。</param>
/// <param name="Callback">命令完成后的回调命令。</param>
public record CommandParameter(
    object Parameter,
    ICommand Callback);