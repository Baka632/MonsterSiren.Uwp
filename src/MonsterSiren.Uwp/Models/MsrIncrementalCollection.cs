using System.Collections.ObjectModel;
using System.Threading;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 使用塞壬唱片相关服务的增量集合
/// </summary>
/// <typeparam name="T">集合类型</typeparam>
public sealed class MsrIncrementalCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading
{
    private T lastObject = default;
    private readonly Func<T, Task<ListPackage<T>>> loadMoreDelegate;

    public event Action<Exception> ErrorOccured;

    public MsrIncrementalCollection(ListPackage<T> listPkg, Func<T, Task<ListPackage<T>>> loadMoreFunc) : base(listPkg.List)
    {
        if (loadMoreFunc is null)
        {
            throw new ArgumentNullException(nameof(loadMoreFunc));
        }
        else
        {
            loadMoreDelegate = loadMoreFunc;
        }

        if (listPkg.IsEnd == true)
        {
            HasMoreItems = false;
        }
        else
        {
            HasMoreItems = true;
            lastObject = listPkg.List.Last();
        }
    }

    public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
    {
        return AsyncInfo.Run(LoadFromServer);
    }

    private async Task<LoadMoreItemsResult> LoadFromServer(CancellationToken token)
    {
        LoadMoreItemsResult result = new();

        try
        {
            ListPackage<T> listPkg = EqualityComparer<T>.Default.Equals(lastObject, default)
                ? await Task.Run(() => loadMoreDelegate(default), token)
                : await Task.Run(() => loadMoreDelegate(lastObject), token);

            uint count = 0;
            foreach (T item in listPkg.List)
            {
                Add(item);
                count++;
            }

            result.Count = count;

            if (listPkg.IsEnd == true)
            {
                HasMoreItems = false;
            }
            else
            {
                HasMoreItems = true;
                lastObject = listPkg.List.Last();
            }
        }
        catch (Exception ex)
        {
            ErrorOccured?.Invoke(ex);
        }

        return result;
    }

    public bool HasMoreItems { get; private set; }
}
