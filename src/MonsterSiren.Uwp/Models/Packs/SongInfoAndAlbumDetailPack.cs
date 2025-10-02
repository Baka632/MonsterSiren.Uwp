namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 同时包含 <see cref="Api.Models.Song.SongInfo"/> 与 <see cref="Api.Models.Album.AlbumDetail"/> 的结构。
/// </summary>
/// <param name="SongInfo"><see cref="Api.Models.Song.SongInfo"/> 的实例。</param>
/// <param name="AlbumDetail"><see cref="Api.Models.Album.AlbumDetail"/> 的实例。</param>
public record struct SongInfoAndAlbumDetailPack(SongInfo SongInfo, AlbumDetail AlbumDetail);