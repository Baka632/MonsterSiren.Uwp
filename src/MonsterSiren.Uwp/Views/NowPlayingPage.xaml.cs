using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 正在播放页
/// </summary>
public sealed partial class NowPlayingPage : Page
{
    public NowPlayingPage()
    {
        this.InitializeComponent();
    }

    private void OnNowPlayingPageLoaded(object sender, RoutedEventArgs e)
    {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
    }
}
