using System.ComponentModel;
using Windows.Media.Playback;
using Windows.UI;

namespace MonsterSiren.Uwp.Controls;

public sealed class NowPlayingItemGrid : Grid
{
    private static Color CurrentThemeColor { get => MusicInfoService.Default.MusicThemeColorLight2; }

    public Brush ContentBrush
    {
        get => (Brush)GetValue(ContentBrushProperty);
        set => SetValue(ContentBrushProperty, value);
    }

    public static readonly DependencyProperty ContentBrushProperty
        = DependencyProperty.Register("ContentBrush",
                                      typeof(Brush),
                                      typeof(NowPlayingItemGrid),
                                      new PropertyMetadata(new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"])));

    public Visibility CurrentNowPlayingItemIndicatorVisibility
    {
        get => (Visibility)GetValue(CurrentNowPlayingItemIndicatorVisibilityProperty);
        set => SetValue(CurrentNowPlayingItemIndicatorVisibilityProperty, value);
    }

    public static readonly DependencyProperty CurrentNowPlayingItemIndicatorVisibilityProperty =
        DependencyProperty.Register("CurrentNowPlayingItemIndicatorVisibility", typeof(Visibility), typeof(NowPlayingItemGrid), new PropertyMetadata(Visibility.Collapsed));

    public NowPlayingItemGrid() : base()
    {
        MusicService.PlayerPlayItemChanged += OnPlayerPlayItemChanged;
        Loaded += OnLoaded;
        MusicInfoService.Default.PropertyChanged += OnMusicInfoServicePropertyChanged;
    }

    private void OnMusicInfoServicePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MusicInfoService.MusicThemeColorLight2) && MusicService.CurrentMediaPlaybackItem is not null && MusicService.CurrentMediaPlaybackItem == DataContext)
        {
            ContentBrush = new SolidColorBrush(CurrentThemeColor);
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (MusicService.CurrentMediaPlaybackItem is not null && MusicService.CurrentMediaPlaybackItem == DataContext)
        {
            ContentBrush = new SolidColorBrush(CurrentThemeColor);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Visible;
        }
        else
        {
            ContentBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Collapsed;
        }
    }

    private void OnPlayerPlayItemChanged(CurrentMediaPlaybackItemChangedEventArgs args)
    {
        if (args.NewItem is not null && args.NewItem == DataContext)
        {
            ContentBrush = new SolidColorBrush(CurrentThemeColor);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Visible;
        }
        else
        {
            ContentBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
            CurrentNowPlayingItemIndicatorVisibility = Visibility.Collapsed;
        }
    }

    ~NowPlayingItemGrid()
    {
        MusicService.PlayerPlayItemChanged -= OnPlayerPlayItemChanged;
        MusicInfoService.Default.PropertyChanged -= OnMusicInfoServicePropertyChanged;
    }
}
