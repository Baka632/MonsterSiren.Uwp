using Windows.UI;

namespace MonsterSiren.Uwp.Controls;

public sealed class ThemedToggleButton : ToggleButton
{
    public SolidColorBrush ThemeBackground
    {
        get => (SolidColorBrush)GetValue(ThemeBackgroundProperty);
        set => SetValue(ThemeBackgroundProperty, value);
    }

    public static readonly DependencyProperty ThemeBackgroundProperty =
        DependencyProperty.Register("ThemeBackground", typeof(SolidColorBrush), typeof(ThemedToggleButton), new PropertyMetadata(new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorLight1"])));
}
