using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class SongListMenuFlyout : ListMenuFlyoutBase
{
    public SongListMenuFlyout()
    {
        this.InitializeComponent();
    }

    public Playlist OptionalPlaylist
    {
        get => (Playlist)GetValue(OptionalPlaylistProperty);
        set => SetValue(OptionalPlaylistProperty, value);
    }

    public static readonly DependencyProperty OptionalPlaylistProperty =
        DependencyProperty.Register(nameof(OptionalPlaylist), typeof(Playlist), typeof(SongListMenuFlyout), new PropertyMetadata(null, OnOptionalPlaylistPropertyChanged));

    public Visibility RemoveFromPlaylistVisibility
    {
        get => (Visibility)GetValue(RemoveFromPlaylistVisibilityProperty);
        set => SetValue(RemoveFromPlaylistVisibilityProperty, value);
    }

    public static readonly DependencyProperty RemoveFromPlaylistVisibilityProperty =
        DependencyProperty.Register(nameof(RemoveFromPlaylistVisibility), typeof(Visibility), typeof(SongMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public ValueTuple<Playlist, object> RemoveFromPlaylistParameter
    {
        get => (ValueTuple<Playlist, object>)GetValue(RemoveFromPlaylistParameterProperty);
        set => SetValue(RemoveFromPlaylistParameterProperty, value);
    }

    public static readonly DependencyProperty RemoveFromPlaylistParameterProperty =
        DependencyProperty.Register(nameof(RemoveFromPlaylistParameter), typeof(ValueTuple<Playlist, object>), typeof(SongMenuFlyout), new PropertyMetadata(default(ValueTuple<Playlist, object>)));

    private void OnSongListMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData, OptionalPlaylist, addToNowPlayingCommandCallback: MultipleOperationEndCallbackCommand, playlistCommandCallback: MultipleOperationEndCallbackCommand);
    }

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
}
