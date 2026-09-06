using MonsterSiren.Api.Models.Song;
using MonsterSiren.Api.Helpers.AudioHeaderParser;
using Microsoft.IO;
using System.Buffers;

namespace MonsterSiren.Api.Services;

/// <summary>
/// 塞壬唱片歌曲服务。
/// </summary>
public static partial class SongService
{
    private static readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager = new(new RecyclableMemoryStreamManager.Options()
    {
        BlockSize = 2048, // 2KB
        LargeBufferMultiple = 32 * 1024, // 32KB
        UseExponentialLargeBuffer = true,
        MaximumBufferSize = 512 * 1024, // 512KB
        MaximumSmallPoolFreeBytes = 1 * 1024 * 1024,
        MaximumLargePoolFreeBytes = 5 * 1024 * 1024,
        ZeroOutBuffer = false,
        GenerateCallStacks = false,
    });

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
    /// 通过部分读取文件，获取歌曲时长。
    /// </summary>
    /// <remarks>
    /// <para>本方法至多通过网络读取 512 KB 的数据，如果仍无法解析出时长，则会抛出 <see cref="InvalidOperationException"/>。</para>
    /// <para>请在此情况下下载完整文件后交由其他库解析。</para>
    /// </remarks>
    /// <param name="uri">歌曲文件的 <see cref="Uri"/>。</param>
    /// <returns>歌曲的时长。</returns>
    /// <exception cref="HttpRequestException">HTTP 请求出错。</exception>
    /// <exception cref="ArgumentException">URI 格式不正确。</exception>
    /// <exception cref="InvalidDataException">数据格式错误。</exception>
    /// <exception cref="NotImplementedException">不支持的音频格式。</exception>
    /// <exception cref="InvalidOperationException">已达到最大大小限制，解析仍未成功。请下载完整文件后交由其他库解析。</exception>
    public static async Task<TimeSpan> GetSongDurationAsync(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException("Uri 必须是绝对 Uri。", nameof(uri));
        }

        // 初始 2 KB。
        int audioFileExpectedLength = 2048;
        // 最大 512 KB。
        const int maxSize = 512 * 1024;
        long actualFileSize = -1;

        using RecyclableMemoryStream stream = recyclableMemoryStreamManager.GetStream("AudioParser", 2048);

        while (stream.Length < maxSize
            && audioFileExpectedLength < (actualFileSize == -1 ? maxSize : Math.Min(actualFileSize, maxSize)))
        {
            stream.Position = stream.Length;

            using HttpRequestMessage request = new(HttpMethod.Get, uri);
            request.Headers.Range = new(stream.Length, audioFileExpectedLength - 1);

            using HttpResponseMessage response = await HttpClientProvider.HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            actualFileSize = response.Content.Headers.ContentRange?.Length ?? -1;

            using Stream webstream = await response.Content.ReadAsStreamAsync();
            webstream.CopyTo(stream);

            ReadOnlySequence<byte> sequence = stream.GetReadOnlySequence().Slice(0, audioFileExpectedLength);

            try
            {
                if (WavHeaderParser.IsWavHeader(sequence))
                {
                    return WavHeaderParser.GetWavDuration(sequence);
                }
                else if (Mp3HeaderParser.IsMp3Header(sequence))
                {
                    return Mp3HeaderParser.GetMp3Duration(sequence, actualFileSize);
                }
                else
                {
#if DEBUG
                    System.Diagnostics.Debugger.Break();
#endif
                    throw new NotImplementedException("尚未实现对其他音频格式的支持");
                }
            }
            catch (InsufficientDataException insufficientData)
            {
                audioFileExpectedLength = insufficientData.RequiredBytes;
            }
        }

        throw new InvalidOperationException("已达到最大大小限制，解析仍未成功。");
    }
}
