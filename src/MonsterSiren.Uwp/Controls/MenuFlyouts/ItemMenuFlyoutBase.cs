using System.Windows.Input;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public abstract class ItemMenuFlyoutBase : AddToMenuFlyoutBase
{
    public ICommand StartMultipleSelectionCommand
    {
        get => (ICommand)GetValue(StartMultipleSelectionCommandProperty);
        set => SetValue(StartMultipleSelectionCommandProperty, value);
    }

    public static readonly DependencyProperty StartMultipleSelectionCommandProperty =
        DependencyProperty.Register(nameof(StartMultipleSelectionCommand), typeof(ICommand), typeof(ItemMenuFlyoutBase), new PropertyMetadata(null));

    public Visibility MultipleSelectionVisibility
    {
        get => (Visibility)GetValue(MultipleSelectionVisibilityProperty);
        set => SetValue(MultipleSelectionVisibilityProperty, value);
    }

    public static readonly DependencyProperty MultipleSelectionVisibilityProperty =
        DependencyProperty.Register(nameof(MultipleSelectionVisibility), typeof(Visibility), typeof(ItemMenuFlyoutBase), new PropertyMetadata(Visibility.Visible));

    public ICommand AddItemToFavoriteCallbackCommand
    {
        get => (ICommand)GetValue(AddItemToFavoriteCallbackCommandProperty);
        set => SetValue(AddItemToFavoriteCallbackCommandProperty, value);
    }

    public static readonly DependencyProperty AddItemToFavoriteCallbackCommandProperty =
        DependencyProperty.Register(nameof(AddItemToFavoriteCallbackCommand), typeof(ICommand), typeof(ItemMenuFlyoutBase), new PropertyMetadata(null));
}
