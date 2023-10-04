using Windows.Foundation.Metadata;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 帮助设置亚克力背景的类
/// </summary>
public static class AcrylicHelper
{
    private const string AcrylicBrushTypeName = "Windows.UI.Xaml.Media.AcrylicBrush";
    private const string DefaultAcrylicBrushResourceName = "SystemControlChromeMediumLowAcrylicWindowMediumBrush";

    /// <summary>
    /// 尝试将指定的控件的背景以默认配置设置为亚克力背景
    /// </summary>
    /// <param name="control">指定的控件</param>
    /// <returns>指示过程是否成功的值</returns>
    public static bool TrySetAcrylicBrush(Control control)
    {
        if (IsSupported() && Application.Current.Resources[DefaultAcrylicBrushResourceName] is AcrylicBrush brush)
        {
            control.Background = brush;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 确定当前运行时是否支持亚克力画笔
    /// </summary>
    /// <returns>若支持亚克力画笔，则返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    public static bool IsSupported()
    {
        return ApiInformation.IsTypePresent(AcrylicBrushTypeName);
    }
}
