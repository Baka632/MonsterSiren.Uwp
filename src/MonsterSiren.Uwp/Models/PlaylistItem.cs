namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示播放列表的一个项目
/// </summary>
/// <param name="SongCid">歌曲 CID</param>
/// <param name="AlbumCid">专辑 CID</param>
/// <param name="SongTitle">歌曲标题</param>
/// <param name="SongDuration">歌曲时长</param>
public record struct PlaylistItem(string SongCid, string AlbumCid, string SongTitle, TimeSpan SongDuration);