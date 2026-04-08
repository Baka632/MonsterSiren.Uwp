using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

/// <summary>
/// 表示专辑收藏夹中的一个项目。
/// </summary>
/// <param name="AlbumCid">专辑 CID。</param>
/// <param name="AlbumName">专辑名称。</param>
/// <param name="Artistes">专辑艺术家。</param>
public record struct AlbumFavoriteItem(
    string AlbumCid,
    string AlbumName,
    IEnumerable<string> Artistes)
{
    /// <summary>
    /// 指示此收藏夹项目是否损坏。
    /// </summary>
    [JsonIgnore]
    public bool IsCorruptedItem { get; init; }
}