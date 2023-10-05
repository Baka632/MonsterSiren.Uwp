using Windows.System.Profile;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为确定当前设备环境提供帮助方法的类
/// </summary>
public static class EnvironmentHelper
{
    /// <summary>
    /// 确定设备是否为 Windows 10 Mobile
    /// </summary>
    /// <returns>若是 Windows 10 Mobile，则返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    public static bool IsWindowsMobile()
    {
        return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
    }

    public static bool IsWindows11()
    {
        return Environment.OSVersion.Version.Build >= 22000;
    }
}
