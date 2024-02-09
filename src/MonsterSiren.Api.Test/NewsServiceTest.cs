using MonsterSiren.Api.Models.News;

namespace MonsterSiren.Api.Test;

public class NewsServiceTest
{
    [Fact]
    public async void GetRecommendedNews()
    {
        RecommendedNewsInfo defaultRecommendedNewsInfo = default;

        IEnumerable<RecommendedNewsInfo> result = await NewsService.GetRecommendedNewsAsync();

        Assert.NotEmpty(result);

        foreach (RecommendedNewsInfo item in result)
        {
            Assert.NotEqual(defaultRecommendedNewsInfo, item);
        }
    }

    [Fact]
    public async void GetNewsList()
    {
        NewsInfo defaultNewsInfo = default;

        ListPackage<NewsInfo> result = await NewsService.GetNewsListAsync();
        Assert.NotEmpty(result.List);

        foreach (NewsInfo item in result)
        {
            Assert.NotEqual(defaultNewsInfo, item);
        }

        string lastCid = result.List.Last().Cid;

        ListPackage<NewsInfo> resultWithLastCid = await NewsService.GetNewsListAsync(lastCid);
        Assert.NotEmpty(resultWithLastCid.List);

        foreach (NewsInfo item in resultWithLastCid.List)
        {
            Assert.NotEqual(defaultNewsInfo, item);
        }
    }

    [Fact]
    public async void GetDetailedNewsInfo()
    {
        NewsDetail defaultNewsDetail = default;

        //605965 - #AUS小屋 - https://monster-siren.hypergryph.com/info/605965
        NewsDetail result = await NewsService.GetDetailedNewsInfoAsync("605965");

        Assert.NotEqual(defaultNewsDetail, result);
    }
}
