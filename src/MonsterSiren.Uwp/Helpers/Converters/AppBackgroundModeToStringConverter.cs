
namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class AppBackgroundModeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AppBackgroundMode mode)
        {
            return mode switch
            {
                AppBackgroundMode.Mica => "MicaBackground".GetLocalized(),
                AppBackgroundMode.Acrylic => "AcrylicBackground".GetLocalized(),
                AppBackgroundMode.PureColor => "PureColorBackground".GetLocalized(),
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
