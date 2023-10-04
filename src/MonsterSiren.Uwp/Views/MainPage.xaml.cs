// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板
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

        TitleBarHelper.Initialize(ContentFrame);
        NavigationHelper.Initialize(ContentFrame);
        AcrylicHelper.TrySetAcrylicBrush(this);

        if (DeviceFamilyHelper.IsWindowsMobile())
        {
            TitleBarTextBlock.Visibility = Visibility.Collapsed;
        }
        else
        {
            TitleBarHelper.BackButtonVisibilityChangedEvent += OnBackButtonVisibilityChanged;
            TitleBarHelper.TitleBarVisibilityChangedEvent += OnTitleBarVisibilityChanged;
        }

        NavigationHelper.Navigate(typeof(MusicPage));
        ChangeSelectedItemOfNavigationView();
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
            NavigationHelper.Navigate(typeof(SettingsPage));
        }
        else
        {
            if (str == "MusicPage" && ContentFrame.CurrentSourcePageType != typeof(MusicPage))
            {
                NavigationHelper.Navigate(typeof(MusicPage));
            }
            else if (str == "NowPlayingPage" && ContentFrame.CurrentSourcePageType != typeof(NowPlayingPage))
            {
                NavigationHelper.Navigate(typeof(NowPlayingPage));
            }
            else if (str == "NewsPage" && ContentFrame.CurrentSourcePageType != typeof(NewsPage))
            {
                NavigationHelper.Navigate(typeof(NewsPage));
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
}
