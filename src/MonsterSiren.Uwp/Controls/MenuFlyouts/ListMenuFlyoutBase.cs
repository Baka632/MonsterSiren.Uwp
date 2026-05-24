using System.Windows.Input;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public abstract class ListMenuFlyoutBase : AddToMenuFlyoutBase
{
    protected ICommand MultipleOperationEndCallbackCommand { get; }

    public ListMenuFlyoutBase()
    {
        MultipleOperationEndCallbackCommand = new RelayCommand<bool>(MultipleOperationEndCallback);
    }

    public ICommand SelectAllListCommand
    {
        get => (ICommand)GetValue(SelectAllListCommandProperty);
        set => SetValue(SelectAllListCommandProperty, value);
    }

    public static readonly DependencyProperty SelectAllListCommandProperty =
        DependencyProperty.Register(nameof(SelectAllListCommand), typeof(ICommand), typeof(ListMenuFlyoutBase), new PropertyMetadata(null));

    public ICommand DeselectAllListCommand
    {
        get => (ICommand)GetValue(DeselectAllListCommandProperty);
        set => SetValue(DeselectAllListCommandProperty, value);
    }

    public static readonly DependencyProperty DeselectAllListCommandProperty =
        DependencyProperty.Register(nameof(DeselectAllListCommand), typeof(ICommand), typeof(ListMenuFlyoutBase), new PropertyMetadata(null));

    public ICommand StopMultipleSelectionCommand
    {
        get => (ICommand)GetValue(StopMultipleSelectionCommandProperty);
        set => SetValue(StopMultipleSelectionCommandProperty, value);
    }

    public static readonly DependencyProperty StopMultipleSelectionCommandProperty =
        DependencyProperty.Register(nameof(StopMultipleSelectionCommand), typeof(ICommand), typeof(ListMenuFlyoutBase), new PropertyMetadata(null));

    private void MultipleOperationEndCallback(bool result)
    {
        if (result)
        {
            StopMultipleSelectionCommand?.Execute(null);
        }
    }
}
