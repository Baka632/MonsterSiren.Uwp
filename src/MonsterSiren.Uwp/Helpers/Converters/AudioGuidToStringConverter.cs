
using Windows.Media.Core;

namespace MonsterSiren.Uwp.Helpers.Converters;

/// <summary>
/// 将表示音频格式的 GUID（或其集合）转换为字符串的转换器
/// </summary>
public sealed class AudioGuidToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is IEnumerable<string> guids)
        {
            if (guids.Any(IsTargetCodec(CodecSubtypes.AudioFormatMP3)))
            {
                return "Mp3Format".GetLocalized();
            }
            else if (guids.Any(IsTargetCodec(CodecSubtypes.AudioFormatFlac)))
            {
                return "FlacFormat".GetLocalized();
            }
        }
        else if (value is string guid)
        {
            if (guid == CodecSubtypes.AudioFormatMP3)
            {
                return "Mp3Format".GetLocalized();
            }
            else if (guid == CodecSubtypes.AudioFormatFlac)
            {
                return "FlacFormat".GetLocalized();
            }
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

    private static Func<string, bool> IsTargetCodec(string target)
    {
        return guid => guid == target;
    }
}
