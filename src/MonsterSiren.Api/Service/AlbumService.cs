using MonsterSiren.Api.Models.Album;

namespace MonsterSiren.Api.Service;

/// <summary>
/// 塞壬唱片专辑服务
/// </summary>
public static class AlbumService
{
    /// <summary>
    /// 获取全部专辑信息
    /// </summary>
    /// <returns>包含全部专辑信息的 <see cref="IEnumerable{T}"/></returns>
    /// <exception cref="InvalidOperationException">出现未知错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<IEnumerable<AlbumInfo>> GetAllAlbumsAsync()
    {
        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync("albums");
        ResponsePackage<IEnumerable<AlbumInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<IEnumerable<AlbumInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data!;
        }
        else
        {
            throw new InvalidOperationException($"出现错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }

    /// <summary>
    /// 获取专辑的基本信息
    /// </summary>
    /// <param name="cid">专辑 CID</param>
    /// <returns>包含专辑基本信息的 <see cref="AlbumInfo"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">参数错误</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cid"/> 为 null 或空白</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<AlbumInfo> GetAlbumInfoAsync(string cid)
    {
        if (string.IsNullOrWhiteSpace(cid))
        {
            throw new ArgumentNullException(nameof(cid), $"“{nameof(cid)}”不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"album/{cid}/data");
        ResponsePackage<AlbumInfo> result = await JsonSerializer.DeserializeAsync<ResponsePackage<AlbumInfo>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

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
    /// 获取专辑的详细信息
    /// </summary>
    /// <param name="cid">专辑 CID</param>
    /// <returns>包含专辑详细信息的 <see cref="AlbumDetail"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">参数错误</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cid"/> 为 null 或空白</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<AlbumDetail> GetAlbumDetailedInfoAsync(string cid)
    {
        if (string.IsNullOrWhiteSpace(cid))
        {
            throw new ArgumentNullException(nameof(cid), $"“{nameof(cid)}”不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"album/{cid}/detail");
        ResponsePackage<AlbumDetail> result = await JsonSerializer.DeserializeAsync<ResponsePackage<AlbumDetail>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

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
}
