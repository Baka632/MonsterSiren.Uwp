using System.Net.Http;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private MsrIncrementalCollection<NewsInfo> _newsList;
    [ObservableProperty]
    private MsrIncrementalCollection<AlbumInfo> _albumList;
    [ObservableProperty]
    private bool isAlbumListEmpty;
    [ObservableProperty]
    private bool isNewsListEmpty;

    partial void OnAlbumListChanged(MsrIncrementalCollection<AlbumInfo> value)
    {
        IsAlbumListEmpty = !value.Any();
    }

    partial void OnNewsListChanged(MsrIncrementalCollection<NewsInfo> value)
    {
        IsNewsListEmpty = !value.Any();
    }

    public async Task Initialize(string keyword)
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;

        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            //TODO: Fix missing artist name...

            SearchAlbumAndNewsResult albumAndNewsWarpper = await SearchService.SearchAlbumAndNewsAsync(keyword);

            NewsList = new MsrIncrementalCollection<NewsInfo>(albumAndNewsWarpper.News, async lastInfo => await SearchService.SearchNewsAsync(keyword, lastInfo.Cid));
            AlbumList = new MsrIncrementalCollection<AlbumInfo>(albumAndNewsWarpper.Albums, async lastInfo => await SearchService.SearchAlbumAsync(keyword, lastInfo.Cid));

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