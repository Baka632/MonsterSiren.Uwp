// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Services.Store.Engagement; // 别删这个，发布模式要用！
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
        NavigationCacheMode = NavigationCacheMode.Required;
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
        ConnectionCost costInfo = NetworkInformation.GetInternetConnectionProfile()?.GetConnectionCost();

        if (costInfo is null || costInfo.NetworkCostType is NetworkCostType.Fixed or NetworkCostType.Variable)
        {
            return;
        }

        try
        {
            Image image = sender as Image;
            if (image?.DataContext is AlbumInfo info)
            {
                Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(info);
                if (fileCoverUri is null)
                {
                    await FileCacheHelper.StoreAlbumCoverAsync(info);
                }
            }
        }
        catch (Exception ex)
        {
#if !DEBUG
                try
                {
                    StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                    logger.Log("缓存封面图像失败");
                }
                catch
                {
                    // Enough!
                }
#else
            Debug.WriteLine(ex);
            Debugger.Break();
#endif
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

    private void OnGridViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        AlbumInfo albumInfo = (AlbumInfo)element.DataContext;

        ViewModel.SelectedAlbumInfo = albumInfo;
    }

    private void OnAlbumContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddToNowPlayingForAlbumInfoCommand,
                                                                          ViewModel.SelectedAlbumInfo,
                                                                          ViewModel.AddAlbumInfoToPlaylistCommand);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }
}
