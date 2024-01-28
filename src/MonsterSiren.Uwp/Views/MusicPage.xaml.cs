// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Text.Json;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 音乐展示页
/// </summary>
public sealed partial class MusicPage : Page
{
    private object _storedGridViewItem;

    public MusicViewModel ViewModel { get; } = new MusicViewModel();

    public MusicPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    private void OnContentGridViewItemClicked(object sender, ItemClickEventArgs e)
    {
        _storedGridViewItem = e.ClickedItem;
        ContentGridView.PrepareConnectedAnimation(CommonValues.AlbumInfoForwardConnectedAnimationKeyForMusicPage, e.ClickedItem, "AlbumImage");
        ContentFrameNavigationHelper.Navigate(typeof(AlbumDetailPage), e.ClickedItem, new SuppressNavigationTransitionInfo());
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (_storedGridViewItem is not null && e.NavigationMode == NavigationMode.Back)
        {
            ContentGridView.ScrollIntoView(_storedGridViewItem);
            ContentGridView.UpdateLayout();

            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(CommonValues.AlbumInfoBackConnectedAnimationKeyForMusicPage);
            if (animation != null)
            {
                await ContentGridView.TryStartConnectedAnimationAsync(animation, _storedGridViewItem, "AlbumImage");
            }

            ContentGridView.Focus(FocusState.Programmatic);
        }
    }

    private async void OnMusicPageLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.Initialize();
    }

    private void OnGridViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
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

    private async void OnAlbumImageLoaded(object sender, RoutedEventArgs e)
    {
        ConnectionCost costInfo = NetworkInformation.GetInternetConnectionProfile().GetConnectionCost();

        if (costInfo?.NetworkCostType is NetworkCostType.Fixed or NetworkCostType.Variable)
        {
            return;
        }

        Image image = (Image)sender;
        if (image.DataContext is AlbumInfo info)
        {
            Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(info);
            if (fileCoverUri is null)
            {
                await FileCacheHelper.StoreAlbumCoverAsync(info);
            }
        }
    }

    private async void OnRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
    {
        using Deferral deferral = args.GetDeferral();
        await ViewModel.RefreshAlbums();
        deferral.Complete();
    }

    private void RefreshAlbums(object sender, RoutedEventArgs e)
    {
        RefreshActionContainer.RequestRefresh();
    }
}
