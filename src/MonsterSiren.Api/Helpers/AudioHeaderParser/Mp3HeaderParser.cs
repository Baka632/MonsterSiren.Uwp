using System.Buffers;
using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace MonsterSiren.Api.Helpers.AudioHeaderParser;

internal static class Mp3HeaderParser
{
    private static readonly Dictionary<byte, int> Mpeg2Layer2And3BitrateMapping = new()
    {
        [0b0001] = 8,
        [0b0010] = 16,
        [0b0011] = 24,
        [0b0100] = 32,
        [0b0101] = 40,
        [0b0110] = 48,
        [0b0111] = 56,
        [0b1000] = 64,
        [0b1001] = 80,
        [0b1010] = 96,
        [0b1011] = 112,
        [0b1100] = 128,
        [0b1101] = 144,
        [0b1110] = 160,
    };

    private static readonly ReadOnlyDictionary<MpegAndLayerVersion, Dictionary<byte, int>> BitrateInKbpsDictionary = new(new Dictionary<MpegAndLayerVersion, Dictionary<byte, int>>()
    {
        [MpegAndLayerVersion.Mpeg1Layer1] = new()
        {
            [0b0001] = 32,
            [0b0010] = 64,
            [0b0011] = 96,
            [0b0100] = 128,
            [0b0101] = 160,
            [0b0110] = 192,
            [0b0111] = 224,
            [0b1000] = 256,
            [0b1001] = 288,
            [0b1010] = 320,
            [0b1011] = 352,
            [0b1100] = 384,
            [0b1101] = 416,
            [0b1110] = 448,
        },
        [MpegAndLayerVersion.Mpeg1Layer2] = new()
        {
            [0b0001] = 32,
            [0b0010] = 48,
            [0b0011] = 56,
            [0b0100] = 64,
            [0b0101] = 80,
            [0b0110] = 96,
            [0b0111] = 112,
            [0b1000] = 128,
            [0b1001] = 160,
            [0b1010] = 192,
            [0b1011] = 224,
            [0b1100] = 256,
            [0b1101] = 320,
            [0b1110] = 384,
        },
        [MpegAndLayerVersion.Mpeg1Layer3] = new()
        {
            [0b0001] = 32,
            [0b0010] = 40,
            [0b0011] = 48,
            [0b0100] = 56,
            [0b0101] = 64,
            [0b0110] = 80,
            [0b0111] = 96,
            [0b1000] = 112,
            [0b1001] = 128,
            [0b1010] = 160,
            [0b1011] = 192,
            [0b1100] = 224,
            [0b1101] = 256,
            [0b1110] = 320,
        },
        [MpegAndLayerVersion.Mpeg2Layer1] = new()
        {
            [0b0001] = 32,
            [0b0010] = 48,
            [0b0011] = 56,
            [0b0100] = 64,
            [0b0101] = 80,
            [0b0110] = 96,
            [0b0111] = 112,
            [0b1000] = 128,
            [0b1001] = 144,
            [0b1010] = 160,
            [0b1011] = 176,
            [0b1100] = 192,
            [0b1101] = 224,
            [0b1110] = 256,
        },
        [MpegAndLayerVersion.Mpeg2Layer2] = Mpeg2Layer2And3BitrateMapping,
        [MpegAndLayerVersion.Mpeg2Layer3] = Mpeg2Layer2And3BitrateMapping,
    });

    private static readonly ReadOnlyDictionary<MpegVersion, Dictionary<byte, int>> SamplingRateInHzDictionary = new(new Dictionary<MpegVersion, Dictionary<byte, int>>()
    {
        [MpegVersion.Mpeg1] = new()
        {
            [0b00] = 44100,
            [0b01] = 48000,
            [0b10] = 32000,
        },
        [MpegVersion.Mpeg2] = new()
        {
            [0b00] = 22050,
            [0b01] = 24000,
            [0b10] = 16000,
        },
        [MpegVersion.Mpeg2Dot5] = new()
        {
            [0b00] = 11025,
            [0b01] = 12000,
            [0b10] = 8000,
        },
    });

    public static bool IsMp3Header(ReadOnlySequence<byte> sequence)
    {
        return HasID3Tag(sequence) || HasMpegSyncWord(sequence);
    }

    private static bool HasMpegSyncWord(ReadOnlySequence<byte> sequence)
    {
        if (sequence.Length < 2)
        {
            return false;
        }
        Span<byte> buffer = stackalloc byte[2];
        sequence.Slice(0, 2).CopyTo(buffer);
        return HasMpegSyncWord(buffer);
    }

    private static bool HasMpegSyncWord(Span<byte> bytes)
    {
        if (bytes.Length < 2)
        {
            return false;
        }
        return bytes[0] == 0b1111_1111 && (bytes[1] & 0b1110_0000) == 0b1110_0000;
    }

    private static bool HasID3Tag(ReadOnlySequence<byte> sequence)
    {
        if (sequence.Length < 3)
        {
            return false;
        }
        Span<byte> buffer = stackalloc byte[3];
        sequence.Slice(0, 3).CopyTo(buffer);

        return buffer.SequenceEqual("ID3"u8);
    }

    public static TimeSpan GetMp3Duration(ReadOnlySequence<byte> sequence, long actualFileSize)
    {
        int firstFrameIndex;
        int tagSize;
        int headerSize;

        if (HasID3Tag(sequence))
        {
            // 3 字节 => “ID3”文本；3 字节 => 版本号；4 字节 => ID3 标签大小。
            headerSize = 3 + 3 + 4;
            if (sequence.Length < headerSize)
            {
                throw new InsufficientDataException(headerSize);
            }

            Span<byte> tagSizeSpan = stackalloc byte[4];
            sequence.Slice(6, 4).CopyTo(tagSizeSpan);
            tagSize = GetSyncSafeInteger(tagSizeSpan);
            firstFrameIndex = tagSize + headerSize;
        }
        else if(HasMpegSyncWord(sequence))
        {
            firstFrameIndex = 0;
            tagSize = 0;
            headerSize = 0;
        }
        else
        {
            if (sequence.Length < 3)
            {
                throw new InsufficientDataException(3);
            }

            throw new InvalidDataException("不是有效的 MP3 文件。");
        }

        if (firstFrameIndex + 4 > sequence.Length)
        {
            //（可能的）ID3 标签头 +（可能的）ID3 标签大小 + 4 字节的 MPEG 帧头长度。
            throw new InsufficientDataException(tagSize + headerSize + 4);
        }
        Span<byte> frameHeader = stackalloc byte[4];
        sequence.Slice(firstFrameIndex, 4).CopyTo(frameHeader);
        byte byte2 = frameHeader[1];
        byte byte3 = frameHeader[2];
        byte byte4 = frameHeader[3];

        // MP3 简直是一团乱麻，帧头同步字有人说是 12 位，有人说是 11 位。
        // 塞壬唱片官网的 MP3 文件的帧头同步字是 11 位，那就这样比较吧。
        // 参见：https://samples.ffmpeg.org/A-codecs/sf/mpeg_header.html
        if (!HasMpegSyncWord(frameHeader[..2]))
        {
            throw new InvalidDataException("无效的 MP3 帧头，期望一个同步字。");
        }

        // 这里我们忽略 CRC 保护位。
        MpegAndLayerVersion mpegAndLayerVersion = (MpegAndLayerVersion)(byte2 | 0b0000_0001);
        MpegVersion mpegVersion = (MpegVersion)((byte)mpegAndLayerVersion & 0b111_11_001);

        bool hasCrc = (frameHeader[1] & 0b0000_0001) == 0;
        int crcSize = hasCrc ? 2 : 0;

        byte bitrateIndex = (byte)((byte3 & 0b1111_0000) >> 4);
        if (bitrateIndex == 0 || bitrateIndex == 0b1111)
        {
            throw new InvalidDataException("无法处理“free”或无效的比特率索引。");
        }

        byte samplingRateIndex = (byte)((byte3 & 0b0000_11_00) >> 2);
        if (samplingRateIndex == 0b11)
        {
            throw new InvalidOperationException("无法处理保留的采样率索引。");
        }

        int bitrateInKbps = BitrateInKbpsDictionary[mpegAndLayerVersion][bitrateIndex];
        int samplingRateInHz = SamplingRateInHzDictionary[mpegVersion][samplingRateIndex];
        ChannelMode channelMode = (ChannelMode)(byte4 & 0b1100_0000);

        int sideInfoLength = GetSideInfoLength(mpegVersion, channelMode);
        int vbrIndex = firstFrameIndex + 4 + sideInfoLength + crcSize;

        if (vbrIndex + 12 > sequence.Length)
        {
            throw new InsufficientDataException(vbrIndex + 12);
        }

        Span<byte> vbrHeader = stackalloc byte[12];
        sequence.Slice(vbrIndex, 12).CopyTo(vbrHeader);
        Span<byte> vbrTagName = vbrHeader[..4];
        if (vbrTagName.SequenceEqual("Xing"u8) || vbrTagName.SequenceEqual("Info"u8))
        {
            Span<byte> vbrFlag = vbrHeader.Slice(4, 4);
            if ((vbrFlag[3] & 0b0000_0001) == 0b0000_0001)
            {
                Span<byte> vbrTotalFrame = vbrHeader.Slice(8, 4);
                int totalFrameCount = BinaryPrimitives.ReadInt32BigEndian(vbrTotalFrame);
                int samplesPerFrame = GetSamplesPerFrame(mpegAndLayerVersion);
                double durationSeconds = totalFrameCount * samplesPerFrame / (double)samplingRateInHz;
                return TimeSpan.FromSeconds(durationSeconds);
            }
        }

        // 无 VBR 头或 VBR 头无效的情况，尝试 CBR 计算。
        return CalculateDurationCBR(actualFileSize, tagSize, headerSize, bitrateInKbps);
    }

    private static TimeSpan CalculateDurationCBR(long actualFileSize, int tagSize, int headerSize, int bitrateInKbps)
    {
        if (actualFileSize == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(actualFileSize), "由于此歌曲既无有效的 VBR 信息，也无具体文件长度，因此无法计算歌曲时长。");
        }

        long audioDataLength = actualFileSize - tagSize - headerSize;
        if (audioDataLength < 0)
        {
            throw new InvalidDataException("无效的文件大小。");
        }

        double audioDataLengthInBits = audioDataLength * 8d;
        double bitrateInBps = bitrateInKbps * 1000d;
        double seconds = audioDataLengthInBits / bitrateInBps;
        return TimeSpan.FromSeconds(seconds);
    }

    private static int GetSamplesPerFrame(MpegAndLayerVersion version)
    {
        return version switch
        {
            MpegAndLayerVersion.Mpeg1Layer1 or MpegAndLayerVersion.Mpeg2Layer1 or MpegAndLayerVersion.Mpeg2Dot5Layer1 => 384,
            MpegAndLayerVersion.Mpeg1Layer2 or MpegAndLayerVersion.Mpeg2Layer2 or MpegAndLayerVersion.Mpeg2Dot5Layer2 => 1152,
            MpegAndLayerVersion.Mpeg1Layer3 => 1152,
            MpegAndLayerVersion.Mpeg2Layer3 or MpegAndLayerVersion.Mpeg2Dot5Layer3 => 576,
            _ => throw new InvalidDataException("不支持的 MPEG 和 Layer 组合。")
        };
    }

    private static int GetSideInfoLength(MpegVersion mpegVersion, ChannelMode channelMode)
    {
        return mpegVersion switch
        {
            MpegVersion.Mpeg1 => channelMode switch
            {
                ChannelMode.SingleChannel => 17,
                _ => 32,
            },
            _ => channelMode switch
            {
                ChannelMode.SingleChannel => 9,
                _ => 17,
            }
        };
    }

    private static int GetSyncSafeInteger(Span<byte> bytes)
    {
        if (bytes.Length != 4)
        {
            throw new ArgumentException("SyncSafe 整数必须是 4 个字节。", nameof(bytes));
        }
        return (bytes[0] << 21) | (bytes[1] << 14) | (bytes[2] << 7) | bytes[3];
    }

    /// <summary>
    /// 用于将层信息编码为一个字节的枚举。
    /// </summary>
    /// <remarks>
    /// <para>此枚举对应 MPEG 帧头格式的第二个字节，布局是 AAABBCCD（即认为是 11 位同步字）。</para>
    /// <para>A 指代固定的帧同步字，固定为全 1；B 指代 MPEG 版本；D 指代保护位，本枚举不表示它，固定为 1；其他位均为 0。</para>
    /// </remarks>
    private enum MpegVersion : byte
    {
        Reserved = 0b111_01_00_1,
        Mpeg2Dot5 = 0b111_00_00_1,
        Mpeg2 = 0b111_10_00_1,
        Mpeg1 = 0b111_11_00_1
    }

    /// <summary>
    /// 用于将层信息编码为一个字节的枚举。
    /// </summary>
    /// <remarks>
    /// <para>此枚举对应 MPEG 帧头格式的第二个字节，布局是 AAABBCCD（即认为是 11 位同步字）。</para>
    /// <para>A 指代固定的帧同步字，固定为全 1；C 指代层信息；D 指代保护位，本枚举不表示它，固定为 1；其他位均为 0。</para>
    /// </remarks>
    private enum LayerVersion : byte
    {
        Reserved = 0b111_00_00_1,
        Layer3 = 0b111_00_01_1,
        Layer2 = 0b111_00_10_1,
        Layer1 = 0b111_00_11_1
    }

    /// <summary>
    /// 用于将 MPEG 版本和层信息编码为一个字节的枚举。
    /// </summary>
    /// <remarks>
    /// <para>此枚举对应 MPEG 帧头格式的第二个字节，布局是 AAABBCCD（即认为是 11 位同步字）。</para>
    /// <para>A 指代固定的帧同步字，固定为全 1；B 指代 MPEG 版本；C 指代层信息；D 指代保护位，本枚举不表示它，固定为 1。</para>
    /// <para><strong>此枚举不能表示具有 CRC 检查的 MPEG 帧。</strong></para>
    /// </remarks>
    private enum MpegAndLayerVersion : byte
    {
        Reserved = MpegVersion.Reserved | LayerVersion.Reserved,
        Mpeg2Dot5Layer3 = MpegVersion.Mpeg2Dot5 | LayerVersion.Layer3,
        Mpeg2Dot5Layer2 = MpegVersion.Mpeg2Dot5 | LayerVersion.Layer2,
        Mpeg2Dot5Layer1 = MpegVersion.Mpeg2Dot5 | LayerVersion.Layer1,
        Mpeg2Layer3 = MpegVersion.Mpeg2 | LayerVersion.Layer3,
        Mpeg2Layer2 = MpegVersion.Mpeg2 | LayerVersion.Layer2,
        Mpeg2Layer1 = MpegVersion.Mpeg2 | LayerVersion.Layer1,
        Mpeg1Layer3 = MpegVersion.Mpeg1 | LayerVersion.Layer3,
        Mpeg1Layer2 = MpegVersion.Mpeg1 | LayerVersion.Layer2,
        Mpeg1Layer1 = MpegVersion.Mpeg1 | LayerVersion.Layer1,
    }

    /// <summary>
    /// 用于将声道信息编码为一个字节的枚举。
    /// </summary>
    /// <remarks>
    /// <para>此枚举对应 MPEG 帧头格式的第三个字节，布局是 IIJJKLMM。</para>
    /// <para>I 指代声道模式；其他位均不表示，视为 0，初始化时请注意。</para>
    /// </remarks>
    private enum ChannelMode : byte
    {
        Stereo = 0b00_000000,
        JointStereo = 0b01_000000,
        DualChannel = 0b10_000000,
        SingleChannel = 0b11_000000,
    }
}