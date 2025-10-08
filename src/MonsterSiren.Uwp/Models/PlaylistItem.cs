using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示播放列表的一个项目。
/// </summary>
/// <param name="SongCid">歌曲 CID。</param>
/// <param name="AlbumCid">专辑 CID。</param>
/// <param name="SongTitle">歌曲标题。</param>
/// <param name="AlbumTitle">专辑标题。</param>
/// <param name="SongDuration">歌曲时长。</param>
public record struct PlaylistItem(string SongCid, string AlbumCid, string SongTitle, string AlbumTitle, TimeSpan SongDuration)
{
    /// <summary>
    /// 指示此播放列表项目是否损坏。
    /// </summary>
    [JsonIgnore]
    public bool IsCorruptedItem { get; init; }
}