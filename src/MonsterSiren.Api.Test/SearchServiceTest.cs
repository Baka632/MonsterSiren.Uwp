using MonsterSiren.Api.Models.Album;
using MonsterSiren.Api.Models.News;

namespace MonsterSiren.Api.Test;

public class SearchServiceTest
{
    [Fact]
    public async void SearchAlbumAndNews()
    {
        SearchAlbumAndNewsResult result = await SearchService.SearchAlbumAndNewsAsync("Spark");

        Assert.NotEmpty(result.Albums.List);
        Assert.NotEmpty(result.News.List);

        AlbumInfo defaultAlbumInfo = default;
        foreach (AlbumInfo item in result.Albums)
        {
            Assert.NotEqual(defaultAlbumInfo, item);
        }

        NewsInfo defaultNewsInfo = default;
        foreach (NewsInfo item in result.News)
        {
            Assert.NotEqual(defaultNewsInfo, item);
        }
    }

    [Fact]
    public async void SearchAlbum()
    {
        ListPackage<AlbumInfo> result = await SearchService.SearchAlbumAsync("OST");

        Assert.NotEmpty(result.List);

        AlbumInfo defaultAlbumInfo = default;
        foreach (AlbumInfo item in result)
        {
            Assert.NotEqual(defaultAlbumInfo, item);
        }

        string lastCid = result.List.Last().Cid;
        ListPackage<AlbumInfo> resultWithLastCid = await SearchService.SearchAlbumAsync("OST", lastCid);
        Assert.NotEmpty(resultWithLastCid.List);
        foreach (AlbumInfo item in resultWithLastCid)
        {
            Assert.NotEqual(defaultAlbumInfo, item);
        }
    }
    
    [Fact]
    public async void SearchNews()
    {
        ListPackage<NewsInfo> result = await SearchService.SearchNewsAsync("D.D.D");

        Assert.NotEmpty(result.List);

        NewsInfo defaultNewsInfo = default;
        foreach (NewsInfo item in result)
        {
            Assert.NotEqual(defaultNewsInfo, item);
        }

        string lastCid = result.List.Last().Cid;
        ListPackage<NewsInfo> resultWithLastCid = await SearchService.SearchNewsAsync("D.D.D", lastCid);
        Assert.NotEmpty(resultWithLastCid.List);
        foreach (NewsInfo item in resultWithLastCid)
        {
            Assert.NotEqual(defaultNewsInfo, item);
        }
    }
}