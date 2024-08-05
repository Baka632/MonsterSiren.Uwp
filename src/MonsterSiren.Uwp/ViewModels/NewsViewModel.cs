using System.Net.Http;

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
    private MsrIncrementalCollection<NewsInfo> newsInfos;

    public async Task Initialize()
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;

        try
        {
            if (MemoryCacheHelper<MsrIncrementalCollection<NewsInfo>>.Default.TryGetData(CommonValues.NewsItemCollectionCacheKey, out MsrIncrementalCollection<NewsInfo> newsInfos))
            {
                NewsInfos = newsInfos;
            }
            else
            {
                NewsInfos = await CreateNewsInfoIncrementalCollection();
                MemoryCacheHelper<MsrIncrementalCollection<NewsInfo>>.Default.Store(CommonValues.NewsItemCollectionCacheKey, NewsInfos);
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
            MsrIncrementalCollection<NewsInfo> newsInfos = await CreateNewsInfoIncrementalCollection();
            NewsInfos = newsInfos;
            MemoryCacheHelper<MsrIncrementalCollection<NewsInfo>>.Default.Store(CommonValues.NewsItemCollectionCacheKey, newsInfos);

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
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
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
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    private static async Task<MsrIncrementalCollection<NewsInfo>> CreateNewsInfoIncrementalCollection()
    {
        return new MsrIncrementalCollection<NewsInfo>(await NewsService.GetNewsListAsync(),
            async lastNewsInfo => await NewsService.GetNewsListAsync(lastNewsInfo.Cid));
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