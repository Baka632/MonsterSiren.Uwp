using Windows.Media.Core;

namespace MonsterSiren.Uwp.Helpers;

internal static class CodecQueryHelper
{
    private static IEnumerable<CodecInfo> _cachedCommonEncoders;

    public static async Task<ValueTuple<bool, IEnumerable<CodecInfo>>> TryGetCommonEncoders()
    {
        try
        {
            if (_cachedCommonEncoders is not null)
            {
                return (true, _cachedCommonEncoders);
            }

            CodecQuery codecQuery = new();
            IEnumerable<CodecInfo> commonEncoders = from info
                                                    in await codecQuery.FindAllAsync(CodecKind.Audio, CodecCategory.Encoder, string.Empty)
                                                    where HasCommonEncoders(info)
                                                    select info;
            _cachedCommonEncoders = commonEncoders;
            return (commonEncoders.Any(), commonEncoders);
        }
        catch
        {
            return (false, null);
        }
    }

    public static bool IsCodecInfoHasTargetEncoder(CodecInfo info, string targetEncoderGuid)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        if (string.IsNullOrWhiteSpace(targetEncoderGuid))
        {
            throw new ArgumentException($"“{nameof(targetEncoderGuid)}”不能为 null 或空白。", nameof(targetEncoderGuid));
        }

        return info.Subtypes.Any(guid => guid == targetEncoderGuid);
    }

    private static bool HasCommonEncoders(CodecInfo info)
    {
        return info.Subtypes.Any(IsCommonEncoder);
    }

    private static bool IsCommonEncoder(string encoderGuid)
    {
        return encoderGuid == CodecSubtypes.AudioFormatFlac || encoderGuid == CodecSubtypes.AudioFormatMP3;
    }
}
