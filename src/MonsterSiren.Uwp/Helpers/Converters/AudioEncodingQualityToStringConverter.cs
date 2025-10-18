
using Windows.Media.MediaProperties;

namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class AudioEncodingQualityToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AudioEncodingQuality quality)
        {
            return quality switch
            {
                AudioEncodingQuality.High => "AudioQualityHigh".GetLocalized(),
                AudioEncodingQuality.Medium => "AudioQualityMedium".GetLocalized(),
                AudioEncodingQuality.Low => "AudioQualityLow".GetLocalized(),
                _ => DependencyProperty.UnsetValue
            };
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
