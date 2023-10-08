namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为应用程序数据提供缓存的类
/// </summary>
internal class CacheHelper<T>
{
    private readonly Dictionary<string, T> _cache = new(200);

    public static CacheHelper<T> Default { get; } = new CacheHelper<T>();

    /// <summary>
    /// 使用指定的 Key 存储数据
    /// </summary>
    /// <param name="key">访问缓存数据的键</param>
    /// <param name="value">缓存数据</param>
    public void Store(string key, T value)
    {
        _cache[key] = value;
    }

    /// <summary>
    /// 使用指定的 Key 获取缓存数据
    /// </summary>
    /// <param name="key">访问缓存数据的键</param>
    /// <returns>缓存数据</returns>
    /// <exception cref="KeyNotFoundException">未使用指定的 Key 存储数据</exception>
    public T GetData(string key)
    {
        return _cache[key];
    }

    /// <summary>
    /// 尝试使用指定的 Key 获取缓存数据
    /// </summary>
    /// <param name="key">访问缓存数据的键</param>
    /// <param name="value">缓存数据；若能够使用 <paramref name="key"/>，则返回实际数据，否则返回 <see cref="{T}"/> 的默认值</param>
    /// <returns>指示过程是否成功的值</returns>
    public bool TryGetData(string key, out T value)
    {
        return _cache.TryGetValue(key, out value);
    }

    /// <summary>
    /// 尝试使用指定的 <see cref="Func{T, TResult}"/> 查询缓存数据
    /// </summary>
    /// <param name="predicate">用于选择缓存数据的 <see cref="Func{T, TResult}"/></param>
    /// <param name="value">缓存数据；若能够使用查询到缓存数据，则返回实际数据，否则返回 <see langword="null"/></param>
    /// <returns>指示过程是否成功的值</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="predicate"/> 为空</exception>
    public bool TryQueryData(Func<T, bool> predicate, out IEnumerable<T> value)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        IEnumerable<T> target = _cache.Values.Where(predicate);
        if (target.Any())
        {
            value = target;
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
}
