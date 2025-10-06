using System.Threading;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 正在播放页。
/// </summary>
public sealed partial class NowPlayingPage : Page
{
    private bool isNowPlayingListExpanded = false;

    public NowPlayingViewModel ViewModel { get; }

    public NowPlayingPage()
    {
        this.InitializeComponent();
        ViewModel = new NowPlayingViewModel(this);
    }

    private void OnNowPlayingPageLoaded(object sender, RoutedEventArgs e)
    {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        EntranceStoryboard.Begin();
    }

    private void OnPositionSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (ViewModel.MusicInfo.IsModifyingMusicPositionBySlider)
        {
            ViewModel.MusicInfo.MusicPosition = TimeSpan.FromSeconds(e.NewValue);
        }
    }

    private void OnPositionSliderPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.MusicInfo.IsModifyingMusicPositionBySlider = false;
        ViewModel.UpdateMusicPosition(TimeSpan.FromSeconds(MusicProcessSlider.Value));
    }

    private void OnPositionSliderPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.MusicInfo.IsModifyingMusicPositionBySlider = true;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.NavigationMode == NavigationMode.Back)
        {
            SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
            navigationManager.BackRequested -= MainPage.BackRequested;
            navigationManager.BackRequested += MainPage.BackRequested;
        }

        if (Frame.CanGoBack)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }
        else
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        MusicService.PlayerPlayItemChanged += OnPlayerPlayItemChanged;

        //当在 Code-behind 中添加事件处理器，且 handledEventsToo 设置为 true 时，我们才能捕获到 Slider 的 PointerReleased 与 PointerPressed 这两个事件
        MusicProcessSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(OnPositionSliderPointerReleased), true);
        MusicProcessSlider.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPositionSliderPointerPressed), true);

        if (e.Parameter is bool expandNowPlayingList && expandNowPlayingList && isNowPlayingListExpanded == false)
        {
            ExpandNowPlayingList();
        }
        else
        {
            FoldNowPlayingList();
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        MusicService.PlayerPlayItemChanged -= OnPlayerPlayItemChanged;
        MusicProcessSlider.RemoveHandler(PointerReleasedEvent, new PointerEventHandler(OnPositionSliderPointerReleased));
        MusicProcessSlider.RemoveHandler(PointerPressedEvent, new PointerEventHandler(OnPositionSliderPointerPressed));
    }

    private void OnExpandOrFoldNowPlayingList(object sender, RoutedEventArgs e)
    {
        ExpandOrFoldNowPlayingList();
    }

    private void OnPlayerPlayItemChanged(CurrentMediaPlaybackItemChangedEventArgs args)
    {
        if (args.NewItem is not null)
        {
            NowPlayingListView.ScrollIntoView(args.NewItem, ScrollIntoViewAlignment.Leading);
            NowPlayingListView.SelectedItem = args.NewItem;
        }
    }

    private void ExpandOrFoldNowPlayingList()
    {
        if (isNowPlayingListExpanded || MusicService.IsPlayerPlaylistHasMusic != true)
        {
            FoldNowPlayingList();
        }
        else
        {
            ExpandNowPlayingList();
        }
    }

    private void ExpandNowPlayingList()
    {
        MusicListExpandStoryboard.Begin();
        isNowPlayingListExpanded = true;
    }

    private void FoldNowPlayingList()
    {
        MusicListFoldStoryboard.Begin();
        isNowPlayingListExpanded = false;
    }

    private void OnMusicListExpandStoryboardCompleted(object sender, object e)
    {
        MediaPlaybackItem currentItem = MusicService.CurrentMediaPlaybackItem;
        if (currentItem is not null && NowPlayingListView.Items.Contains(currentItem))
        {
            NowPlayingListView.ScrollIntoView(currentItem, ScrollIntoViewAlignment.Leading);
            NowPlayingListView.SelectedItem = currentItem;
        }
    }

    private void OnVolumeSliderPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        double addDelta = 5d;
        UIElement element = sender as UIElement;
        PointerPoint currentPoint = e.GetCurrentPoint(element);
        int wheelDelta = currentPoint.Properties.MouseWheelDelta;

        if (wheelDelta > 0)
        {
            if (Math.Ceiling(MusicInfoService.Default.Volume + addDelta) >= 100d)
            {
                MusicInfoService.Default.Volume = 100d;
            }
            else
            {
                MusicInfoService.Default.Volume += addDelta;
            }
        }
        else if (wheelDelta < 0)
        {
            if (Math.Floor(MusicInfoService.Default.Volume - addDelta) <= 0d)
            {
                MusicInfoService.Default.Volume = 0d;
            }
            else
            {
                MusicInfoService.Default.Volume -= addDelta;
            }
        }
    }

    private async void OnSongDurationTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        MediaPlaybackItem playbackItem = (MediaPlaybackItem)textBlock.DataContext;
        MediaSource source = playbackItem.Source;
        string sourceUri = source.Uri.ToString();

        textBlock.Text = "-:-";

        if (source.Duration.HasValue)
        {
            textBlock.Text = source.Duration.Value.ToString(@"m\:ss");
        }
        else
        {
            SemaphoreSlim semaphore = CommonValues.SongDurationLocker.GetOrCreateLocker(sourceUri);

            try
            {
                await semaphore.WaitAsync();

                if (MemoryCacheHelper<SongDetail>.Default.TryQueryData(detail => new Uri(detail.SourceUrl, UriKind.Absolute) == source.Uri, out IEnumerable<SongDetail> details))
                {
                    SongDetail songDetail = details.FirstOrDefault();
                    TimeSpan? span = await FileCacheHelper.GetSongDurationAsync(songDetail.Cid);

                    if (span.HasValue)
                    {
                        textBlock.Text = span.Value.ToString(@"m\:ss");
                        return;
                    }
                }

                await source.OpenAsync();
                TimeSpan? duration = source.Duration;

                if (duration.HasValue)
                {
                    textBlock.Text = duration.Value.ToString(@"m\:ss");
                }
                else
                {
                    textBlock.Text = "-:-";
                }
            }
            catch (Exception ex) when (ex.HResult == -1072877849)
            {
                textBlock.Text = "-:-";
            }
            finally
            {
                semaphore.Release();
                CommonValues.SongDurationLocker.ReturnLocker(sourceUri);
            }
        }
    }

    private void GestureReceiverGridManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {

    }

    private void OnGestureReceiverGridManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {

    }

    private void OnGestureReceiverGridManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        double y = e.Cumulative.Translation.Y;

        if (y > 0)
        {
            FoldNowPlayingList();
        }
        else if (y < 0)
        {
            ExpandNowPlayingList();
        }
    }

    private bool isHandlingRootGridPointerWheelChangedEvent = false;

    private void OnRootGridPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (isHandlingRootGridPointerWheelChangedEvent)
        {
            return;
        }
        isHandlingRootGridPointerWheelChangedEvent = true;

        try
        {
            UIElement element = sender as UIElement;
            PointerPoint currentPoint = e.GetCurrentPoint(element);
            PointerPointProperties properties = currentPoint.Properties;
            int wheelDelta = properties.MouseWheelDelta;

            if (!properties.IsHorizontalMouseWheel && Math.Abs(wheelDelta) > 40)
            {
                if (wheelDelta > 0)
                {
                    FoldNowPlayingList();
                }
                else if (wheelDelta < 0)
                {
                    ExpandNowPlayingList();
                }
            }
        }
        finally
        {
            isHandlingRootGridPointerWheelChangedEvent = false;
        }
    }
}
