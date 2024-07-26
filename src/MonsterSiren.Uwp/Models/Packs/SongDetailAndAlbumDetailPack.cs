namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 同时包含 <see cref="Api.Models.Song.SongDetail"/> 与 <see cref="Api.Models.Album.AlbumDetail"/> 的结构
/// </summary>
/// <param name="songDetail">一个 <see cref="Api.Models.Song.SongDetail"/> 实例</param>
/// <param name="albumDetail">一个 <see cref="Api.Models.Album.AlbumDetail"/> 实例</param>
public struct SongDetailAndAlbumDetailPack(SongDetail songDetail, AlbumDetail albumDetail)
{
    /// <summary>
    /// <see cref="Api.Models.Song.SongDetail"/> 的实例
    /// </summary>
    public SongDetail SongDetail { get; set; } = songDetail;
    /// <summary>
    /// <see cref="Api.Models.Album.AlbumDetail"/> 的实例
    /// </summary>
    public AlbumDetail AlbumDetail { get; set; } = albumDetail;
}
