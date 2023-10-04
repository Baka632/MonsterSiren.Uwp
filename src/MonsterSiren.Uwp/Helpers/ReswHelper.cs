using Windows.ApplicationModel.Resources;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为 RESW 文件中的资源提供访问的类
/// </summary>
public class ReswHelper
{
    /// <summary>
    /// 从 RESW 文件中获取字符串
    /// </summary>
    /// <param name="name">字符串在资源文件中的键</param>
    /// <returns>RESW 文件中的字符串</returns>
    public static string GetReswString(string name)
    {
        ResourceLoader loader = new();
        return loader.GetString(name);
    }
}
