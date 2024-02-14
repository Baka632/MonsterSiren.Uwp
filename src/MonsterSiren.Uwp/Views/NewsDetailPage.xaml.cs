// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using Windows.System;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class NewsDetailPage : Page
{
    public NewsDetailViewModel ViewModel { get; } = new NewsDetailViewModel();

    public NewsDetailPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is NewsDetail newsDetail)
        {
            ViewModel.CurrentNewsDetail = newsDetail;
            ContentWebView.Navigate(new Uri("ms-appx-web:///Assets/Web/Html/NewsDetailPageTemplate.html"));
        }
    }

    private async void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
    {
        SolidColorBrush colorBrush = (SolidColorBrush)Resources["SystemControlForegroundBaseHighBrush"];
        Windows.UI.Color textColor = colorBrush.Color;

        Windows.UI.Color accentColor = (Windows.UI.Color)Resources["SystemAccentColor"];
        await ViewModel.InitializeWebView(sender, textColor, accentColor);
    }

    private async void OnContentWebViewUnviewableContentIdentified(WebView sender, WebViewUnviewableContentIdentifiedEventArgs args)
    {
        await Launcher.LaunchUriAsync(args.Referrer, ViewModel.DefaultLauncherOptionsForExternal);
    }
}
