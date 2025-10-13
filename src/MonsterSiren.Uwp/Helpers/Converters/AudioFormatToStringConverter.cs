
namespace MonsterSiren.Uwp.Helpers.Converters;

internal class AudioFormatToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AudioFormat format)
        {
            return format switch
            {
                AudioFormat.Mp3 => "Mp3Format".GetLocalized(),
                AudioFormat.Flac => "FlacFormat".GetLocalized(),
                _ => DependencyProperty.UnsetValue,
            };
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
