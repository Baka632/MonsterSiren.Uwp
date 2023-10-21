﻿using Windows.Media.Playback;
using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 正在播放页
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

        //当在Code-behind中添加事件处理器，且handledEventsToo设置为true时，我们才能捕获到Slider的PointerReleased与PointerPressed这两个事件
        MusicProcessSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(OnPositionSliderPointerReleased), true);
        MusicProcessSlider.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPositionSliderPointerPressed), true);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

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
        if (args.NewItem is null)
        {
            MusicListFoldStoryboard.Begin();
            isNowPlayingListExpanded = false;
        }
        else
        {
            NowPlayingListView.ScrollIntoView(args.NewItem);
        }
    }

    private void ExpandOrFoldNowPlayingList()
    {
        if (isNowPlayingListExpanded)
        {
            MusicListFoldStoryboard.Begin();
            isNowPlayingListExpanded = false;
        }
        else
        {
            MusicListExpandStoryboard.Begin();
            isNowPlayingListExpanded = true;
        }
    }

    private void OnMusicListExpandStoryboardCompleted(object sender, object e)
    {
        if (MusicService.CurrentMediaPlaybackItem is not null && NowPlayingListView.Items.Contains(MusicService.CurrentMediaPlaybackItem))
        {
            NowPlayingListView.ScrollIntoView(MusicService.CurrentMediaPlaybackItem);
        }
    }

    private void PlayCommandButtonLoaded(object sender, RoutedEventArgs e)
    {
        AppBarButton barButton = sender as AppBarButton;
        barButton.Command = ViewModel.MoveToIndexCommand;
    }

    private void RemoveCommandButtonLoaded(object sender, RoutedEventArgs e)
    {
        AppBarButton barButton = sender as AppBarButton;
        barButton.Command = ViewModel.RemoveAtCommand;
    }
}
