using System.Diagnostics;

namespace MonsterSiren.Api.Models.Song;

/// <summary>
/// 提供歌曲基本信息的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct SongInfo : IEquatable<SongInfo>
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
    /// 歌曲艺术家
    /// </summary>
    public IEnumerable<string> Artists { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// 以默认参数构造 <see cref="SongInfo"/> 的新实例
    /// </summary>
    public SongInfo()
    {
    }

    /// <inheritdoc/>
    public readonly override bool Equals(object? obj)
    {
        return obj is SongInfo info && Equals(info);
    }

    /// <inheritdoc/>
    public readonly bool Equals(SongInfo other)
    {
        bool isArtistsEqual = Artists is not null && other.Artists is not null
            ? Artists.SequenceEqual(other.Artists)
            : ReferenceEquals(Artists, other.Artists);

        return Cid == other.Cid &&
               Name == other.Name &&
               AlbumCid == other.AlbumCid &&
               isArtistsEqual;
    }

    /// <inheritdoc/>
    public readonly override int GetHashCode()
    {
        int hashCode = 422685196;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Cid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlbumCid);
        hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<string>>.Default.GetHashCode(Artists);
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="SongInfo"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="SongInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="SongInfo"/> 实例</param>
    public static bool operator ==(SongInfo left, SongInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="SongInfo"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="SongInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="SongInfo"/> 实例</param>
    public static bool operator !=(SongInfo left, SongInfo right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(Cid)} = {Cid}, {nameof(Name)} = {Name}, {nameof(AlbumCid)} = {AlbumCid}, {nameof(Artists)} Count = {Artists.Count()}";
    }
}
