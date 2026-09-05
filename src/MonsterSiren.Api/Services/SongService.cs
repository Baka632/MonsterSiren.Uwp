using System.Buffers;
using MonsterSiren.Api.Models.Song;
using MonsterSiren.Api.Helpers.AudioHeaderParser;

namespace MonsterSiren.Api.Services;

/// <summary>
/// 塞壬唱片歌曲服务。
/// </summary>
public static partial class SongService
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

            if (WavHeaderParser.IsWavHeader(buffer))
            {
                return WavHeaderParser.GetWavDuration(buffer);
            }
            else if (Mp3HeaderParser.IsMp3Header(buffer))
            {
                return Mp3HeaderParser.GetMp3Duration(buffer);
            }
            else
            {
                throw new NotImplementedException("尚未实现对其他音频格式的支持");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }
}
