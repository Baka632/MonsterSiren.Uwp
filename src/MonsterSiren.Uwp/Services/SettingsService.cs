using Windows.Storage;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 为读取、存储及修改设置提供帮助的类
/// </summary>
public static class SettingsService
{
    private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

    /// <summary>
    /// 使用指定的键获取存储的值
    /// </summary>
    /// <typeparam name="T">在设置中存储的类型</typeparam>
    /// <param name="key">值的存储键</param>
    /// <returns><typeparamref name="T"/> 的实例</returns>
    /// <exception cref="ArgumentException"><paramref name="key"/> 为 <see langword="null"/> 或空白</exception>
    public static T Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"“{nameof(key)}”不能为 null 或空白。", nameof(key));
        }

        T val = (T)localSettings.Values[key];
        return val;
    }

    /// <summary>
    /// 尝试使用指定的键获取存储的值
    /// </summary>
    /// <typeparam name="T">在设置中存储的类型</typeparam>
    /// <param name="key">值的存储键</param>
    /// <param name="result">若成功获取在设置中寻找到指定的值，则返回 <typeparamref name="T"/> 的实例，否则返回 <typeparamref name="T"/> 的默认值</param>
    /// <returns>指示过程是否成功的值</returns>
    public static bool TryGet<T>(string key, out T result)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            result = default;
            return false;
        }

        if (localSettings.Values.TryGetValue(key, out object val) && val is T target)
        {
            result = target;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// 使用指定的键修改存储的值，或将指定的值存储到系统设置容器中
    /// </summary>
    /// <typeparam name="T">指定值的类型</typeparam>
    /// <param name="key">在系统设置容器中的键</param>
    /// <param name="value">指定值的实例。有关可以存储的值，请参阅 <see href="https://learn.microsoft.com/windows/apps/design/app-settings/store-and-retrieve-app-data#settings"/></param>
    /// <exception cref="ArgumentException"><paramref name="key"/> 为 <see langword="null"/> 或空白</exception>
    public static void Set<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"“{nameof(key)}”不能为 null 或空白。", nameof(key));
        }

        localSettings.Values[key] = value;
    }

    /// <summary>
    /// 确定设置中是否包含指定的键
    /// </summary>
    /// <param name="key">要检查是否存在的键</param>
    /// <returns>若存在，则返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    public static bool Exists(string key)
    {
        return localSettings.Values.ContainsKey(key);
    }
}
