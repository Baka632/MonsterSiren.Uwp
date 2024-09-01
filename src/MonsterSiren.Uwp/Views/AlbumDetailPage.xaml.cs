using System.Net.Http;
using System.Text.Json;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 专辑详细信息页
/// </summary>
public sealed partial class AlbumDetailPage : Page
{
    private bool enableBackAnimation = true;

    public AlbumDetailViewModel ViewModel { get; }

    public AlbumDetailPage()
    {
        ViewModel = new AlbumDetailViewModel(this);
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(CommonValues.AlbumInfoForwardConnectedAnimationKeyForMusicPage);
        animation?.TryStart(AlbumCover,
        [
                AlbumName,
                AlbumArtists,
                SeparatorStackPanel,
                DetailScrollViewer,
                ControlBarStackPanel
        ]);

        if (e.Parameter is AlbumInfo albumInfo)
        {
            await ViewModel.Initialize(albumInfo).ConfigureAwait(false);
        }
        else if (e.Parameter is ValueTuple<AlbumInfo, bool> tuple)
        {
            albumInfo = tuple.Item1;
            enableBackAnimation = tuple.Item2;

            await ViewModel.Initialize(albumInfo).ConfigureAwait(false);
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        if (e.NavigationMode == NavigationMode.Back && enableBackAnimation)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(CommonValues.AlbumInfoBackConnectedAnimationKeyForMusicPage, AlbumCover);
        }
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        SongInfoAndAlbumDetailPack pack = new((SongInfo)dataContext, ViewModel.CurrentAlbumDetail);

        string json = JsonSerializer.Serialize(pack);

        e.Data.SetData(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId, json);
    }

    private void OnSongDurationTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        SongInfo songInfo = (SongInfo)textBlock.DataContext;
        textBlock.Text = "-:-";

        _ = Task.Run(async () =>
        {
            try
            {
                SongDetail detail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                TimeSpan? span = await MsrModelsHelper.GetSongDurationAsync(detail);

                await UIThreadHelper.RunOnUIThread(() =>
                {
                    if (span.HasValue)
                    {
                        textBlock.Text = span.Value.ToString(@"m\:ss");
                    }
                    else
                    {
                        textBlock.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (HttpRequestException)
            {
                await UIThreadHelper.RunOnUIThread(() => textBlock.Visibility = Visibility.Collapsed);
            }
        });
    }

    private void OnListViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedSongInfo = (SongInfo)element.DataContext;
    }

    private void OnMoreOptionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        Button button = (Button)sender;
        ViewModel.SelectedSongInfo = (SongInfo)button.DataContext;
    }

    private void OnSongContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddToNowPlayingForSongInfoCommand,
                                                                          ViewModel.SelectedSongInfo,
                                                                          ViewModel.AddToPlaylistForSongInfoCommand);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }

    private void OnListViewItemSongContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;

        flyout.Items.Clear();

        MenuFlyoutItem addToNowPlayingItem = CommonValues.CreateAddToNowPlayingItem(ViewModel.AddToNowPlayingForCurrentAlbumDetailCommand, null);
        MenuFlyoutSubItem addToPlaylistSubItem = CommonValues.CreateAddToPlaylistSubItem(ViewModel.AddToPlaylistForCurrentAlbumDetailCommand);

        flyout.Items.Add(addToNowPlayingItem);
        flyout.Items.Add(addToPlaylistSubItem);
    }

    private void OnSongSelectionFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddToNowPlayingForListViewSelectedItemCommand,
                                                                          null,
                                                                          ViewModel.AddToPlaylistForListViewSelectedItemCommand);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }
}
