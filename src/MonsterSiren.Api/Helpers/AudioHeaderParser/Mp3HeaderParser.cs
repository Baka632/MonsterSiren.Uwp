using System.Buffers.Binary;

namespace MonsterSiren.Api.Helpers.AudioHeaderParser;

internal static class Mp3HeaderParser
{
    public static bool IsMp3Header(Span<byte> bytes)
    {
        throw new NotImplementedException();
    }

    public static TimeSpan GetMp3Duration(Span<byte> bytes)
    {
        throw new NotImplementedException();
    }
}
