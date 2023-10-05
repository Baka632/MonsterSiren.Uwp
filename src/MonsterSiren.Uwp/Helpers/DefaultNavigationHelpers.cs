namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 提供默认的 <see cref="NavigationHelper"/>
/// </summary>
internal static class DefaultNavigationHelpers
{
    public static NavigationHelper ContentFrameNavigationHelper { get; internal set; }
    public static NavigationHelper MainPageNavigationHelper { get; internal set; }
}
