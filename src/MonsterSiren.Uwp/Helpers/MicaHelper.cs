using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 帮助设置 Mica 背景的类
/// </summary>
public static class MicaHelper
{
    /// <summary>
    /// 尝试将指定的控件的背景设置为 Mica
    /// </summary>
    /// <param name="control">指定的控件</param>
    /// <returns>指示过程是否成功的值</returns>
    public static bool TrySetMica(Control control)
    {
        if (IsSupported())
        {
            BackdropMaterial.SetApplyToRootOrPageBackground(control, true);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 确定当前运行时是否支持 Mica
    /// </summary>
    /// <returns>若支持 Mica，则返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    public static bool IsSupported()
    {
        return EnvironmentHelper.IsWindows11();
    }
}
