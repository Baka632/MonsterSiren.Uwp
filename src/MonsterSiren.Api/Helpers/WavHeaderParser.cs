using System.Buffers.Binary;

namespace MonsterSiren.Api.Helpers;

internal static class WavHeaderParser
{
    public static (bool IsWavHeader, int Length) IsWavHeader(Span<byte> bytes)
    {
        if (bytes.Length < 12)
        {
            return (false, -1);
        }
        ReadOnlySpan<byte> riff = "RIFF"u8;
        ReadOnlySpan<byte> wave = "WAVE"u8;
        return (bytes[..4].SequenceEqual(riff)
            && bytes[8..12].SequenceEqual(wave), BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8]));
    }

    public static TimeSpan GetWavDuration(Span<byte> bytes)
    {
        if (bytes.Length < 44)
        {
            throw new InvalidDataException("传入的数据长度不足");
        }

        (bool foundFmt, int fmtStartIndex, int fmtChunkSize) = FindChunk(bytes, "fmt "u8);

        if (!foundFmt || fmtChunkSize < 16)
        {
            throw new InvalidDataException("WAV 文件格式不正确（找不到 fmt 块或其大小不足）。");
        }

        int fmtContentStartIndex = fmtStartIndex + 8;
        short format = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(fmtContentStartIndex, 2));
        if (format != 1 && format != 3)
        {
            throw new NotImplementedException("当前仅支持未压缩的 PCM 及采用 IEEE Float 格式的 WAV 文件。");
        }
        short channelNumber = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(fmtContentStartIndex + 2, 2));
        int sampleRate = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(fmtContentStartIndex + 2 + 2, 4));
        // Byte Rate (4 bytes): + 2 + 2 + 4
        // Block Align (2 bytes): + 2 + 2 + 4 + 4
        short bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(bytes.Slice(fmtContentStartIndex + 2 + 2 + 4 + 4 + 2, 2));

        (bool foundData, _, int dataChunkSize) = FindChunk(bytes, "data"u8);

        if (!foundData || dataChunkSize <= 0)
        {
            throw new InvalidDataException("WAV 文件格式不正确（找不到 data 块或其大小不正确）。");
        }

        double bytesPerSecond = sampleRate * channelNumber * (bitsPerSample / 8.0);
        double durationInSeconds = dataChunkSize / bytesPerSecond;
        return TimeSpan.FromSeconds(durationInSeconds);
    }

    private static (bool ChunkFound, int StartIndex, int ChunkSize) FindChunk(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> chunkName)
    {
        // 跳过 RIFF、文件长度及 WAVE 标识
        int index = 12;
        while (index + 8 <= bytes.Length)
        {
            ReadOnlySpan<byte> currentChunkName = bytes.Slice(index, 4);
            int currentChunkSize = BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(index + 4, 4));
            if (currentChunkName.SequenceEqual(chunkName))
            {
                return (true, index, currentChunkSize);
            }
            index += 8 + currentChunkSize;
        }
        return (false, -1, -1);
    }
}
