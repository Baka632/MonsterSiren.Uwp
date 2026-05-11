using System.Windows.Input;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class AlbumListMenuFlyout : WithAddToMenuFlyout
{
    private ICommand MultipleOperationEndCallbackCommand { get; }

    public AlbumListMenuFlyout()
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
        DependencyProperty.Register(nameof(SelectAllListCommand), typeof(ICommand), typeof(AlbumListMenuFlyout), new PropertyMetadata(null));

    public ICommand DeselectAllListCommand
    {
        get => (ICommand)GetValue(DeselectAllListCommandProperty);
        set => SetValue(DeselectAllListCommandProperty, value);
    }

    public static readonly DependencyProperty DeselectAllListCommandProperty =
        DependencyProperty.Register(nameof(DeselectAllListCommand), typeof(ICommand), typeof(AlbumListMenuFlyout), new PropertyMetadata(null));

    public ICommand StopMultipleSelectionCommand
    {
        get => (ICommand)GetValue(StopMultipleSelectionCommandProperty);
        set => SetValue(StopMultipleSelectionCommandProperty, value);
    }

    public static readonly DependencyProperty StopMultipleSelectionCommandProperty =
        DependencyProperty.Register(nameof(StopMultipleSelectionCommand), typeof(ICommand), typeof(AlbumListMenuFlyout), new PropertyMetadata(null));

    public Visibility FavoriteVisibility
    {
        get => (Visibility)GetValue(FavoriteVisibilityProperty);
        set => SetValue(FavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty FavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(FavoriteVisibility), typeof(Visibility), typeof(AlbumListMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public Visibility UnfavoriteVisibility
    {
        get => (Visibility)GetValue(UnfavoriteVisibilityProperty);
        set => SetValue(UnfavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty UnfavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(UnfavoriteVisibility), typeof(Visibility), typeof(AlbumListMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public object SourceData
    {
        get => GetValue(SourceDataProperty);
        set => SetValue(SourceDataProperty, value);
    }

    public static readonly DependencyProperty SourceDataProperty =
        DependencyProperty.Register(nameof(SourceData), typeof(object), typeof(AlbumListMenuFlyout), new PropertyMetadata(null));

    private void OnAlbumListMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData, addToNowPlayingCommandCallback: MultipleOperationEndCallbackCommand, playlistCommandCallback: MultipleOperationEndCallbackCommand);
    }

    private void MultipleOperationEndCallback(bool result)
    {
        if (result)
        {
            StopMultipleSelectionCommand?.Execute(null);
        }
    }
}
