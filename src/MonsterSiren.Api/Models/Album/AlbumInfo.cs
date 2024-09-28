using System.Diagnostics;

namespace MonsterSiren.Api.Models.Album;

/// <summary>
/// 提供专辑基本信息的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct AlbumInfo : IEquatable<AlbumInfo>
{
    /// <summary>
    /// 专辑的 CID
    /// </summary>
    public string Cid { get; set; } = string.Empty;
    /// <summary>
    /// 专辑名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 专辑引言
    /// </summary>
    public string Intro { get; set; } = string.Empty;
    /// <summary>
    /// Belong（TBD）
    /// </summary>
    public string Belong { get; set; } = string.Empty;
    /// <summary>
    /// 专辑封面的 Uri
    /// </summary>
    public string CoverUrl { get; set; } = string.Empty;
    /// <summary>
    /// 专辑大图封面的 Uri
    /// </summary>
    public string CoverDeUrl { get; set; } = string.Empty;
    /// <summary>
    /// 专辑艺术家
    /// </summary>
    public IEnumerable<string> Artistes { get; set; } = [];

    /// <summary>
    /// 使用指定的参数构造 <see cref="AlbumInfo"/> 的新实例
    /// </summary>
    public AlbumInfo(string cid, string name, string intro, string belong, string coverUrl, string coverDeUrl, IEnumerable<string> artistes)
    {
        Cid = cid;
        Name = name;
        Intro = intro;
        Belong = belong;
        CoverUrl = coverUrl;
        CoverDeUrl = coverDeUrl;
        Artistes = artistes;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is AlbumInfo info && Equals(info);
    }

    /// <inheritdoc/>
    public readonly bool Equals(AlbumInfo other)
    {
        bool isArtistesEqual = Artistes is not null && other.Artistes is not null
            ? Artistes.SequenceEqual(other.Artistes)
            : ReferenceEquals(Artistes, other.Artistes);

        return Cid == other.Cid &&
               Name == other.Name &&
               Intro == other.Intro &&
               Belong == other.Belong &&
               CoverUrl == other.CoverUrl &&
               CoverDeUrl == other.CoverDeUrl &&
               isArtistesEqual;
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        int hashCode = 45371074;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Cid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Intro);
        hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(Belong);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CoverUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CoverDeUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<string>>.Default.GetHashCode(Artistes);
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="AlbumInfo"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="AlbumInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="AlbumInfo"/> 实例</param>
    public static bool operator ==(AlbumInfo left, AlbumInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="AlbumInfo"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="AlbumInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="AlbumInfo"/> 实例</param>
    public static bool operator !=(AlbumInfo left, AlbumInfo right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(Cid)} = {Cid}, {nameof(Name)} = {Name}, {nameof(Intro)} = {Intro}, {nameof(Belong)} = {Belong}, {nameof(CoverUrl)} = {CoverUrl}, {nameof(CoverDeUrl)} = {CoverDeUrl}, {nameof(Artistes)} Count = {Artistes.Count()}";
    }
}
