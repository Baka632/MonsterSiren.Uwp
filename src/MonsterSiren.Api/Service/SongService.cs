using MonsterSiren.Api.Models.Song;

namespace MonsterSiren.Api.Service;

/// <summary>
/// 塞壬唱片歌曲服务
/// </summary>
public static class SongService
{
    /// <summary>
    /// 获取歌曲详细信息
    /// </summary>
    /// <param name="cid">歌曲 CID</param>
    /// <returns>包含歌曲详细信息的 <see cref="SongDetail"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">参数错误</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cid"/> 为 null 或空白</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
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
    /// 获取全部歌曲
    /// </summary>
    /// <returns>包含全部歌曲的 <see cref="ListPackage{T}"/></returns>
    /// <exception cref="InvalidOperationException">出现未知错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
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
}
