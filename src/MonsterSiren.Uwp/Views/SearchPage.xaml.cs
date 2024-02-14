// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using MonsterSiren.Api.Models.News;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class SearchPage : Page
{
    private object _storedGridViewItem;

    public SearchViewModel ViewModel { get; } = new SearchViewModel();

    public SearchPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (_storedGridViewItem is not null && e.NavigationMode == NavigationMode.Back)
        {
            AlbumGridView.ScrollIntoView(_storedGridViewItem);
            AlbumGridView.UpdateLayout();

            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(CommonValues.AlbumInfoBackConnectedAnimationKeyForMusicPage);
            if (animation != null)
            {
                await AlbumGridView.TryStartConnectedAnimationAsync(animation, _storedGridViewItem, "AlbumImage");
            }

            AlbumGridView.Focus(FocusState.Programmatic);
        }
        else if (e.Parameter is string keyword)
        {
            await ViewModel.Initialize(keyword);
        }
    }

    private void OnAlbumGridViewItemClick(object sender, ItemClickEventArgs e)
    {
        _storedGridViewItem = e.ClickedItem;
        AlbumGridView.PrepareConnectedAnimation(CommonValues.AlbumInfoForwardConnectedAnimationKeyForMusicPage, e.ClickedItem, "AlbumImage");
        ContentFrameNavigationHelper.Navigate(typeof(AlbumDetailPage), e.ClickedItem, new SuppressNavigationTransitionInfo());
    }

    private async void OnNewsListViewItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            NewsInfo newsInfo = (NewsInfo)e.ClickedItem;

            if (!MemoryCacheHelper<NewsDetail>.Default.TryGetData(newsInfo.Cid, out NewsDetail newsDetail))
            {
                newsDetail = await NewsService.GetDetailedNewsInfoAsync(newsInfo.Cid);
                MemoryCacheHelper<NewsDetail>.Default.Store(newsInfo.Cid, newsDetail);
            }

            ContentFrameNavigationHelper.Navigate(typeof(NewsDetailPage), newsDetail, CommonValues.DefaultTransitionInfo);
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    private void OnAlbumGridViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        string json = JsonSerializer.Serialize((AlbumInfo)dataContext);
        e.Data.SetData(CommonValues.MusicAlbumInfoFormatId, json);
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
