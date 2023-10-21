using System.Collections.Specialized;
using Windows.Media.Playback;

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
    /// 当播放器的媒体开始被替换时引发
    /// </summary>
    public static event Action PlayerMediaReplacing;
    /// <summary>
    /// 当播放器的播放位置发生改变时引发
    /// </summary>
    public static event Action<TimeSpan> PlayerPositionChanged;
    /// <summary>
    /// 当播放器播放结束时引发
    /// </summary>
    public static event Action PlayerMediaEnded;
    /// <summary>
    /// 当音乐的时长发生改变时引发
    /// </summary>
    public static event Action<TimeSpan> MusicDurationChanged;
    /// <summary>
    /// 当音乐停止播放时引发
    /// </summary>
    public static event Action MusicStopped;
    /// <summary>
    /// 当播放列表发生变化时引发
    /// </summary>
    public static event NotifyCollectionChangedEventHandler PlaylistChanged;

    /// <summary>
    /// 获取播放器的播放状态
    /// </summary>
    public static MediaPlaybackState PlayerPlayBackState
    {
        get => mediaPlayer.PlaybackSession.PlaybackState;
    }

    /// <summary>
    /// 获取或设置播放器的音量
    /// </summary>
    public static double PlayerVolume
    {
        get => mediaPlayer.Volume;
        set => mediaPlayer.Volume = value;
    }

    /// <summary>
    /// 获取或设置播放器的播放位置
    /// </summary>
    public static TimeSpan PlayerPosition
    {
        get => mediaPlayer.PlaybackSession.Position;
        set
        {
            if (value > mediaPlayer.PlaybackSession.NaturalDuration)
            {
                mediaPlayer.PlaybackSession.Position = mediaPlayer.PlaybackSession.NaturalDuration;
            }
            else if (value < TimeSpan.Zero)
            {
                mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
            }
            else
            {
                mediaPlayer.PlaybackSession.Position = value;
            }
        }
    }

    /// <summary>
    /// 获取当前播放项的长度
    /// </summary>
    public static TimeSpan MusicDuration
    {
        get => mediaPlayer.PlaybackSession.NaturalDuration;
    }

    /// <summary>
    /// 确定播放器的播放列表是否有曲目
    /// </summary>
    public static bool IsPlayerPlaylistHasMusic
    {
        get => CurrentMediaPlaybackList.Count != 0;
    }

    /// <summary>
    /// 获取播放器当前的播放列表
    /// </summary>
    public static NowPlayingList CurrentMediaPlaybackList { get; }

    /// <summary>
    /// 获取播放器当前的 <see cref="MediaPlaybackItem"/>
    /// </summary>
    public static MediaPlaybackItem CurrentMediaPlaybackItem
    {
        get => mediaPlaybackList.CurrentItem;
    }

    /// <summary>
    /// 获取或设置播放器的静音状态
    /// </summary>
    public static bool IsPlayerMuted
    {
        get => mediaPlayer.IsMuted;
        set => mediaPlayer.IsMuted = value;
    }

    /// <summary>
    /// 获取或设置播放器的随机播放状态
    /// </summary>
    public static bool IsPlayerShuffleEnabled
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
    private static readonly MediaPlaybackList mediaPlaybackList = new();

    static MusicService()
    {
        mediaPlayer.Source = mediaPlaybackList;
        CurrentMediaPlaybackList = new NowPlayingList(mediaPlaybackList.Items);

        //下面的事件处理器在 UI 线程引发事件，这样可以让与 UI 相关的代码在处理这些事件时不会出错
        mediaPlayer.VolumeChanged += async (sender, arg) =>
        {
            double volume = sender.Volume;
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerVolumeChanged?.Invoke(volume);
            });
        };
        mediaPlayer.IsMutedChanged += async (sender, arg) =>
        {
            bool mute = sender.IsMuted;
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerMuteStateChanged?.Invoke(mute);
            });
        };
        mediaPlayer.PlaybackSession.PositionChanged += async (session, arg) =>
        {
            TimeSpan position = session.Position;
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerPositionChanged?.Invoke(position);
            });
        };
        mediaPlayer.PlaybackSession.PlaybackStateChanged += async (session, obj) =>
        {
            MediaPlaybackState state = session.PlaybackState;
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerPlaybackStateChanged?.Invoke(state);
            });
        };
        mediaPlayer.PlaybackSession.NaturalDurationChanged += async (session, obj) =>
        {
            TimeSpan naturalDuration = session.NaturalDuration;
            await UIThreadHelper.RunOnUIThread(() =>
            {
                MusicDurationChanged?.Invoke(naturalDuration);
            });
        };
        mediaPlayer.MediaFailed += async (sender, args) =>
        {
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerMediaFailed?.Invoke(args);
            });
        };
        mediaPlayer.MediaEnded += async (sender, args) =>
        {
            PlayerPosition = TimeSpan.Zero;
            NextMusic();
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerMediaEnded?.Invoke();
            });
        };

        mediaPlaybackList.CurrentItemChanged += async (sender, args) =>
        {
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerPlayItemChanged?.Invoke(args);
            });
        };

        CurrentMediaPlaybackList.CollectionChanged += async (sender, args) =>
        {
            await UIThreadHelper.RunOnUIThread(() =>
            {
                if (args.Action == NotifyCollectionChangedAction.Remove && IsPlayerPlaylistHasMusic != true)
                {
                    mediaPlayer.Pause();
                    mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
                    MusicStopped?.Invoke();
                }

                PlaylistChanged?.Invoke(sender, args);
            });
        };
    }

    /// <summary>
    /// 添加要播放的音乐
    /// </summary>
    /// <param name="media">表示音乐的 <see cref="MediaPlaybackItem"/></param>
    public static async void AddMusic(MediaPlaybackItem media)
    {
        bool shouldPlayMusic = false;
        if (IsPlayerPlaylistHasMusic != true)
        {
            shouldPlayMusic = true;
        }

        await UIThreadHelper.RunOnUIThread(() => CurrentMediaPlaybackList.Add(media));

        if (shouldPlayMusic)
        {
            PlayMusic();
        }
    }

    /// <summary>
    /// 将当前的音乐替换为指定的音乐
    /// </summary>
    /// <param name="media">包含音乐的 <see cref="MediaPlaybackItem"/></param>
    public static async void ReplaceMusic(MediaPlaybackItem media)
    {
        StopMusic();

        await UIThreadHelper.RunOnUIThread(() =>
        {
            PlayerMediaReplacing?.Invoke();
        });

        await UIThreadHelper.RunOnUIThread(() => CurrentMediaPlaybackList.Add(media));
        PlayMusic();
    }

    /// <summary>
    /// 将当前的音乐列表替换为指定的音乐列表
    /// </summary>
    /// <param name="medias">表示音乐的 <see cref="IEnumerable{T}"/></param>
    public static async void ReplaceMusic(IEnumerable<MediaPlaybackItem> medias)
    {
        StopMusic();

        await UIThreadHelper.RunOnUIThread(() =>
        {
            PlayerMediaReplacing?.Invoke();
            foreach (MediaPlaybackItem media in medias)
            {
                CurrentMediaPlaybackList.Add(media);
            }
        });
        PlayMusic();
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
    ///将正在播放的项目更改为索引指向的项目
    /// </summary>
    /// <param name="index">项目在正在播放列表的索引</param>
    /// <exception cref="ArgumentOutOfRangeException">索引指向不存在的项目</exception>
    public static void MoveTo(uint index)
    {
        if (index + 1 > CurrentMediaPlaybackList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        mediaPlaybackList.MoveTo(index);
    }

    /// <summary>
    /// 移除指定索引指向的播放项
    /// </summary>
    /// <param name="index">项目在正在播放列表的索引</param>
    /// <exception cref="ArgumentOutOfRangeException">索引为负，或指向不存在的项目</exception>
    public static async void RemoveAt(int index)
    {
        if (index < 0 || index + 1 > CurrentMediaPlaybackList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        await UIThreadHelper.RunOnUIThread(() => CurrentMediaPlaybackList.RemoveAt(index));
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
    public static async void StopMusic()
    {
        mediaPlayer.Pause();
        mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;

        await UIThreadHelper.RunOnUIThread(() =>
        {
            CurrentMediaPlaybackList.Clear();
            MusicStopped?.Invoke();
        });
    }
}
