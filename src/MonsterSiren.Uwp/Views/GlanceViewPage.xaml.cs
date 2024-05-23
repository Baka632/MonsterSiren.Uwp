// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using Windows.UI.ViewManagement;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class GlanceViewPage : Page
{
    private readonly DispatcherTimer _timer = new();
    private readonly Random _random = new();

    public GlanceViewViewModel ViewModel { get; } = new GlanceViewViewModel();

    public GlanceViewPage()
    {
        this.InitializeComponent();
        _timer.Interval = TimeSpan.FromSeconds(60d);
        _timer.Tick += OnTimerTick;
        _timer.Start();
        SizeChanged += OnPageSizeChanged;
    }

    private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.ContentOffset = ContentCanvas.ActualHeight - ContentStackPanel.ActualHeight;
    }

    private void OnTimerTick(object sender, object e)
    {
        AdjustContentPosition();
    }

    private void AdjustContentPosition()
    {
        double height = ContentCanvas.ActualHeight - ContentStackPanel.ActualHeight;

        if (ViewModel.ContentOffset == height)
        {
            ViewModel.ContentOffset -= height / 5d;
        }
        else if (ViewModel.ContentOffset > height - (height * 0.85) && ViewModel.ContentOffset < height)
        {
            double randomValue = _random.NextDouble();

            if (randomValue > 0.5)
            {
                ViewModel.ContentOffset -= _random.NextDouble() * (height / 5d);
            }
            else
            {
                double delta = _random.NextDouble() * (height / 5d);

                if (delta > height)
                {
                    ViewModel.ContentOffset = height;
                }
                else
                {
                    ViewModel.ContentOffset += delta;
                }
            }
        }
        else
        {
            ViewModel.ContentOffset = height;
        }
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

    private void OnContentLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ContentOffset = ContentCanvas.ActualHeight - ContentStackPanel.ActualHeight;
    }
}
