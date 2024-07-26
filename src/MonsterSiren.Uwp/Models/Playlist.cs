using System.Collections.ObjectModel;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个播放列表的结构
/// </summary>
public sealed class Playlist
{
    /// <summary>
    /// 播放列表标题
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 播放列表描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 播放列表总时长（以毫秒为单位）
    /// </summary>
    public int TotalDurationInMillisecond { get; private set; }
    /// <summary>
    /// 播放列表的歌曲列表
    /// </summary>
    public ObservableCollection<SongDetailAndAlbumDetailPack> Items { get; private set; } = [];

    public IEnumerator<SongDetailAndAlbumDetailPack> GetEnumerator()
    {
        return Items.GetEnumerator();
    }
}
