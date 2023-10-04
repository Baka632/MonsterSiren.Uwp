using MonsterSiren.Api.Models.Album;
using MonsterSiren.Api.Models.News;

namespace MonsterSiren.Api.Service;

/// <summary>
/// 塞壬唱片搜索服务
/// </summary>
public static class SearchService
{
    /// <summary>
    /// 搜索专辑及新闻信息
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <returns>包含专辑及新闻信息的 <see cref="SearchAlbumAndNewsResult"/>，若结果较多，各分类将只返回前十项</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="keyword"/> 为 null 或空白</exception>
    /// <exception cref="ArgumentException">参数错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<SearchAlbumAndNewsResult> SearchAlbumAndNews(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentOutOfRangeException(nameof(keyword), "搜索关键字不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"search?keyword={keyword}");
        ResponsePackage<SearchAlbumAndNewsResult> result = await JsonSerializer.DeserializeAsync<ResponsePackage<SearchAlbumAndNewsResult>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentException($"传入参数错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }

    /// <summary>
    /// 搜索专辑信息
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <returns>包含专辑信息的 <see cref="ListPackage{T}"/>，若结果较多，将只返回前十项</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="keyword"/> 为 null 或空白</exception>
    /// <exception cref="ArgumentException">参数错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<ListPackage<AlbumInfo>> SearchAlbum(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentOutOfRangeException(nameof(keyword), "搜索关键字不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"search/album?keyword={keyword}");
        ResponsePackage<ListPackage<AlbumInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<ListPackage<AlbumInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentException($"传入参数错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }

    /// <summary>
    /// 搜索专辑信息
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <param name="lastCid">上次请求中，列表最后一项的 CID</param>
    /// <returns>包含专辑信息的 <see cref="ListPackage{T}"/>，若结果较多，将只返回前十项</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="keyword"/> 或 <paramref name="lastCid"/> 为 null 或空白</exception>
    /// <exception cref="ArgumentException">参数错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<ListPackage<AlbumInfo>> SearchAlbum(string keyword, string lastCid)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentOutOfRangeException(nameof(keyword), "搜索关键字不能为 null 或空白。");
        }

        if (string.IsNullOrWhiteSpace(lastCid))
        {
            throw new ArgumentOutOfRangeException(nameof(lastCid), $"“{nameof(lastCid)}”不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"search/album?keyword={keyword}&lastCid={lastCid}");
        ResponsePackage<ListPackage<AlbumInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<ListPackage<AlbumInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentException($"传入参数错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }

    /// <summary>
    /// 搜索新闻信息
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <returns>包含新闻信息的 <see cref="ListPackage{T}"/>，若结果较多，将只返回前十项</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="keyword"/> 为 null 或空白</exception>
    /// <exception cref="ArgumentException">参数错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<ListPackage<NewsInfo>> SearchNews(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentOutOfRangeException(nameof(keyword), "搜索关键字不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"search/news?keyword={keyword}");
        ResponsePackage<ListPackage<NewsInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<ListPackage<NewsInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentException($"传入参数错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }

    /// <summary>
    /// 搜索新闻信息
    /// </summary>
    /// <param name="keyword">搜索关键字</param>
    /// <param name="lastCid">上次请求中，列表最后一项的 CID</param>
    /// <returns>包含新闻信息的 <see cref="ListPackage{T}"/>，若结果较多，将只返回前十项</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="keyword"/> 或 <paramref name="lastCid"/> 为 null 或空白</exception>
    /// <exception cref="ArgumentException">参数错误</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task<ListPackage<NewsInfo>> SearchNews(string keyword, string lastCid)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new ArgumentOutOfRangeException(nameof(keyword), "搜索关键字不能为 null 或空白。");
        }

        if (string.IsNullOrWhiteSpace(lastCid))
        {
            throw new ArgumentOutOfRangeException(nameof(lastCid), $"“{nameof(lastCid)}”不能为 null 或空白。");
        }

        Stream jsonStream = await HttpClientProvider.HttpClient.GetStreamAsync($"search/news?keyword={keyword}&lastCid={lastCid}");
        ResponsePackage<ListPackage<NewsInfo>> result = await JsonSerializer.DeserializeAsync<ResponsePackage<ListPackage<NewsInfo>>>(jsonStream, CommonValues.DefaultJsonSerializerOptions);

        if (result.IsSuccess())
        {
            return result.Data;
        }
        else
        {
            throw new ArgumentException($"传入参数错误\n错误代码：{result.Code}\n错误信息：{result.Message}");
        }
    }
}

