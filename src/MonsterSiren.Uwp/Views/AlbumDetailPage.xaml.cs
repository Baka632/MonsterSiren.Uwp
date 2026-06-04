using System.Net.Http;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 专辑详细信息页。
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
            await ViewModel.Initialize(albumInfo.Name, albumInfo.Cid, albumInfo.Artistes, albumInfo.Intro, null,
                                       albumInfo.CoverUrl).ConfigureAwait(false);
        }
        else if (e.Parameter is ValueTuple<AlbumInfo, bool> tuple)
        {
            albumInfo = tuple.Item1;
            enableBackAnimation = tuple.Item2;

            await ViewModel.Initialize(albumInfo.Name, albumInfo.Cid, albumInfo.Artistes, albumInfo.Intro, null,
                                       albumInfo.CoverUrl).ConfigureAwait(false);
        }
        else if (e.Parameter is AlbumDetail detail)
        {
            enableBackAnimation = false;
            await ViewModel.Initialize(detail.Name, detail.Cid, null, detail.Intro, detail.Songs,
                                       detail.CoverUrl).ConfigureAwait(false);
        }
        else if (e.Parameter is AlbumFavoriteItem item)
        {
            enableBackAnimation = false;
            await ViewModel.Initialize(item.AlbumName, item.AlbumCid, item.Artistes).ConfigureAwait(false);
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        if (enableBackAnimation && e.NavigationMode == NavigationMode.Back)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(CommonValues.AlbumInfoBackConnectedAnimationKeyForMusicPage, AlbumCover);
        }
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        DragHelper.WriteDataToDragItemsStartingEventArgs<SongInfo>(e);
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

    private void OnIndexTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        int index = SongList.Items.IndexOf(textBlock.DataContext);
        textBlock.Text = $"{index + 1}.";
    }

    private async void OnListViewItemGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        SongInfo songInfo = (SongInfo)element.DataContext;

        await CommonValues.StartPlay(songInfo.ToAdapter());
    }
}
