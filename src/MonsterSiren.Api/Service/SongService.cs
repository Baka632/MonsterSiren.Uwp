using System.Buffers;
using System.Buffers.Binary;
using MonsterSiren.Api.Models.Song;

namespace MonsterSiren.Api.Service;

/// <summary>
/// 塞壬唱片歌曲服务。
/// </summary>
public static class SongService
{
    /// <summary>
    /// 获取歌曲详细信息。
    /// </summary>
    /// <param name="cid">歌曲 CID。</param>
    /// <returns>包含歌曲详细信息的 <see cref="SongDetail"/>。</returns>
    /// <exception cref="ArgumentOutOfRangeException">参数错误。</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cid"/> 为 <see langword="null"/> 或空白。</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    public static async Task<SongDetail> GetSongDetailedInfoAsync(string cid)
    {
        if (string.IsNullOrWhiteSpace(cid))
        {
            throw new ArgumentNullException(nameof(cid), $"“{nameof(cid)}”不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"song/{cid}");
        ResponsePackage<SongDetail> result = await JsonSerializer.DeserializeAsync<ResponsePackage<SongDetail>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"传入参数错误\n错误代码：{result.Code}\n错误信息：{result.Message}")
            {
                Data =
                {
                    ["ErrorCid"] = cid
                }
            };
        }
    }

    /// <summary>
    /// 获取全部歌曲。
    /// </summary>
    /// <returns>包含全部歌曲的 <see cref="ListPackage{T}"/>。</returns>
    /// <exception cref="InvalidOperationException">出现未知错误。</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    public static async Task<ListPackage<SongInfo>> GetAllSongsAsync()
    {
        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync("songs");
        ResponsePackage<ListPackage<SongInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<ListPackage<SongInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new InvalidOperationException($"出现错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }

    /// <summary>
    /// 获取歌曲时长。
    /// </summary>
    /// <param name="uri">歌曲文件的 <see cref="Uri"/>。</param>
    /// <returns>歌曲的时长。</returns>
    /// <exception cref="ArgumentException">URI 格式不正确。</exception>
    /// <exception cref="InvalidDataException">数据格式错误。</exception>
    /// <exception cref="NotImplementedException">不支持的音频格式。</exception>
    public static async Task<TimeSpan> GetSongDurationAsync(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException("Uri 必须是绝对 Uri。", nameof(uri));
        }
        using HttpRequestMessage request = new(HttpMethod.Get, uri);
        // 只请求前 16384 字节以获取文件头信息
        const int bufferSize = 16384;
        request.Headers.Range = new(0, bufferSize - 1);

        using HttpResponseMessage response = await HttpClientProvider.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        byte[] array = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            using Stream stream = await response.Content.ReadAsStreamAsync();
            int bytesRead = await stream.ReadAsync(array, 0, bufferSize);
            Span<byte> buffer = array.AsSpan(0, bytesRead);

            (bool isWavHeader, _) = WavHeaderParser.IsWavHeader(buffer);
            if (isWavHeader)
            {
                TimeSpan duration = WavHeaderParser.GetWavDuration(buffer);
                return duration;
            }
            else
            {
                // 在未来 MP3 相关内容添加时再检查长度。
                throw new NotImplementedException();
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    static class WavHeaderParser
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

            if(!foundFmt || fmtChunkSize < 16)
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
}
