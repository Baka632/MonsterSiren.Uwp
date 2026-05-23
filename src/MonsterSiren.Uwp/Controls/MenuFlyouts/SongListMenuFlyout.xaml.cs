using System.Windows.Input;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class SongListMenuFlyout : WithAddToMenuFlyout
{
    private ICommand MultipleOperationEndCallbackCommand { get; }

    public SongListMenuFlyout()
    {
        this.InitializeComponent();
        MultipleOperationEndCallbackCommand = new RelayCommand<bool>(MultipleOperationEndCallback);
    }

    public ICommand SelectAllListCommand
    {
        get => (ICommand)GetValue(SelectAllListCommandProperty);
        set => SetValue(SelectAllListCommandProperty, value);
    }

    public static readonly DependencyProperty SelectAllListCommandProperty =
        DependencyProperty.Register(nameof(SelectAllListCommand), typeof(ICommand), typeof(SongListMenuFlyout), new PropertyMetadata(null));

    public ICommand DeselectAllListCommand
    {
        get => (ICommand)GetValue(DeselectAllListCommandProperty);
        set => SetValue(DeselectAllListCommandProperty, value);
    }

    public static readonly DependencyProperty DeselectAllListCommandProperty =
        DependencyProperty.Register(nameof(DeselectAllListCommand), typeof(ICommand), typeof(SongListMenuFlyout), new PropertyMetadata(null));

    public ICommand StopMultipleSelectionCommand
    {
        get => (ICommand)GetValue(StopMultipleSelectionCommandProperty);
        set => SetValue(StopMultipleSelectionCommandProperty, value);
    }

    public static readonly DependencyProperty StopMultipleSelectionCommandProperty =
        DependencyProperty.Register(nameof(StopMultipleSelectionCommand), typeof(ICommand), typeof(SongListMenuFlyout), new PropertyMetadata(null));

    public Visibility FavoriteVisibility
    {
        get => (Visibility)GetValue(FavoriteVisibilityProperty);
        set => SetValue(FavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty FavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(FavoriteVisibility), typeof(Visibility), typeof(SongListMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public Visibility UnfavoriteVisibility
    {
        get => (Visibility)GetValue(UnfavoriteVisibilityProperty);
        set => SetValue(UnfavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty UnfavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(UnfavoriteVisibility), typeof(Visibility), typeof(SongListMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public object SourceData
    {
        get => GetValue(SourceDataProperty);
        set => SetValue(SourceDataProperty, value);
    }

    public static readonly DependencyProperty SourceDataProperty =
        DependencyProperty.Register(nameof(SourceData), typeof(object), typeof(SongListMenuFlyout), new PropertyMetadata(null, OnSourceDataPropertyChanged));

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

    private void MultipleOperationEndCallback(bool result)
    {
        if (result)
        {
            StopMultipleSelectionCommand?.Execute(null);
        }
    }

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
}
