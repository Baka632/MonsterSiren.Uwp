// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 应用主页面
/// </summary>
public sealed partial class MainPage : Page
{
    private bool IsTitleBarTextBlockForwardBegun = false;
    private bool IsFirstRun = true;

    public MainViewModel ViewModel { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();
        UIThreadHelper.Initialize(Dispatcher);

        SetMainPageBackground();
        ConfigureTitleBar();

        SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
        navigationManager.BackRequested += BackRequested;
        ContentFrameNavigationHelper = new NavigationHelper(ContentFrame);
        ContentFrameNavigationHelper.Navigate(typeof(MusicPage));
        ChangeSelectedItemOfNavigationView();

        //当在这里添加事件处理器，且handledEventsToo设置为true时，我们才能捕获到Slider的PointerReleased与PointerPressed这两个事件
        MusicProcessSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler(OnPositionSliderPointerReleased), true);
        MusicProcessSlider.AddHandler(PointerPressedEvent, new PointerEventHandler(OnPositionSliderPointerPressed), true);
    }

    private void ConfigureTitleBar()
    {
        if (EnvironmentHelper.IsWindowsMobile())
        {
            TitleBarTextBlock.Visibility = Visibility.Collapsed;
        }
        else
        {
            TitleBarHelper.Hook(ContentFrame);
            TitleBarHelper.BackButtonVisibilityChangedEvent += OnBackButtonVisibilityChanged;
            TitleBarHelper.TitleBarVisibilityChangedEvent += OnTitleBarVisibilityChanged;
        }
    }

    private void SetMainPageBackground()
    {
        if (MicaHelper.IsSupported())
        {
            MicaHelper.TrySetMica(this);
        }
        else if (AcrylicHelper.IsSupported())
        {
            AcrylicHelper.TrySetAcrylicBrush(this);
        }
        else
        {
            Background = Resources["ApplicationPageBackgroundThemeBrush"] as Brush;
        }
    }

    private void BackRequested(object sender, BackRequestedEventArgs e)
    {
        ContentFrameNavigationHelper.GoBack(e);
    }

    private void OnTitleBarVisibilityChanged(CoreApplicationViewTitleBar bar)
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

    private void OnBackButtonVisibilityChanged(BackButtonVisibilityChangedEventArgs args)
    {
        StartTitleTextBlockAnimation(args.BackButtonVisibility);
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
        string str = args.InvokedItemContainer.Tag as string;
        if (args.IsSettingsInvoked && ContentFrame.CurrentSourcePageType != typeof(SettingsPage))
        {
            ContentFrameNavigationHelper.Navigate(typeof(SettingsPage));
        }
        else
        {
            if (str == "MusicPage" && ContentFrame.CurrentSourcePageType != typeof(MusicPage))
            {
                ContentFrameNavigationHelper.Navigate(typeof(MusicPage));
            }
            else if (str == "NowPlayingPage" && ContentFrame.CurrentSourcePageType != typeof(NowPlayingPage))
            {
                ContentFrameNavigationHelper.Navigate(typeof(NowPlayingPage));
            }
            else if (str == "NewsPage" && ContentFrame.CurrentSourcePageType != typeof(NewsPage))
            {
                ContentFrameNavigationHelper.Navigate(typeof(NewsPage));
            }
        }
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
        if (ContentFrame.CurrentSourcePageType == typeof(MusicPage))
        {
            NavigationView.SelectedItem = MusicPageItem;
        }
        else if (ContentFrame.CurrentSourcePageType == typeof(NowPlayingPage))
        {
            NavigationView.SelectedItem = NowPlayingPageItem;
        }
        else if (ContentFrame.CurrentSourcePageType == typeof(NewsPage))
        {
            NavigationView.SelectedItem = NewsPageItem;
        }
        else if (ContentFrame.CurrentSourcePageType == typeof(SettingsPage))
        {
            NavigationView.SelectedItem = NavigationView.SettingsItem;
        }
    }

    private void OnMainPageLoaded(object sender, RoutedEventArgs e)
    {
        MainPageNavigationHelper = new NavigationHelper(Frame);
    }

    private void OnPositionSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (ViewModel.IsModifyingMusicPositionBySlider)
        {
            ViewModel.MusicPosition = TimeSpan.FromSeconds(e.NewValue);
        }
    }

    private void OnPositionSliderPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.IsModifyingMusicPositionBySlider = false;
        ViewModel.UpdateMusicPosition(TimeSpan.FromSeconds(MusicProcessSlider.Value));
    }

    private void OnPositionSliderPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        ViewModel.IsModifyingMusicPositionBySlider = true;
    }
}
