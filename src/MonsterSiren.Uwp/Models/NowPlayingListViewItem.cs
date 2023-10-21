using Windows.Media.Playback;
using Windows.UI;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 自定义的正在播放列表项目
/// </summary>
public sealed class NowPlayingListViewItem : ListViewItem
{
    public SolidColorBrush ContentBrush
    {
        get => (SolidColorBrush)GetValue(ContentBrushProperty);
        set => SetValue(ContentBrushProperty, value);
    }

    public static readonly DependencyProperty ContentBrushProperty
        = DependencyProperty.Register("ContentBrush",
                                      typeof(SolidColorBrush),
                                      typeof(NowPlayingListViewItem),
                                      new PropertyMetadata(new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"])));

    public Visibility CurrentNowPlayingItemIndicatorVisibility
    {
        get => (Visibility)GetValue(CurrentNowPlayingItemIndicatorVisibilityProperty);
        set => SetValue(CurrentNowPlayingItemIndicatorVisibilityProperty, value);
    }

    public static readonly DependencyProperty CurrentNowPlayingItemIndicatorVisibilityProperty =
        DependencyProperty.Register("CurrentNowPlayingItemIndicatorVisibility", typeof(Visibility), typeof(NowPlayingListViewItem), new PropertyMetadata(Visibility.Collapsed));

    public NowPlayingListViewItem() : base()
    {
        MusicService.PlayerPlayItemChanged += OnPlayerPlayItemChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (MusicService.CurrentMediaPlaybackItem is not null && MusicService.CurrentMediaPlaybackItem == DataContext)
        {
            ContentBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorLight2"]);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Visible;
        }
        else
        {
            ContentBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Collapsed;
        }
    }

    ~NowPlayingListViewItem()
    {
        MusicService.PlayerPlayItemChanged -= OnPlayerPlayItemChanged;
    }

    private void OnPlayerPlayItemChanged(CurrentMediaPlaybackItemChangedEventArgs args)
    {
        if (args.NewItem is not null && args.NewItem == DataContext)
        {
            ContentBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorLight2"]);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Visible;
        }
        else
        {
            ContentBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Collapsed;
        }
    }
}
