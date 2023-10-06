using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 提供音乐服务的类
/// </summary>
public static class MusicService
{
    /// <summary>
    /// 当播放器的音量发生改变时引发
    /// </summary>
    public static event Action<double> PlayerVolumeChanged;
    /// <summary>
    /// 当播放器的静音状态发生改变时引发
    /// </summary>
    public static event Action<bool> PlayerMuteStateChanged;
    /// <summary>
    /// 当播放器的随机播放状态发生改变时引发
    /// </summary>
    public static event Action<bool> PlayerShuffleStateChanged;
    /// <summary>
    /// 当播放器的循环播放状态发生改变时引发
    /// </summary>
    public static event Action<PlayerRepeatingState> PlayerRepeatingStateChanged;
    /// <summary>
    /// 当播放器的播放曲目发生改变时引发
    /// </summary>
    public static event Action<CurrentMediaPlaybackItemChangedEventArgs> PlayerPlayItemChanged;
    /// <summary>
    /// 当播放器播放出现错误时引发
    /// </summary>
    public static event Action<MediaPlayerFailedEventArgs> PlayerMediaFailed;
    /// <summary>
    /// 当播放器的播放状态发生改变时引发
    /// </summary>
    public static event Action<MediaPlaybackState> PlayerPlaybackStateChanged;
    /// <summary>
    /// 当播放器的播放位置发生改变时引发
    /// </summary>
    public static event Action<TimeSpan> PlayerPositionChanged;
    /// <summary>
    /// 当音乐的时长发生改变时引发
    /// </summary>
    public static event Action<TimeSpan> MusicDurationChanged;

    /// <summary>
    /// 获取或设置播放器的音量
    /// </summary>
    public static double PlayerVolume
    {
        get => mediaPlayer.Volume;
        set => mediaPlayer.Volume = value;
    }

    /// <summary>
    /// 获取播放器的播放状态
    /// </summary>
    public static MediaPlaybackState PlayerPlayBackState => mediaPlayer.PlaybackSession.PlaybackState;

    /// <summary>
    /// 获取或设置播放器的静音状态
    /// </summary>
    public static bool PlayerMuteState
    {
        get => mediaPlayer.IsMuted;
        set => mediaPlayer.IsMuted = value;
    }

    /// <summary>
    /// 获取或设置播放器的随机播放状态
    /// </summary>
    public static bool PlayerShuffleState
    {
        get => mediaPlaybackList.ShuffleEnabled;
        set
        {
            mediaPlaybackList.ShuffleEnabled = value;
            PlayerShuffleStateChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// 获取或设置播放器的循环播放状态
    /// </summary>
    public static PlayerRepeatingState PlayerRepeatingState
    {
        get
        {
            return mediaPlaybackList.AutoRepeatEnabled switch
            {
                true when mediaPlayer.IsLoopingEnabled == false => PlayerRepeatingState.RepeatAll,
                false when mediaPlayer.IsLoopingEnabled == false => PlayerRepeatingState.None,
                _ => PlayerRepeatingState.RepeatSingle,
            };
        }
        set
        {
            switch (value)
            {
                case PlayerRepeatingState.None:
                    mediaPlaybackList.AutoRepeatEnabled = false;
                    mediaPlayer.IsLoopingEnabled = false;
                    PlayerRepeatingStateChanged?.Invoke(PlayerRepeatingState.None);
                    break;
                case PlayerRepeatingState.RepeatAll:
                    mediaPlaybackList.AutoRepeatEnabled = true;
                    mediaPlayer.IsLoopingEnabled = false;
                    PlayerRepeatingStateChanged?.Invoke(PlayerRepeatingState.RepeatAll);
                    break;
                case PlayerRepeatingState.RepeatSingle:
                    mediaPlaybackList.AutoRepeatEnabled = false;
                    mediaPlayer.IsLoopingEnabled = true;
                    PlayerRepeatingStateChanged?.Invoke(PlayerRepeatingState.RepeatSingle);
                    break;
            }
        }
    }

    private static readonly MediaPlayer mediaPlayer = new()
    {
        AutoPlay = true,
        AudioCategory = MediaPlayerAudioCategory.Media
    };
    private static readonly MediaPlaybackList mediaPlaybackList = new()
    {
        MaxPlayedItemsToKeepOpen = 5
    };

    static MusicService()
    {
        mediaPlayer.Source = mediaPlaybackList;

        mediaPlayer.VolumeChanged += (sender, arg) =>
        {
            double volume = sender.Volume;
            PlayerVolumeChanged?.Invoke(volume);
        };
        mediaPlayer.IsMutedChanged += (sender, arg) =>
        {
            bool mute = sender.IsMuted;
            PlayerMuteStateChanged?.Invoke(mute);
        };
        mediaPlayer.PlaybackSession.PositionChanged += (session, arg) =>
        {
            TimeSpan position = session.Position;
            PlayerPositionChanged?.Invoke(position);
        };
        mediaPlayer.PlaybackSession.PlaybackStateChanged += (session, obj) =>
        {
            MediaPlaybackState state = session.PlaybackState;
            PlayerPlaybackStateChanged?.Invoke(state);
        };
        mediaPlayer.PlaybackSession.NaturalDurationChanged += (session, obj) =>
        {
            TimeSpan naturalDuration = session.NaturalDuration;
            MusicDurationChanged?.Invoke(naturalDuration);
        };
        mediaPlayer.MediaFailed += (sender, args) =>
        {
            PlayerMediaFailed?.Invoke(args);
        };

        mediaPlaybackList.CurrentItemChanged += (sender, args) =>
        {
            PlayerPlayItemChanged?.Invoke(args);
        };
    }

    /// <summary>
    /// 添加要播放的音乐
    /// </summary>
    /// <param name="media">表示音乐的 <see cref="MediaPlaybackItem"/></param>
    public static void AddMusic(MediaPlaybackItem media)
    {
        mediaPlaybackList.Items.Add(media);
    }

    /// <summary>
    /// 切换到上一个播放项
    /// </summary>
    public static void PreviousMusic()
    {
        mediaPlaybackList.MovePrevious();
    }

    /// <summary>
    /// 切换到下一个播放项
    /// </summary>
    public static void NextMusic()
    {
        mediaPlaybackList.MoveNext();
    }

    /// <summary>
    /// 开始播放音乐
    /// </summary>
    public static void PlayMusic()
    {
        switch (mediaPlayer.PlaybackSession.PlaybackState)
        {
            case MediaPlaybackState.None:
            case MediaPlaybackState.Paused:
                mediaPlayer.Play();
                break;
            case MediaPlaybackState.Playing:
            default:
                break;
        }
    }
    
    /// <summary>
    /// 暂停播放音乐
    /// </summary>
    public static void PauseMusic()
    {
        switch (mediaPlayer.PlaybackSession.PlaybackState)
        {
            case MediaPlaybackState.Playing:
                mediaPlayer.Pause();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 终止音乐播放
    /// </summary>
    public static void StopMusic()
    {
        mediaPlayer.Pause();
        mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
        mediaPlaybackList.Items.Clear();
    }
}
