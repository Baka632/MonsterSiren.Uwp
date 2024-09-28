using MonsterSiren.Api.Models.Song;

namespace MonsterSiren.Api.Test;

public class SongServiceTest
{
    [Fact]
    public async Task GetSongDetail()
    {
        SongDetail defaultSongDetail = default;

        // 880368 - Magic Theorem
        SongDetail songDetail = await SongService.GetSongDetailedInfoAsync("880368");
        Assert.NotEqual(defaultSongDetail, songDetail);
        Assert.NotEmpty(songDetail.Artists);
    }
    
    [Fact]
    public async Task GetAllSong()
    {
        SongInfo defaultSongInfo = default;

        ListPackage<SongInfo> songInfos = await SongService.GetAllSongsAsync();
        Assert.NotEmpty(songInfos.List);

        foreach (SongInfo item in songInfos)
        {
            Assert.NotEqual(defaultSongInfo, item);
        }
    }
}
