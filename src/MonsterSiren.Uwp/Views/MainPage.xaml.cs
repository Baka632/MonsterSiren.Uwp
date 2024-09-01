using System.Collections.Specialized;
using System.Text.Json;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Media.Animation;
using MUXCNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

// TODO: 优化一下播放控制区中歌曲名的布局

/// <summary>
/// 应用主页面
/// </summary>
public sealed partial class MainPage : Page
{
    private bool IsTitleBarTextBlockForwardBegun = false;
    private bool IsFirstRun = true;
    private long tokenForPlaylistPageItemIsExpandChangedEvent;

    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = new MainViewModel(this);

        this.InitializeComponent();

        NavigationCacheMode = NavigationCacheMode.Enabled;

        AutoSetMainPageBackground();
        ConfigureTitleBar();

        ContentFrameNavigationHelper = new NavigationHelper(ContentFrame);
        ContentFrameNavigationHelper.Navigate(typeof(MusicPage));
        ChangeSelectedItemOfNavigationView();
        LoadPlaylistForNavigationView();
    }

    ~MainPage()
    {
        MainPageNavigationHelper.GoBackComplete -= OnMainPageGoBackComplete;
        MainPageNavigationHelper = null;
    }

    private void ConfigureTitleBar()
    {
        if (EnvironmentHelper.IsWindowsMobile)
        {
            TitleBarTextBlock.Visibility = Visibility.Collapsed;
        }
        else
        {
            TitleBarHelper.Hook(ContentFrame);
            TitleBarHelper.BackButtonVisibilityChanged += OnBackButtonVisibilityChanged;
            TitleBarHelper.TitleBarVisibilityChanged += OnTitleBarVisibilityChanged;
        }
    }

    private void AutoSetMainPageBackground()
    {
        if (SettingsHelper.TryGet(CommonValues.AppBackgroundModeSettingsKey, out string modeString) && Enum.TryParse(modeString, out AppBackgroundMode mode))
        {
            // :D
        }
        else
        {
            if (MicaHelper.IsSupported())
            {
                mode = AppBackgroundMode.Mica;
            }
            else if (AcrylicHelper.IsSupported())
            {
                mode = AppBackgroundMode.Acrylic;
            }
            else
            {
                mode = AppBackgroundMode.PureColor;
            }
        }

        SetMainPageBackground(mode);
    }

    public bool SetMainPageBackground(AppBackgroundMode mode)
    {
        switch (mode)
        {
            case AppBackgroundMode.Acrylic:
                return AcrylicHelper.TrySetAcrylicBrush(this);
            case AppBackgroundMode.Mica:
                // 设置 Mica 时，要将控件背景设置为透明
                Background = new SolidColorBrush(Colors.Transparent);
                return MicaHelper.TrySetMica(this);
            case AppBackgroundMode.PureColor:
            default:
                Background = Resources["ApplicationPageBackgroundThemeBrush"] as Brush;
                return true;
        }
    }

    internal static void BackRequested(object sender, BackRequestedEventArgs e)
    {
        if (MainPageNavigationHelper.CanGoBack)
        {
            MainPageNavigationHelper.GoBack(e);
        }
        else if(ContentFrameNavigationHelper.CanGoBack)
        {
            ContentFrameNavigationHelper.GoBack(e);
        }
    }

    private void OnTitleBarVisibilityChanged(object sender, CoreApplicationViewTitleBar bar)
    {
        if (bar.IsVisible)
        {
            StartTitleBarAnimation(Visibility.Visible);
        }
        else
        {
            StartTitleBarAnimation(Visibility.Collapsed);
        }
    }

    private void OnBackButtonVisibilityChanged(object sender, AppViewBackButtonVisibility args)
    {
        StartTitleTextBlockAnimation(args);
    }

    private void StartTitleTextBlockAnimation(AppViewBackButtonVisibility buttonVisibility)
    {
        switch (buttonVisibility)
        {
            case AppViewBackButtonVisibility.Disabled:
            case AppViewBackButtonVisibility.Visible:
                if (IsTitleBarTextBlockForwardBegun)
                {
                    goto default;
                }
                TitleBarTextBlockForward.Begin();
                IsTitleBarTextBlockForwardBegun = true;
                break;
            case AppViewBackButtonVisibility.Collapsed:
                TitleBarTextBlockBack.Begin();
                IsTitleBarTextBlockForwardBegun = false;
                break;
            default:
                break;
        }
    }

    private void StartTitleBarAnimation(Visibility visibility)
    {
        if (IsFirstRun)
        {
            IsFirstRun = false;
            TitleBar.Visibility = visibility;
            return;
        }

        switch (visibility)
        {
            case Visibility.Visible:
                TitleBarShow.Begin();
                break;
            case Visibility.Collapsed:
            default:
                break;
        }
        TitleBar.Visibility = visibility;
    }

    private void OnNavigationViewItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is null)
        {
            return;
        }

        string tag = args.InvokedItemContainer.Tag as string;
        if (args.IsSettingsInvoked && ContentFrame.CurrentSourcePageType != typeof(SettingsPage))
        {
            ContentFrameNavigationHelper.Navigate(typeof(SettingsPage), transitionInfo: CommonValues.DefaultTransitionInfo);
        }
        else
        {
            NavigateByNavViewItemTag(tag);
        }
    }

    private void NavigateByNavViewItemTag(string tag)
    {
        if (tag == "MusicPage" && ContentFrame.CurrentSourcePageType != typeof(MusicPage))
        {
            ContentFrameNavigationHelper.Navigate(typeof(MusicPage), transitionInfo: CommonValues.DefaultTransitionInfo);
        }
        else if (tag == "NowPlayingPage" && ContentFrame.CurrentSourcePageType != typeof(NowPlayingPage))
        {
            NavigateToNowPlayingPage(true);
        }
        else if (tag == "NewsPage" && ContentFrame.CurrentSourcePageType != typeof(NewsPage))
        {
            ContentFrameNavigationHelper.Navigate(typeof(NewsPage), transitionInfo: CommonValues.DefaultTransitionInfo);
        }
        else if (tag == "DownloadPage" && ContentFrame.CurrentSourcePageType != typeof(DownloadPage))
        {
            ContentFrameNavigationHelper.Navigate(typeof(DownloadPage), transitionInfo: CommonValues.DefaultTransitionInfo);
        }
        else if (tag == "PlaylistPage" && ContentFrame.CurrentSourcePageType != typeof(PlaylistPage))
        {
            ContentFrameNavigationHelper.Navigate(typeof(PlaylistPage), transitionInfo: CommonValues.DefaultTransitionInfo);
        }
    }

    private void NavigateToNowPlayingPage(bool expandNowPlayingList = false)
    {
        StartTitleTextBlockAnimation(AppViewBackButtonVisibility.Visible);
        MainPageNavigationHelper.Navigate(typeof(NowPlayingPage), expandNowPlayingList, new DrillInNavigationTransitionInfo());
    }

    private void OnContentFrameNavigated(object sender, NavigationEventArgs e)
    {
        if (e.NavigationMode == NavigationMode.Back)
        {
            ChangeSelectedItemOfNavigationView();
        }
    }

    /// <summary>
    /// 改变导航视图的选择项
    /// </summary>
    private void ChangeSelectedItemOfNavigationView()
    {
        Type currentSourcePageType = ContentFrame.CurrentSourcePageType;

        if (currentSourcePageType == typeof(MusicPage) || currentSourcePageType == typeof(AlbumDetailPage))
        {
            NavigationView.SelectedItem = MusicPageItem;
        }
        else if (currentSourcePageType == typeof(NowPlayingPage))
        {
            NavigationView.SelectedItem = NowPlayingPageItem;
        }
        else if (currentSourcePageType == typeof(DownloadPage))
        {
            NavigationView.SelectedItem = DownloadPageItem;
        }
        else if (currentSourcePageType == typeof(PlaylistPage) || currentSourcePageType == typeof(PlaylistDetailPage))
        {
            NavigationView.SelectedItem = PlaylistPageItem;
        }
        else if (currentSourcePageType == typeof(NewsPage) || currentSourcePageType == typeof(NewsDetailPage))
        {
            NavigationView.SelectedItem = NewsPageItem;
        }
        else if (currentSourcePageType == typeof(SettingsPage))
        {
            NavigationView.SelectedItem = NavigationView.SettingsItem;
        }
        else if (currentSourcePageType == typeof(SearchPage))
        {
            NavigationView.SelectedItem = null;
        }
#if DEBUG
        else
        {
            System.Diagnostics.Debugger.Break();
        }
#endif
    }

    private void OnMainPageLoaded(object sender, RoutedEventArgs e)
    {
        if (MainPageNavigationHelper is null)
        {
            MainPageNavigationHelper = new NavigationHelper(Frame);
            MainPageNavigationHelper.GoBackComplete += OnMainPageGoBackComplete;
        }
    }

    private void OnMainPageGoBackComplete(object sender, EventArgs arg)
    {
        AppViewBackButtonVisibility backButtonVisibility = ContentFrame.CanGoBack
                ? AppViewBackButtonVisibility.Visible
                : AppViewBackButtonVisibility.Collapsed;
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = backButtonVisibility;
        StartTitleTextBlockAnimation(backButtonVisibility);
        ChangeSelectedItemOfNavigationView();
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

    private void OnMediaInfoButtonClick(object sender, RoutedEventArgs e)
    {
        NavigateToNowPlayingPage();
    }

    private void LoadPlaylistForNavigationView()
    {
        PlaylistPageItem.MenuItems.Clear();
        foreach (Playlist playlist in PlaylistService.TotalPlaylists)
        {
            MUXCNavigationViewItem item = CreateNavItemByPlaylist(playlist);
            PlaylistPageItem.MenuItems.Add(item);
        }
    }

    private MUXCNavigationViewItem CreateNavItemByPlaylist(Playlist playlist)
    {
        MUXCNavigationViewItem item = new()
        {
            DataContext = playlist,
            ContextFlyout = PlaylistItemFlyout,
            Tag = playlist,
            Icon = new FontIcon()
            {
                Glyph = "\uEC4F"
            },
            IsRightTapEnabled = true,
            AllowDrop = true,
            SelectsOnInvoked = false,
        };

        Binding binding = new()
        {
            Path = new PropertyPath("Title")
        };

        item.SetBinding(ContentControl.ContentProperty, binding);

        item.Tapped += (s, e) =>
        {
            if (ContentFrame.Content is PlaylistDetailPage page && page.ViewModel?.CurrentPlaylist == playlist)
            {
                return;
            }

            ContentFrameNavigationHelper.Navigate(typeof(PlaylistDetailPage), playlist, CommonValues.DefaultTransitionInfo);

            ChangeSelectedItemOfNavigationView();
        };

        item.RightTapped += (s, e) =>
        {
            ViewModel.SelectedPlaylist = playlist;
        };

        item.DragOver += (s, e) =>
        {
            if (e.DataView.Contains(CommonValues.MusicAlbumInfoFormatId)
                || e.DataView.Contains(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId)
                || e.DataView.Contains(CommonValues.MusicPlaylistItemFormatId))
            {
                e.AcceptedOperation = DataPackageOperation.Link;
                e.DragUIOverride.Caption = "AddToPlaylistLiteral".GetLocalized();
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        };

        item.Drop += async (s, e) =>
        {
            if (e.DataView.Contains(CommonValues.MusicAlbumInfoFormatId))
            {
                string json = (string)await e.DataView.GetDataAsync(CommonValues.MusicAlbumInfoFormatId);

                AlbumInfo albumInfo = JsonSerializer.Deserialize<AlbumInfo>(json);

                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid).ConfigureAwait(false);

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                    await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, albumDetail);
                }
            }
            else if (e.DataView.Contains(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId))
            {
                string json = (string)await e.DataView.GetDataAsync(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId);

                (SongInfo songInfo, AlbumDetail albumDetail) = JsonSerializer.Deserialize<SongInfoAndAlbumDetailPack>(json);
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);

                await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, albumDetail);
            }
            else if (e.DataView.Contains(CommonValues.MusicPlaylistItemFormatId))
            {
                string json = (string)await e.DataView.GetDataAsync(CommonValues.MusicPlaylistItemFormatId);

                PlaylistItem item = JsonSerializer.Deserialize<PlaylistItem>(json);
                await PlaylistService.AddItemForPlaylistAsync(playlist, item);
            }
        };
        return item;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
        navigationManager.BackRequested -= BackRequested; //防止重复添加事件订阅
        navigationManager.BackRequested += BackRequested;

        NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
        OnNetworkStatusChanged();

        PlaylistService.TotalPlaylists.CollectionChanged += OnTotalPlaylistsCollectionChanged;

        // 当在 Code-behind 中添加事件处理器，且 handledEventsToo 设置为 true 时
        // 我们才能捕获到 e.Handled 被设为 true 的事件
        MusicProcessSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(OnPositionSliderPointerReleased), true);
        MusicProcessSlider.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPositionSliderPointerPressed), true);

        tokenForPlaylistPageItemIsExpandChangedEvent = PlaylistPageItem.RegisterPropertyChangedCallback(MUXCNavigationViewItem.IsExpandedProperty, OnPlaylistPageItemIsExpandedChanged);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        if (NavigationView?.SettingsItem is Microsoft.UI.Xaml.Controls.NavigationViewItemBase settings)
        {
            settings.AccessKeyInvoked -= OnNavigationViewItemAccessKeyInvoked;
        }
        NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;
        PlaylistService.TotalPlaylists.CollectionChanged -= OnTotalPlaylistsCollectionChanged;

        MusicProcessSlider.RemoveHandler(PointerReleasedEvent, new PointerEventHandler(OnPositionSliderPointerReleased));
        MusicProcessSlider.RemoveHandler(PointerPressedEvent, new PointerEventHandler(OnPositionSliderPointerPressed));

        PlaylistPageItem.UnregisterPropertyChangedCallback(MUXCNavigationViewItem.IsExpandedProperty, tokenForPlaylistPageItemIsExpandChangedEvent);
    }

    private void OnPlaylistPageItemIsExpandedChanged(DependencyObject sender, DependencyProperty dp)
    {
        if (dp == MUXCNavigationViewItem.IsExpandedProperty)
        {
            bool isExpanded = (bool)sender.GetValue(dp);

            if (!isExpanded)
            {
                sender.SetValue(dp, true);
            }
        }
    }

    private void OnTotalPlaylistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        IList<object> target = PlaylistPageItem.MenuItems;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    foreach (object item in e.NewItems)
                    {
                        if (item is Playlist playlist)
                        {
                            MUXCNavigationViewItem navItem = CreateNavItemByPlaylist(playlist);
                            target.Add(navItem);
                        }
                    }
                }
                break;
            case NotifyCollectionChangedAction.Move:
                object oldItem = target[e.OldStartingIndex];
                target.RemoveAt(e.OldStartingIndex);
                target.Insert(e.NewStartingIndex, oldItem);
                break;
            case NotifyCollectionChangedAction.Remove:
                target.RemoveAt(e.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                {
                    object newPlaylist = e.NewItems[0];
                    if (newPlaylist is Playlist playlist)
                    {
                        MUXCNavigationViewItem navItem = CreateNavItemByPlaylist(playlist);
                        target[e.OldStartingIndex] = navItem;
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                target.Clear();
                break;
            default:
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                break;
        }
    }

    private async void OnNetworkStatusChanged(object sender = null)
    {
        ConnectionCost costInfo = NetworkInformation.GetInternetConnectionProfile()?.GetConnectionCost();

        if (costInfo?.NetworkCostType is NetworkCostType.Fixed or NetworkCostType.Variable)
        {
            await UIThreadHelper.RunOnUIThread(() => appNotificationControl.Show("UsingMeteredInternet".GetLocalized(), 5000));
        }
    }

    private void OnMusicDataPackageDragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(CommonValues.MusicAlbumInfoFormatId)
            || e.DataView.Contains(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId)
            || e.DataView.Contains(CommonValues.MusicPlaylistItemFormatId)
            || e.DataView.Contains(CommonValues.MusicPlaylistFormatId))
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.Caption = "AddToNowPlayingLiteral".GetLocalized();
        }
        else
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }
    }

    private async void OnDropMusicDataPackage(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(CommonValues.MusicAlbumInfoFormatId))
        {
            string json = (string)await e.DataView.GetDataAsync(CommonValues.MusicAlbumInfoFormatId);

            AlbumInfo albumInfo = JsonSerializer.Deserialize<AlbumInfo>(json);

            await MainViewModel.AddToNowPlayingForAlbumInfo(albumInfo);
        }
        else if (e.DataView.Contains(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId))
        {
            string json = (string)await e.DataView.GetDataAsync(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId);

            SongInfoAndAlbumDetailPack pack = JsonSerializer.Deserialize<SongInfoAndAlbumDetailPack>(json);

            await MainViewModel.AddToNowPlayingForSongInfo(pack.SongInfo, pack.AlbumDetail);
        }
        else if (e.DataView.Contains(CommonValues.MusicPlaylistItemFormatId))
        {
            string json = (string)await e.DataView.GetDataAsync(CommonValues.MusicPlaylistItemFormatId);
            PlaylistItem item = JsonSerializer.Deserialize<PlaylistItem>(json);
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);

            await MainViewModel.AddToNowPlayingForSongDetail(songDetail, albumDetail);
        }
        else if (e.DataView.Contains(CommonValues.MusicPlaylistFormatId))
        {
            string json = (string)await e.DataView.GetDataAsync(CommonValues.MusicPlaylistFormatId);

            Playlist playlist = JsonSerializer.Deserialize<Playlist>(json);

            await PlaylistService.AddPlaylistToNowPlayingAsync(playlist);
        }
    }

    private void OnAutoSuggestBoxAccessKeyInvoked(UIElement sender, AccessKeyInvokedEventArgs args)
    {
        Control control = sender as Control;
        args.Handled = control.Focus(FocusState.Programmatic);
    }

    private void OnNavigationViewItemAccessKeyInvoked(UIElement sender, AccessKeyInvokedEventArgs args)
    {
        if (sender == NavigationView.SettingsItem && ContentFrame.CurrentSourcePageType != typeof(SettingsPage))
        {
            ContentFrameNavigationHelper.Navigate(typeof(SettingsPage), transitionInfo: CommonValues.DefaultTransitionInfo);
        }
        else
        {
            var item = sender as Microsoft.UI.Xaml.Controls.NavigationViewItemBase;

            string tag = item.Tag as string;
            NavigateByNavViewItemTag(tag);
        }
    }

    private void OnNavigationViewLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.NavigationView view && view.SettingsItem is Microsoft.UI.Xaml.Controls.NavigationViewItemBase settings)
        {
            settings.AccessKeyInvoked += OnNavigationViewItemAccessKeyInvoked;
            settings.AccessKey = "T";
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

    internal static Visibility ShowDownloadItemInfoBadge(int count)
    {
        return XamlHelper.ToVisibility(count > 0);
    }

    private void OnNavigationViewDisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
    {
        if (args.DisplayMode != Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
        {
            ContentFrame.Margin = new Thickness(20, 20, 0, 0);
        }
        else if (EnvironmentHelper.IsWindowsMobile != true)
        {
            ContentFrame.Margin = new Thickness(12, 45, 0, 0);
        }
    }

    private async void OnNavigationViewSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            await ViewModel.UpdateAutoSuggestBoxSuggestion(sender.Text);
        }
    }

    private void OnNavigationViewSearchBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.QueryText == string.Empty)
        {
            return;
        }
        else if (string.IsNullOrWhiteSpace(args.QueryText))
        {
            NotifyNoEmptyStringTeachingTip.IsOpen = true;
            return;
        }

        NotifyNoEmptyStringTeachingTip.IsOpen = false;
        ContentFrameNavigationHelper.Navigate(typeof(SearchPage), args.QueryText, CommonValues.DefaultTransitionInfo);
    }

    private void OnPlaylistItemFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddPlaylistToNowPlayingCommand,
                                                                          ViewModel.SelectedPlaylist,
                                                                          ViewModel.AddPlaylistToAnotherPlaylistCommand,
                                                                          ViewModel.SelectedPlaylist);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }
}
