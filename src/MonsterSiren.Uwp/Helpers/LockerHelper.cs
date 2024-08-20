using System.Collections.Concurrent;
using System.Threading;

namespace MonsterSiren.Uwp.Helpers;

public static class LockerHelper<T>
{
    private static readonly ConcurrentDictionary<T, SemaphoreCountWrapper> objectLockerPairs = [];

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

    public static void RevokeLocker(T obj)
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