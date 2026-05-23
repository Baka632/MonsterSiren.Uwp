using System.Windows.Input;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class SongMenuFlyout : WithAddToMenuFlyout
{
    public SongMenuFlyout()
    {
        this.InitializeComponent();
    }

    public object SourceData
    {
        get => GetValue(SourceDataProperty);
        set => SetValue(SourceDataProperty, value);
    }

    public static readonly DependencyProperty SourceDataProperty =
        DependencyProperty.Register(nameof(SourceData), typeof(object), typeof(SongMenuFlyout), new PropertyMetadata(null, OnSourceDataPropertyChanged));

    public ICommand AddItemToFavoriteCallbackCommand
    {
        get => (ICommand)GetValue(AddItemToFavoriteCallbackCommandProperty);
        set => SetValue(AddItemToFavoriteCallbackCommandProperty, value);
    }

    public static readonly DependencyProperty AddItemToFavoriteCallbackCommandProperty =
        DependencyProperty.Register(nameof(AddItemToFavoriteCallbackCommand), typeof(ICommand), typeof(SongMenuFlyout), new PropertyMetadata(null));

    public Visibility FavoriteVisibility
    {
        get => (Visibility)GetValue(FavoriteVisibilityProperty);
        set => SetValue(FavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty FavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(FavoriteVisibility), typeof(Visibility), typeof(SongMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public Visibility UnfavoriteVisibility
    {
        get => (Visibility)GetValue(UnfavoriteVisibilityProperty);
        set => SetValue(UnfavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty UnfavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(UnfavoriteVisibility), typeof(Visibility), typeof(SongMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public Visibility RemoveFromPlaylistVisibility
    {
        get => (Visibility)GetValue(RemoveFromPlaylistVisibilityProperty);
        set => SetValue(RemoveFromPlaylistVisibilityProperty, value);
    }

    public static readonly DependencyProperty RemoveFromPlaylistVisibilityProperty =
        DependencyProperty.Register(nameof(RemoveFromPlaylistVisibility), typeof(Visibility), typeof(SongMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public ICommand StartMultipleSelectionCommand
    {
        get => (ICommand)GetValue(StartMultipleSelectionCommandProperty);
        set => SetValue(StartMultipleSelectionCommandProperty, value);
    }

    public static readonly DependencyProperty StartMultipleSelectionCommandProperty =
        DependencyProperty.Register(nameof(StartMultipleSelectionCommand), typeof(ICommand), typeof(SongMenuFlyout), new PropertyMetadata(null));

    public Visibility MultipleSelectionVisibility
    {
        get => (Visibility)GetValue(MultipleSelectionVisibilityProperty);
        set => SetValue(MultipleSelectionVisibilityProperty, value);
    }

    public static readonly DependencyProperty MultipleSelectionVisibilityProperty =
        DependencyProperty.Register(nameof(MultipleSelectionVisibility), typeof(Visibility), typeof(SongMenuFlyout), new PropertyMetadata(Visibility.Visible));

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

    private static void OnSourceDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
