using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class SongMenuFlyout : ItemMenuFlyoutBase
{
    public SongMenuFlyout()
    {
        this.InitializeComponent();
    }

    public Visibility RemoveFromPlaylistVisibility
    {
        get => (Visibility)GetValue(RemoveFromPlaylistVisibilityProperty);
        set => SetValue(RemoveFromPlaylistVisibilityProperty, value);
    }

    public static readonly DependencyProperty RemoveFromPlaylistVisibilityProperty =
        DependencyProperty.Register(nameof(RemoveFromPlaylistVisibility), typeof(Visibility), typeof(SongMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public Playlist OptionalPlaylist
    {
        get => (Playlist)GetValue(OptionalPlaylistProperty);
        set => SetValue(OptionalPlaylistProperty, value);
    }

    public static readonly DependencyProperty OptionalPlaylistProperty =
        DependencyProperty.Register(nameof(OptionalPlaylist), typeof(Playlist), typeof(SongMenuFlyout), new PropertyMetadata(null, OnOptionalPlaylistPropertyChanged));

    public ValueTuple<Playlist, object> RemoveFromPlaylistParameter
    {
        get => (ValueTuple<Playlist, object>)GetValue(RemoveFromPlaylistParameterProperty);
        set => SetValue(RemoveFromPlaylistParameterProperty, value);
    }

    public static readonly DependencyProperty RemoveFromPlaylistParameterProperty =
        DependencyProperty.Register(nameof(RemoveFromPlaylistParameter), typeof(ValueTuple<Playlist, object>), typeof(SongMenuFlyout), new PropertyMetadata(default(ValueTuple<Playlist, object>)));

    private static void OnOptionalPlaylistPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (Playlist, object) raw = (ValueTuple<Playlist, object>)d.GetValue(RemoveFromPlaylistParameterProperty);
        (Playlist, object) newValue = ((Playlist)e.NewValue, raw.Item2);
        d.SetValue(RemoveFromPlaylistParameterProperty, newValue);
    }

    protected override void OnSourceDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (Playlist, object) raw = (ValueTuple<Playlist, object>)d.GetValue(RemoveFromPlaylistParameterProperty);
        (Playlist, object) newValue = (raw.Item1, e.NewValue);
        d.SetValue(RemoveFromPlaylistParameterProperty, newValue);
    }

    private void OnSongMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData, OptionalPlaylist);
    }
}
