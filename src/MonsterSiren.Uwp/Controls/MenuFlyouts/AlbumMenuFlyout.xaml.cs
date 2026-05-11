using System.Windows.Input;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class AlbumMenuFlyout : WithAddToMenuFlyout
{
    public AlbumMenuFlyout()
    {
        this.InitializeComponent();
    }

    public object SourceData
    {
        get => GetValue(SourceDataProperty);
        set => SetValue(SourceDataProperty, value);
    }

    public static readonly DependencyProperty SourceDataProperty =
        DependencyProperty.Register(nameof(SourceData), typeof(object), typeof(AlbumMenuFlyout), new PropertyMetadata(null));

    public ICommand AddItemToFavoriteCallbackCommand
    {
        get => (ICommand)GetValue(AddItemToFavoriteCallbackCommandProperty);
        set => SetValue(AddItemToFavoriteCallbackCommandProperty, value);
    }

    public static readonly DependencyProperty AddItemToFavoriteCallbackCommandProperty =
        DependencyProperty.Register(nameof(AddItemToFavoriteCallbackCommand), typeof(ICommand), typeof(AlbumMenuFlyout), new PropertyMetadata(null));

    public Visibility FavoriteVisibility
    {
        get => (Visibility)GetValue(FavoriteVisibilityProperty);
        set => SetValue(FavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty FavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(FavoriteVisibility), typeof(Visibility), typeof(AlbumMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public Visibility UnfavoriteVisibility
    {
        get => (Visibility)GetValue(UnfavoriteVisibilityProperty);
        set => SetValue(UnfavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty UnfavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(UnfavoriteVisibility), typeof(Visibility), typeof(AlbumMenuFlyout), new PropertyMetadata(Visibility.Collapsed));

    public ICommand StartMultipleSelectionCommand
    {
        get => (ICommand)GetValue(StartMultipleSelectionCommandProperty);
        set => SetValue(StartMultipleSelectionCommandProperty, value);
    }

    public static readonly DependencyProperty StartMultipleSelectionCommandProperty =
        DependencyProperty.Register(nameof(StartMultipleSelectionCommand), typeof(ICommand), typeof(AlbumMenuFlyout), new PropertyMetadata(null));

    public Visibility MultipleSelectionVisibility
    {
        get => (Visibility)GetValue(MultipleSelectionVisibilityProperty);
        set => SetValue(MultipleSelectionVisibilityProperty, value);
    }

    public static readonly DependencyProperty MultipleSelectionVisibilityProperty =
        DependencyProperty.Register(nameof(MultipleSelectionVisibility), typeof(Visibility), typeof(AlbumMenuFlyout), new PropertyMetadata(Visibility.Visible));

    private void OnAlbumMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData);
    }
}
