using Windows.Media.Core;

namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class CodecCategoryToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is CodecCategory category)
        {
            return category switch
            {
                CodecCategory.Encoder => "CodecInfo_Encoder".GetLocalized(),
                CodecCategory.Decoder => "CodecInfo_Decoder".GetLocalized(),
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
