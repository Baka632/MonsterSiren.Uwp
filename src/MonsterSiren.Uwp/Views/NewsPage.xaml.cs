// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板


namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 新闻动向页
/// </summary>
public sealed partial class NewsPage : Page
{
    private DispatcherTimer timer;
    private bool isPointerEnteredRecommendedNewsStackPanel;

    public NewsViewModel ViewModel { get; } = new NewsViewModel();

    public NewsPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.Initialize();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        timer ??= new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(5);
        timer.Tick += OnDispatcherTimerTick;
        timer.Start();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        if (timer is not null)
        {
            timer.Tick -= OnDispatcherTimerTick;
            timer.Stop();
        }
    }

    private void OnDispatcherTimerTick(object sender, object e)
    {
        if (isPointerEnteredRecommendedNewsStackPanel != true
            && RecommendedNewsFlipView.SelectedIndex != -1
            && RecommendedNewsFlipView.ItemsSource is IList<RecommendedNewsInfo> newsList
            && newsList.Count > 0)
        {
            int nextItemIndex = RecommendedNewsFlipView.SelectedIndex + 1;
            if (nextItemIndex + 1 <= newsList.Count)
            {
                RecommendedNewsFlipView.SelectedIndex = nextItemIndex;
            }
            else
            {
                RecommendedNewsFlipView.SelectedIndex = 0;
            }
        }
    }

    private void OnRecommendedNewsContainerScrollViewerPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        isPointerEnteredRecommendedNewsStackPanel = true;
    }

    private void OnRecommendedNewsContainerScrollViewerPointerExited(object sender, PointerRoutedEventArgs e)
    {
        isPointerEnteredRecommendedNewsStackPanel = false;
    }

    private void OnRefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
    {

    }
}
