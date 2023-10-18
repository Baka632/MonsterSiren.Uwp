using Windows.Media.Playback;

namespace MonsterSiren.Uwp.Helpers.DataTemplateSelectors;

/// <summary>
/// 正在播放项目的数据模板选择器
/// </summary>
public class NowPlayingItemDataTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// 普通项目的数据模板
    /// </summary>
    public DataTemplate Normal { get; set; }
    /// <summary>
    /// 正在播放项目的数据模板
    /// </summary>
    public DataTemplate NowPlaying { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is MediaPlaybackItem playbackItem && MusicService.CurrentMediaPlaybackItem.Equals(playbackItem))
        {
            return NowPlaying;
        }
        else
        {
            return Normal;
        }
    }
}
