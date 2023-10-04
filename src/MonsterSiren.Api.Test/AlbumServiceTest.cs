using MonsterSiren.Api.Models.Album;

namespace MonsterSiren.Api.Test;

public class AlbumServiceTest
{
    [Fact]
    public async void GetAllAlbum()
    {
        AlbumInfo defaultAlbumInfo = default;

        IEnumerable<AlbumInfo> result = await AlbumService.GetAllAlbums();
        Assert.NotEmpty(result);
        
        foreach (AlbumInfo item in result)
        {
            Assert.NotEqual(defaultAlbumInfo, item);
        }
    }

    [Fact]
    public async void GetAlbumInfo()
    {
        AlbumInfo defaultAlbumInfo = default;

        //8930 - 登临意OST
        AlbumInfo result = await AlbumService.GetAlbumInfo("8930");
        Assert.NotEqual(defaultAlbumInfo, result);
    }
    
    [Fact]
    public async void GetAlbumDetail()
    {
        AlbumDetail defaultAlbumInfo = default;

        //8930 - 登临意OST
        AlbumDetail result = await AlbumService.GetAlbumDetailedInfo("8930");
        Assert.NotEqual(defaultAlbumInfo, result);
    }
}
