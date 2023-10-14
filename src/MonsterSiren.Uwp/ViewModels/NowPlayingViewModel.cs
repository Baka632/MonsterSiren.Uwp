using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="NowPlayingPage"/> 提供视图模型
/// </summary>
public partial class NowPlayingViewModel : ObservableObject
{
    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;

    /// <summary>
    /// 使用指定的 <see cref="TimeSpan"/> 更新音乐播放位置的相关属性
    /// </summary>
    /// <param name="timeSpan">指定的 <see cref="TimeSpan"/></param>
    public void UpdateMusicPosition(TimeSpan timeSpan)
    {
        MusicInfo.MusicPosition = MusicService.PlayerPosition = timeSpan;
    }
}