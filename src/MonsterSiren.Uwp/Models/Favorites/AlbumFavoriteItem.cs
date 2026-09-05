using System.Text.Json.Serialization;
using MonsterSiren.Api.Helpers.Converters;

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
    [property: JsonConverter(typeof(InternedStringArrayConverter))]
    IEnumerable<string> Artistes) : IEquatable<AlbumFavoriteItem>
{
    /// <summary>
    /// 指示此收藏夹项目是否损坏。
    /// </summary>
    [JsonIgnore]
    public bool IsCorruptedItem { get; init; }

    public readonly bool Equals(AlbumFavoriteItem other)
    {
        return AlbumCid == other.AlbumCid
            && AlbumName == other.AlbumName
            && IsCorruptedItem == other.IsCorruptedItem
            && Artistes.SequenceEqual(other.Artistes);
    }

    public override readonly int GetHashCode()
    {
        int hashCode = 1534145849;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlbumCid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlbumName);
        foreach (string artist in Artistes)
        {
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(artist);
        }
        hashCode = hashCode * -1521134295 + IsCorruptedItem.GetHashCode();
        return hashCode;
    }
}