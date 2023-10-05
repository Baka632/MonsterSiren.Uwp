using System.Diagnostics;
using MonsterSiren.Api.Models.Song;

namespace MonsterSiren.Api.Models.Album;

/// <summary>
///  提供专辑详细信息的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct AlbumDetail : IEquatable<AlbumDetail>
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
    /// 专辑内全部歌曲的信息
    /// </summary>
    public IEnumerable<SongInfo> Songs { get; set; } = Enumerable.Empty<SongInfo>();

    /// <summary>
    /// 使用指定参数构造 <see cref="AlbumDetail"/> 的新实例
    /// </summary>
    public AlbumDetail(string cid, string name, string intro, string belong, string coverUrl, string coverDeUrl, IEnumerable<SongInfo> songs)
    {
        Cid = cid;
        Name = name;
        Intro = intro;
        Belong = belong;
        CoverUrl = coverUrl;
        CoverDeUrl = coverDeUrl;
        Songs = songs;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is AlbumDetail detail && Equals(detail);
    }

    /// <inheritdoc/>
    public readonly bool Equals(AlbumDetail other)
    {
        bool isSongsEqual = Songs is not null && other.Songs is not null
            ? Songs.SequenceEqual(other.Songs)
            : ReferenceEquals(Songs, other.Songs);

        return Cid == other.Cid &&
               Name == other.Name &&
               Intro == other.Intro &&
               Belong == other.Belong &&
               CoverUrl == other.CoverUrl &&
               CoverDeUrl == other.CoverDeUrl &&
               isSongsEqual;
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        int hashCode = -821887162;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Cid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Intro);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Belong);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CoverUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CoverDeUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<SongInfo>>.Default.GetHashCode(Songs);
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="AlbumDetail"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="AlbumDetail"/> 实例</param>
    /// <param name="right">第二个 <see cref="AlbumDetail"/> 实例</param>
    public static bool operator ==(AlbumDetail left, AlbumDetail right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="AlbumDetail"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="AlbumDetail"/> 实例</param>
    /// <param name="right">第二个 <see cref="AlbumDetail"/> 实例</param>
    public static bool operator !=(AlbumDetail left, AlbumDetail right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(Cid)} = {Cid}, {nameof(Name)} = {Name}, {nameof(Intro)} = {Intro}, {nameof(Belong)} = {Belong}, {nameof(CoverUrl)} = {CoverUrl}, {nameof(CoverDeUrl)} = {CoverDeUrl}, {nameof(Songs)} Count = {Songs.Count()}";
    }
}
