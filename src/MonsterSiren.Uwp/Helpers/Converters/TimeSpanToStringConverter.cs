namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            TimeSpan timeSpan => timeSpan.ToString(@"m\:ss"),
            null => TimeSpan.Zero.ToString(@"m\:ss"),
            _ => DependencyProperty.UnsetValue
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is string str && TimeSpan.TryParse(str, out TimeSpan result))
        {
            return result;
        }
        else
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
