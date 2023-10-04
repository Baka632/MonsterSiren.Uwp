using MonsterSiren.Api.Models.Song;

namespace MonsterSiren.Api.Test;

public class SongServiceTest
{
    [Fact]
    public async void GetSongDetail()
    {
        SongDetail defaultSongDetail = default;

        //880368 - Magic Theorem
        SongDetail songDetail = await SongService.GetSongDetailedInfo("880368");
        Assert.NotEqual(defaultSongDetail, songDetail);
        Assert.NotEmpty(songDetail.Artists);
    }
    
    [Fact]
    public async void GetAllSong()
    {
        SongInfo defaultSongInfo = default;

        ListPackage<SongInfo> songInfos = await SongService.GetAllSongs();
        Assert.NotEmpty(songInfos.List);

        foreach (SongInfo item in songInfos)
        {
            Assert.NotEqual(defaultSongInfo, item);
        }
    }
}
