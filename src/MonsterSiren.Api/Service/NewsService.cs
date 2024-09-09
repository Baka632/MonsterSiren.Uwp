using MonsterSiren.Api.Models.News;

namespace MonsterSiren.Api.Service;

/// <summary>
/// 塞壬唱片新闻服务
/// </summary>
public static class NewsService
{
    /// <summary>
    /// 获取推荐新闻
    /// </summary>
    /// <returns>包含推荐新闻的 <see cref="IEnumerable{T}"/></returns>
    /// <exception cref="InvalidOperationException">出现未知错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<IEnumerable<RecommendedNewsInfo>> GetRecommendedNewsAsync()
    {
        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync("recommends");
        ResponsePackage<IEnumerable<RecommendedNewsInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<IEnumerable<RecommendedNewsInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

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
    /// 获取新闻列表
    /// </summary>
    /// <returns>包含新闻列表的 <see cref="ListPackage{T}"/>，若结果较多，将只返回前十项</returns>
    /// <exception cref="InvalidOperationException">出现未知错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<ListPackage<NewsInfo>> GetNewsListAsync()
    {
        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync("news");
        ResponsePackage<ListPackage<NewsInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<ListPackage<NewsInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

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
    /// 获取新闻列表
    /// </summary>
    /// <param name="lastCid">上次请求中，列表最后一项的 CID</param>
    /// <returns>包含新闻列表的 <see cref="ListPackage{T}"/>，若结果较多，将只返回前十项</returns>
    /// <exception cref="ArgumentOutOfRangeException">参数错误</exception>
    /// <exception cref="ArgumentNullException"><paramref name="lastCid"/> 为 null 或空白</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<ListPackage<NewsInfo>> GetNewsListAsync(string lastCid)
    {
        if (string.IsNullOrWhiteSpace(lastCid))
        {
            throw new ArgumentNullException(nameof(lastCid), $"“{nameof(lastCid)}”不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"news?lastCid={lastCid}");
        ResponsePackage<ListPackage<NewsInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<ListPackage<NewsInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"出现错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }

    /// <summary>
    /// 获取新闻的详细信息
    /// </summary>
    /// <param name="cid">新闻的 CID</param>
    /// <returns>表示新闻详细信息的 <see cref="NewsDetail"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">参数错误</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cid"/> 为 null 或空白</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<NewsDetail> GetDetailedNewsInfoAsync(string cid)
    {
        if (string.IsNullOrWhiteSpace(cid))
        {
            throw new ArgumentNullException(nameof(cid), $"“{nameof(cid)}”不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"news/{cid}");
        ResponsePackage<NewsDetail> result = await JsonSerializer.DeserializeAsync<ResponsePackage<NewsDetail>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"传入参数错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }
}
