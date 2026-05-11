using System.Windows.Input;

namespace MonsterSiren.Uwp.Controls;

public sealed class SongCidProviderXamlHelper : DependencyObject
{
    public static object GetSourceData(DependencyObject obj) => obj.GetValue(SourceDataProperty);

    public static void SetSourceData(DependencyObject obj, object value) => obj.SetValue(SourceDataProperty, value);

    public static readonly DependencyProperty SourceDataProperty =
        DependencyProperty.RegisterAttached("SourceData", typeof(object), typeof(SongCidProviderXamlHelper), new PropertyMetadata(null, OnSourceDataChanged));

    public static ICommand GetCallbackCommand(DependencyObject obj) => (ICommand)obj.GetValue(CallbackCommandProperty);

    public static void SetCallbackCommand(DependencyObject obj, ICommand value) => obj.SetValue(CallbackCommandProperty, value);

    public static readonly DependencyProperty CallbackCommandProperty =
        DependencyProperty.RegisterAttached("CallbackCommand", typeof(ICommand), typeof(SongCidProviderXamlHelper), new PropertyMetadata(null, OnCallbackCommandChanged));

    private static void OnCallbackCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != e.NewValue)
        {
            ICommand command = GetCallbackCommand(d);

            MenuFlyoutItem flyoutItem = (MenuFlyoutItem)d;
            flyoutItem.CommandParameter = flyoutItem.CommandParameter is CommandParameter rawParameter
                ? rawParameter with { Callback = command }
                : new CommandParameter(null, command);
        }
    }

    private static void OnSourceDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != e.NewValue)
        {
            object data = GetSourceData(d);

            MenuFlyoutItem flyoutItem = (MenuFlyoutItem)d;
            flyoutItem.CommandParameter = flyoutItem.CommandParameter is CommandParameter rawParameter
                ? rawParameter with { Parameter = data }
                : new CommandParameter(data, null);
        }
    }
}
