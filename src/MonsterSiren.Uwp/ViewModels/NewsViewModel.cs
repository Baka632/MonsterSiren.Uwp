using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using MonsterSiren.Api.Models.News;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class NewsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private IList<RecommendedNewsInfo> recommendedNewsInfos;
    [ObservableProperty]
    private NewsItemCollection newsInfos;

    public async Task Initialize()
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;

        try
        {
            RecommendedNewsInfos = (await NewsService.GetRecommendedNewsAsync()).ToList();
            NewsInfos = new NewsItemCollection(await NewsService.GetNewsListAsync());

            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException ex)
        {
            ShowInternetError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowInternetError(HttpRequestException ex)
    {
        ErrorVisibility = Visibility.Visible;
        ErrorInfo = new ErrorInfo()
        {
            Title = "ErrorOccurred".GetLocalized(),
            Message = "InternetErrorMessage".GetLocalized(),
            Exception = ex
        };
    }
}

public sealed class NewsItemCollection : ObservableCollection<NewsInfo>, ISupportIncrementalLoading
{
    private string lastCid = null;

    public event Action<Exception> ErrorOccured;

    public NewsItemCollection(ListPackage<NewsInfo> newsList) : base(newsList.List)
    {
        if (newsList.IsEnd == true)
        {
            HasMoreItems = false;
        }
        else
        {
            HasMoreItems = true;
            lastCid = newsList.List.Last().Cid;
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
            ListPackage<NewsInfo> newsList = string.IsNullOrWhiteSpace(lastCid)
                ? await Task.Run(NewsService.GetNewsListAsync, token)
                : await Task.Run(() => NewsService.GetNewsListAsync(lastCid), token);

            uint count = 0;
            foreach (NewsInfo item in newsList.List)
            {
                Add(item);
                count++;
            }

            result.Count = count;

            if (newsList.IsEnd == true)
            {
                HasMoreItems = false;
            }
            else
            {
                HasMoreItems = true;
                lastCid = newsList.List.Last().Cid;
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