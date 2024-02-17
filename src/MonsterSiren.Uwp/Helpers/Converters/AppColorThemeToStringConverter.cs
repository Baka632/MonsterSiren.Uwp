namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class AppColorThemeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AppColorTheme colorTheme)
        {
            return colorTheme switch
            {
                AppColorTheme.Default => "ColorTheme_Default".GetLocalized(),
                AppColorTheme.Light => "ColorTheme_Light".GetLocalized(),
                AppColorTheme.Dark => "ColorTheme_Dark".GetLocalized(),
                _ => throw new NotImplementedException()
            };
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
