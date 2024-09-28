using System.Collections.Concurrent;
using System.Threading;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为应用程序提供锁的帮助类
/// </summary>
/// <typeparam name="T">作为键值的类型</typeparam>
public static class LockerHelper<T>
{
    private static readonly ConcurrentDictionary<T, SemaphoreCountWrapper> objectLockerPairs = [];

    /// <summary>
    /// 获取或创建锁对象。若要清理通过此方法获得的锁对象，请不要调用 <see cref="SemaphoreSlim.Dispose()"/>，而是调用 <see cref="ReturnLocker(T)"/>
    /// </summary>
    /// <param name="obj">作为键值的对象</param>
    /// <returns>一个 <see cref="SemaphoreSlim"/></returns>
    public static SemaphoreSlim GetOrCreateLocker(T obj)
    {
        SemaphoreCountWrapper wrapper = new()
        {
            Semaphore = new SemaphoreSlim(1),
            UsageCount = 1
        };
        SemaphoreCountWrapper result = objectLockerPairs.GetOrAdd(obj, wrapper);

        if (!ReferenceEquals(wrapper, result))
        {
            result.UsageCount++;
            wrapper.Dispose();
        }

        return result.Semaphore;
    }

    /// <summary>
    /// 归还锁对象
    /// </summary>
    /// <param name="obj">作为键值的对象</param>
    public static void ReturnLocker(T obj)
    {
        if (objectLockerPairs.TryGetValue(obj, out SemaphoreCountWrapper wrapper))
        {
            wrapper.UsageCount--;

            if (wrapper.UsageCount == 0)
            {
                objectLockerPairs.TryRemove(obj, out _);
                wrapper.Dispose();
            }
        }
    }
}

internal sealed class SemaphoreCountWrapper : IDisposable
{
    public SemaphoreSlim Semaphore { get; set; }
    public int UsageCount { get; set; }

    public void Dispose()
    {
        Semaphore.Dispose();
    }
}