using System.Diagnostics;

namespace MonsterSiren.Api.Models.Song;

/// <summary>
///  提供歌曲详细信息的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct SongDetail : IEquatable<SongDetail>
{
    /// <summary>
    /// 歌曲的 CID
    /// </summary>
    public string Cid { get; set; } = string.Empty;
    /// <summary>
    /// 歌曲名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 歌曲所属专辑 CID
    /// </summary>
    public string AlbumCid { get; set; } = string.Empty;
    /// <summary>
    /// 歌曲音频文件的 Uri
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;
    /// <summary>
    /// 歌曲歌词文件的 Uri
    /// </summary>
    public string LyricUrl { get; set; } = string.Empty;
    /// <summary>
    /// 歌曲 MV 的 Uri
    /// </summary>
    public string MvUrl { get; set; } = string.Empty;
    /// <summary>
    /// 歌曲 MV 封面的 Uri
    /// </summary>
    public string MvCoverUrl { get; set; } = string.Empty;
    /// <summary>
    /// 歌曲艺术家
    /// </summary>
    public IEnumerable<string> Artists { get; set; } = [];

    /// <summary>
    /// 使用指定的参数构造 <see cref="SongDetail"/> 的新实例
    /// </summary>
    public SongDetail(string cid, string name, string albumCid, string sourceUrl, string lyricUrl, string mvUrl, string mvCoverUrl, IEnumerable<string> artists)
    {
        Cid = cid;
        Name = name;
        AlbumCid = albumCid;
        SourceUrl = sourceUrl;
        LyricUrl = lyricUrl;
        MvUrl = mvUrl;
        MvCoverUrl = mvCoverUrl;
        Artists = artists;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is SongDetail detail && Equals(detail);
    }

    /// <inheritdoc/>
    public readonly bool Equals(SongDetail other)
    {
        bool isArtistsEqual = Artists is not null && other.Artists is not null
            ? Artists.SequenceEqual(other.Artists)
            : ReferenceEquals(Artists, other.Artists);

        return Cid == other.Cid &&
               Name == other.Name &&
               AlbumCid == other.AlbumCid &&
               SourceUrl == other.SourceUrl &&
               LyricUrl == other.LyricUrl &&
               MvUrl == other.MvUrl &&
               MvCoverUrl == other.MvCoverUrl &&
               isArtistsEqual;
    }

    /// <inheritdoc/>
    public readonly override int GetHashCode()
    {
        int hashCode = -1912451913;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Cid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlbumCid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SourceUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LyricUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MvUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MvCoverUrl);
        hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<string>>.Default.GetHashCode(Artists);
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="SongDetail"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="SongDetail"/> 实例</param>
    /// <param name="right">第二个 <see cref="SongDetail"/> 实例</param>
    public static bool operator ==(SongDetail left, SongDetail right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="SongDetail"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="SongDetail"/> 实例</param>
    /// <param name="right">第二个 <see cref="SongDetail"/> 实例</param>
    public static bool operator !=(SongDetail left, SongDetail right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(Cid)} = {Cid}, {nameof(Name)} = {Name}, {nameof(AlbumCid)} = {AlbumCid}, {nameof(SourceUrl)} = {SourceUrl}, {nameof(LyricUrl)} = {LyricUrl}, {nameof(MvUrl)} = {MvUrl}, {nameof(MvCoverUrl)} = {MvCoverUrl}, {nameof(Artists)} Count = {Artists.Count()}";
    }
}

