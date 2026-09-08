namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 页中显示数据提供模型的结构。
/// </summary>
/// <param name="AlbumName">专辑名称。</param>
/// <param name="AlbumCid">专辑 CID。</param>
/// <param name="Artistes">专辑艺术家。</param>
/// <param name="AlbumIntro">专辑引言。</param>
/// <param name="Songs">专辑歌曲列表。</param>
public readonly record struct AlbumDetailDisplaySource(
    string AlbumName,
    string AlbumCid,
    IEnumerable<string> Artistes,
    string AlbumIntro,
    IEnumerable<SongInfo> Songs);
