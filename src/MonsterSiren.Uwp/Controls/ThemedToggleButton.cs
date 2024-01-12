using Windows.UI;

namespace MonsterSiren.Uwp.Controls;

public sealed class ThemedToggleButton : ToggleButton
{
    public Brush ThemeBackground
    {
        get => (Brush)GetValue(ThemeBackgroundProperty);
        set => SetValue(ThemeBackgroundProperty, value);
    }

    public static readonly DependencyProperty ThemeBackgroundProperty =
        DependencyProperty.Register("ThemeBackground", typeof(Brush), typeof(ThemedToggleButton), new PropertyMetadata(new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorLight1"])));
}
