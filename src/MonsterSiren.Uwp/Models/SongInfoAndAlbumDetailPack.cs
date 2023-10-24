namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 同时包含 <see cref="Api.Models.Song.SongInfo"/> 与 <see cref="Api.Models.Album.AlbumDetail"/> 的结构
/// </summary>
public struct SongInfoAndAlbumDetailPack
{
    /// <summary>
    /// <see cref="Api.Models.Song.SongInfo"/> 的实例
    /// </summary>
    public SongInfo SongInfo { get; set; }
    /// <summary>
    /// <see cref="Api.Models.Album.AlbumDetail"/> 的实例
    /// </summary>
    public AlbumDetail AlbumDetail { get; set; }

    /// <summary>
    /// 构造 <see cref="SongInfoAndAlbumDetailPack"/> 的新实例
    /// </summary>
    /// <param name="songInfo">一个 <see cref="Api.Models.Song.SongInfo"/> 实例</param>
    /// <param name="albumDetail">一个 <see cref="Api.Models.Album.AlbumDetail"/> 实例</param>
    public SongInfoAndAlbumDetailPack(SongInfo songInfo, AlbumDetail albumDetail)
    {
        SongInfo = songInfo;
        AlbumDetail = albumDetail;
    }
}
