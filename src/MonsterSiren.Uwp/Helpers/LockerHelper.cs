using System.Threading;
using System.Collections.Concurrent;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为应用程序提供锁的帮助类。
/// </summary>
/// <typeparam name="T">作为键值的类型。</typeparam>
public class LockerHelper<T>
{
    private readonly ConcurrentDictionary<T, SemaphoreCountWrapper> objectLockerPairs = [];

    /// <summary>
    /// 获取或创建锁对象。
    /// </summary>
    /// <remarks>
    /// 若要清理通过此方法获得的锁对象，请不要调用 <see cref="SemaphoreSlim.Dispose()"/>，而是调用 <see cref="ReturnLocker(T)"/>。
    /// </remarks>
    /// <param name="obj">作为键值的对象。</param>
    /// <returns>一个 <see cref="SemaphoreSlim"/>。</returns>
    public SemaphoreSlim GetOrCreateLocker(T obj)
    {
        SemaphoreCountWrapper wrapper = new()
        {
            Semaphore = new SemaphoreSlim(1),
            UsageCount = 1
        };
        SemaphoreCountWrapper result = objectLockerPairs.GetOrAdd(obj, wrapper);

        if (!ReferenceEquals(wrapper, result))
        {
            // TODO: 这里可能有线程安全问题
            result.UsageCount++;
            wrapper.Dispose();
        }

        return result.Semaphore;
    }

    /// <summary>
    /// 归还锁对象。
    /// </summary>
    /// <param name="obj">作为键值的对象。</param>
    public void ReturnLocker(T obj)
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

    private sealed class SemaphoreCountWrapper : IDisposable
    {
        public SemaphoreSlim Semaphore { get; set; }
        public int UsageCount { get; set; }

        public void Dispose()
        {
            Semaphore.Dispose();
        }
    }
}