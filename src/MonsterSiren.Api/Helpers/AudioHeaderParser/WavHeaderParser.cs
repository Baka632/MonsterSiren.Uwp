using System.Buffers;
using System.Buffers.Binary;

namespace MonsterSiren.Api.Helpers.AudioHeaderParser;

internal static class WavHeaderParser
{
    public static bool IsWavHeader(ReadOnlySequence<byte> sequence)
    {
        if (sequence.Length < 12)
        {
            return false;
        }

        ReadOnlySpan<byte> riff = "RIFF"u8;
        ReadOnlySpan<byte> wave = "WAVE"u8;
        Span<byte> sequenceByte1 = stackalloc byte[4];
        Span<byte> sequenceByte2 = stackalloc byte[4];

        sequence.Slice(0, 4).CopyTo(sequenceByte1);
        sequence.Slice(8, 4).CopyTo(sequenceByte2);

        return sequenceByte1.SequenceEqual(riff) && sequenceByte2.SequenceEqual(wave);
    }

    public static TimeSpan GetWavDuration(ReadOnlySequence<byte> sequence)
    {
        if (sequence.Length < 44)
        {
            throw new InsufficientDataException(44);
        }

        (int fmtStartIndex, int fmtChunkSize) = FindChunkOrThrow(sequence, "fmt "u8);

        if (fmtChunkSize < 16)
        {
            throw new InvalidDataException("WAV 文件格式不正确（fmt 块大小不足）。");
        }
        else if (fmtStartIndex + 8 + fmtChunkSize > sequence.Length)
        {
            throw new InsufficientDataException(fmtStartIndex + 8 + fmtChunkSize);
        }

        Span<byte> fmtContent = fmtChunkSize < 256 ? stackalloc byte[fmtChunkSize] : new byte[fmtChunkSize];
        sequence.Slice(fmtStartIndex + 8, fmtChunkSize).CopyTo(fmtContent);
        short format = BinaryPrimitives.ReadInt16LittleEndian(fmtContent[..2]);
        if (format != 1 && format != 3)
        {
            throw new NotImplementedException("当前仅支持未压缩的 PCM 及采用 IEEE Float 格式的 WAV 文件。");
        }
        short channelNumber = BinaryPrimitives.ReadInt16LittleEndian(fmtContent.Slice(2, 2));
        int sampleRate = BinaryPrimitives.ReadInt32LittleEndian(fmtContent.Slice(2 + 2, 4));
        // 跳过的：
        // Byte Rate (4 bytes): + 2 + 2 + 4
        // Block Align (2 bytes): + 2 + 2 + 4 + 4
        short bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(fmtContent.Slice(2 + 2 + 4 + 4 + 2, 2));

        (_, int dataChunkSize) = FindChunkOrThrow(sequence, "data"u8);

        if (dataChunkSize <= 0)
        {
            throw new InvalidDataException("WAV 文件格式不正确（data 块大小不正确）。");
        }

        double bytesPerSecond = sampleRate * channelNumber * (bitsPerSample / 8.0);
        double durationInSeconds = dataChunkSize / bytesPerSecond;
        return TimeSpan.FromSeconds(durationInSeconds);
    }

    private static (int StartIndex, int ChunkSize) FindChunkOrThrow(ReadOnlySequence<byte> sequence, ReadOnlySpan<byte> chunkName)
    {
        // 跳过 RIFF、文件长度及 WAVE 标识
        int index = 12;
        Span<byte> currentChunkName = stackalloc byte[4];
        Span<byte> currentChunkSizeSpan = stackalloc byte[4];
        while (index + 8 <= sequence.Length)
        {
            sequence.Slice(index, 4).CopyTo(currentChunkName);
            sequence.Slice(index + 4, 4).CopyTo(currentChunkSizeSpan);
            int currentChunkSize = BinaryPrimitives.ReadInt32LittleEndian(currentChunkSizeSpan);
            if (currentChunkName.SequenceEqual(chunkName))
            {
                return (index, currentChunkSize);
            }

            // 4 字节的块名 + 4 字节的块大小 + 块内容大小
            index += 8 + currentChunkSize;
        }

        throw new InsufficientDataException(index + 8);
    }
}
