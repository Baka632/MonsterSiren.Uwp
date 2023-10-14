// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using Windows.UI.Core;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 画中画模式的正在播放页
/// </summary>
public sealed partial class NowPlayingCompactPage : Page
{
    private bool IsContentHide;
    private bool IsActive = false;
    private readonly DispatcherTimer _timer = new()
    {
        Interval = TimeSpan.FromSeconds(5d)
    };

    public NowPlayingCompactViewModel ViewModel { get; } = new();

    public NowPlayingCompactPage()
    {
        this.InitializeComponent();
        _timer.Tick += OnTimerTick;
        _timer.Start();

        SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
        navigationManager.BackRequested -= MainPage.BackRequested;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _timer.Stop();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (_timer.IsEnabled != true)
        {
            _timer.Start();
        }
    }

    private void OnTimerTick(object sender, object e)
    {
        if (IsActive != true && ViewModel.MusicInfo.CurrentMusicPropertiesExists)
        {
            HideContentStoryBoard.Begin();
            IsContentHide = true;
        }

        IsActive = false;
    }

    private void OnPagePointerMoved(object sender, PointerRoutedEventArgs e)
    {
        SetActiveAndTryShowContent();
    }

    private void OnPageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        SetActiveAndTryShowContent();
    }

    private void SetActiveAndTryShowContent()
    {
        IsActive = true;

        if (IsContentHide)
        {
            ShowContentStoryBoard.Begin();
            IsContentHide = false;
        }
    }
}
