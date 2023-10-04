namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为本地化提供帮助方法的类
/// </summary>
public static class LocalizationHelper
{
    /// <summary>
    /// 获取本地化的字符串
    /// </summary>
    /// <param name="key">本地化的字符串在资源文件中的键</param>
    /// <returns>已本地化的字符串，若找不到相应的字符串，则返回 <see langword="null"/></returns>
    public static string GetLocalized(this string key)
    {
        return ReswHelper.GetReswString(key) ?? null;
    }
}
