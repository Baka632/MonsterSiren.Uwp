// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板


using Windows.UI.ViewManagement;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class GlanceViewPage : Page
{
    public GlanceViewViewModel ViewModel { get; } = new GlanceViewViewModel();

    public GlanceViewPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ApplicationView view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode != true)
        {
            view.TryEnterFullScreenMode();
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        ApplicationView view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
        {
            view.ExitFullScreenMode();
        }
    }

    private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        Frame.GoBack();
    }
}
