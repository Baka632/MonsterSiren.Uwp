﻿using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;

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

    public async void HandleNewsListItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not NewsInfo newsInfo)
        {
            throw new ArgumentException($"{nameof(e.ClickedItem)} 应当为一个 {nameof(NewsInfo)} 的实例。");
        }

        try
        {
            if (!MemoryCacheHelper<NewsDetail>.Default.TryGetData(newsInfo.Cid, out NewsDetail newsDetail))
            {
                newsDetail = await NewsService.GetDetailedNewsInfoAsync(newsInfo.Cid);
                MemoryCacheHelper<NewsDetail>.Default.Store(newsInfo.Cid, newsDetail);
            }

            ContentFrameNavigationHelper.Navigate(typeof(NewsDetailPage), newsDetail, CommonValues.DefaultTransitionInfo);
        }
        catch (HttpRequestException ex)
        {
            ShowInternetError(ex);
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