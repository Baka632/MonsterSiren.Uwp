using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

/// <summary>
/// 表示歌曲收藏夹中的一个项目。
/// </summary>
/// <param name="SongCid">歌曲 CID。</param>
/// <param name="AlbumCid">专辑 CID。</param>
/// <param name="SongTitle">歌曲名称。</param>
/// <param name="AlbumTitle">专辑名称。</param>
/// <param name="SongDuration">歌曲时长。</param>
public record struct SongFavoriteItem(
    string SongCid,
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
