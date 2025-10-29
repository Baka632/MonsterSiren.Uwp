using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

public record struct SongFavoriteItem(string SongCid,
                                      string AlbumCid,
                                      string SongTitle,
                                      string AlbumTitle,
                                      TimeSpan SongDuration)
{
    /// <summary>
    /// 指示此收藏夹项目是否损坏。
    /// </summary>
    [JsonIgnore]
    public bool IsCorruptedItem { get; init; }
}
