using Windows.Media.Core;

namespace MonsterSiren.Uwp.Helpers;

internal static class CodecQueryHelper
{
    private static IEnumerable<CodecInfo> _cachedCommonEncoders;

    public static bool TryGetCommonEncoders(out IEnumerable<CodecInfo> codecInfos)
    {
        if (_cachedCommonEncoders is not null)
        {
            codecInfos = _cachedCommonEncoders;
            return true;
        }

        CodecQuery codecQuery = new();
        IEnumerable<CodecInfo> commonEncoders = from info
                                                in codecQuery.FindAllAsync(CodecKind.Audio, CodecCategory.Encoder, string.Empty).AsTask().Result
                                                where HasCommonEncoders(info)
                                                select info;
        _cachedCommonEncoders = codecInfos = commonEncoders;
        return commonEncoders.Any();
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
