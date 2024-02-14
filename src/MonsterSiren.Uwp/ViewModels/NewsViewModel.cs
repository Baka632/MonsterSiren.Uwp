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
    private bool isRefreshing = false;
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
            if (MemoryCacheHelper<NewsItemCollection>.Default.TryGetData(CommonValues.NewsItemCollectionCacheKey, out NewsItemCollection newsInfos))
            {
                NewsInfos = newsInfos;
            }
            else
            {
                NewsInfos = new NewsItemCollection(await NewsService.GetNewsListAsync());
                MemoryCacheHelper<NewsItemCollection>.Default.Store(CommonValues.NewsItemCollectionCacheKey, NewsInfos);
            }

            if (MemoryCacheHelper<IList<RecommendedNewsInfo>>.Default.TryGetData(CommonValues.RecommendedNewsInfosCacheKey, out IList<RecommendedNewsInfo> recommendedNews))
            {
                RecommendedNewsInfos = recommendedNews;
            }
            else
            {
                RecommendedNewsInfos = (await NewsService.GetRecommendedNewsAsync()).ToList();
                MemoryCacheHelper<IList<RecommendedNewsInfo>>.Default.Store(CommonValues.RecommendedNewsInfosCacheKey, RecommendedNewsInfos);
            }

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

    [RelayCommand]
    private async Task RefreshNews()
    {
        ErrorVisibility = Visibility.Collapsed;
        try
        {
            NewsItemCollection newsInfos = new(await NewsService.GetNewsListAsync());
            NewsInfos = newsInfos;
            MemoryCacheHelper<NewsItemCollection>.Default.Store(CommonValues.NewsItemCollectionCacheKey, newsInfos);

            List<RecommendedNewsInfo> recommendeds = (await NewsService.GetRecommendedNewsAsync()).ToList();
            if (RecommendedNewsInfos is null || !RecommendedNewsInfos.SequenceEqual(recommendeds))
            {
                RecommendedNewsInfos = recommendeds;
                MemoryCacheHelper<IList<RecommendedNewsInfo>>.Default.Store(CommonValues.RecommendedNewsInfosCacheKey, RecommendedNewsInfos);
            }

            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    public async void HandleNewsListItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not NewsInfo newsInfo)
        {
            throw new ArgumentException($"{nameof(e.ClickedItem)} 应当为一个 {nameof(NewsInfo)} 的实例。");
        }

        await NavigateToNewsDetail(newsInfo.Cid);
    }

    [RelayCommand]
    private static async Task NavigateToNewsDetail(string cid)
    {
        try
        {
            if (!MemoryCacheHelper<NewsDetail>.Default.TryGetData(cid, out NewsDetail newsDetail))
            {
                newsDetail = await NewsService.GetDetailedNewsInfoAsync(cid);
                MemoryCacheHelper<NewsDetail>.Default.Store(cid, newsDetail);
            }

            ContentFrameNavigationHelper.Navigate(typeof(NewsDetailPage), newsDetail, CommonValues.DefaultTransitionInfo);
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
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

    public static async Task DisplayContentDialog(string title, string message, string primaryButtonText = "", string closeButtonText = "")
    {
        ContentDialog contentDialog = new()
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText
        };

        await contentDialog.ShowAsync();
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