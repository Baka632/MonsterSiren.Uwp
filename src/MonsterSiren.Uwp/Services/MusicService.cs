using Windows.Media.Playback;
using System.Collections.Specialized;
using Windows.Media.Casting;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 提供音乐服务的类。
/// </summary>
public static class MusicService
{
    /// <summary>
    /// 当播放器的音量发生改变时引发。
    /// </summary>
    public static event Action<double> PlayerVolumeChanged;
    /// <summary>
    /// 当播放器的静音状态发生改变时引发。
    /// </summary>
    public static event Action<bool> PlayerMuteStateChanged;
    /// <summary>
    /// 当播放器的随机播放状态发生改变时引发。
    /// </summary>
    public static event Action<bool> PlayerShuffleStateChanged;
    /// <summary>
    /// 当播放器的循环播放状态发生改变时引发。
    /// </summary>
    public static event Action<PlayerRepeatingState> PlayerRepeatingStateChanged;
    /// <summary>
    /// 当播放器的播放曲目发生改变时引发。
    /// </summary>
    public static event Action<CurrentMediaPlaybackItemChangedEventArgs> PlayerPlayItemChanged;
    /// <summary>
    /// 当播放器播放出现错误时引发。
    /// </summary>
    public static event Action<MediaPlayerFailedEventArgs> PlayerMediaFailed;
    /// <summary>
    /// 当播放器的播放状态发生改变时引发。
    /// </summary>
    public static event Action<MediaPlaybackState> PlayerPlaybackStateChanged;
    /// <summary>
    /// 当播放器的媒体开始被替换时引发。
    /// </summary>
    public static event Action PlayerMediaReplacing;
    /// <summary>
    /// 当播放器的播放位置发生改变时引发。
    /// </summary>
    public static event Action<TimeSpan> PlayerPositionChanged;
    /// <summary>
    /// 当播放器播放结束时引发。
    /// </summary>
    public static event Action PlayerMediaEnded;
    /// <summary>
    /// 当播放器是否具有音乐的状态发生改变时引发。
    /// </summary>
    public static event Action PlayerHasMusicStateChanged;
    /// <summary>
    /// 当音乐的时长发生改变时引发。
    /// </summary>
    public static event Action<TimeSpan> MusicDurationChanged;
    /// <summary>
    /// 当准备播放音乐的状态改变时引发。
    /// </summary>
    public static event Action MusicPrepareModeChanged;
    /// <summary>
    /// 当音乐停止播放时引发。
    /// </summary>
    public static event Action MusicStopped;
    /// <summary>
    /// 当音乐即将停止播放时引发。
    /// </summary>
    public static event Action MusicStopping;
    /// <summary>
    /// 当播放列表发生变化时引发。
    /// </summary>
    public static event NotifyCollectionChangedEventHandler PlaylistChanged;
    /// <summary>
    /// 当播放列表中播放项播放失败时引发。
    /// </summary>
    public static event Action<MediaPlaybackItemFailedEventArgs> PlaylistItemFailed;

    /// <summary>
    /// 获取播放器的播放状态。
    /// </summary>
    public static MediaPlaybackState PlayerPlayBackState
    {
        get => mediaPlayer.PlaybackSession.PlaybackState;
    }

    /// <summary>
    /// 获取或设置播放器的音量。
    /// </summary>
    public static double PlayerVolume
    {
        get => mediaPlayer.Volume;
        set => mediaPlayer.Volume = value;
    }

    /// <summary>
    /// 获取或设置播放器的播放位置。
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
    /// 获取当前播放项的长度。
    /// </summary>
    public static TimeSpan MusicDuration
    {
        get => mediaPlayer.PlaybackSession.NaturalDuration;
    }

    /// <summary>
    /// 确定播放器的播放列表是否有曲目。
    /// </summary>
    public static bool IsPlayerPlaylistHasMusic
    {
        get => CurrentMediaPlaybackList.Count != 0;
    }

    /// <summary>
    /// 获取播放器当前的播放列表。
    /// </summary>
    public static NowPlayingList CurrentMediaPlaybackList { get; }

    /// <summary>
    /// 获取播放器当前的 <see cref="MediaPlaybackItem"/>。
    /// </summary>
    public static MediaPlaybackItem CurrentMediaPlaybackItem
    {
        get => mediaPlaybackList.CurrentItem;
    }

    // TODO: 应该自己管理随机播放列表。
    /// <summary>
    /// 获取在随机播放时使用的只读列表。
    /// </summary>
    public static IReadOnlyList<MediaPlaybackItem> CurrentShuffledMediaPlaybackList
    {
        get => mediaPlaybackList.ShuffledItems;
    }

    /// <summary>
    /// 获取或设置播放器的静音状态。
    /// </summary>
    public static bool IsPlayerMuted
    {
        get => mediaPlayer.IsMuted;
        set => mediaPlayer.IsMuted = value;
    }

    /// <summary>
    /// 获取或设置播放器的随机播放状态。
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
    /// 获取是否准备播放音乐的值。
    /// </summary>
    public static bool IsMusicPreparing
    {
        get => isMusicPreparing;
        private set
        {
            isMusicPreparing = value;
            _ = UIThreadHelper.RunOnUIThread(() => MusicPrepareModeChanged?.Invoke());
        }
    }

    /// <summary>
    /// 获取或设置播放器的循环播放状态。
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
    private static bool isMusicPreparing;

    static MusicService()
    {
        mediaPlayer.Source = mediaPlaybackList;
        CurrentMediaPlaybackList = new NowPlayingList(mediaPlaybackList.Items);

        // 下面的事件处理器在 UI 线程引发事件，这样可以让与 UI 相关的代码在处理这些事件时不会出错
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
        mediaPlaybackList.ItemFailed += async (sender, args) =>
        {
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlaylistItemFailed?.Invoke(args);
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
        CurrentMediaPlaybackList.PropertyChanged += async (sender, args) =>
        {
            if (args.PropertyName == nameof(CurrentMediaPlaybackList.Count) && (CurrentMediaPlaybackList.Count - 1 == 0 || CurrentMediaPlaybackList.Count + 1 == 1))
            {
                await UIThreadHelper.RunOnUIThread(() => PlayerHasMusicStateChanged?.Invoke());
            }
        };
    }

    /// <summary>
    /// 获取用于投送音频到其他设备的 <see cref="CastingSource"/>。
    /// </summary>
    /// <returns>一个 <see cref="CastingSource"/>。</returns>
    public static CastingSource GetCastingSource()
    {
        return mediaPlayer.GetAsCastingSource();
    }

    /// <summary>
    /// 添加要播放的音乐。
    /// </summary>
    /// <param name="media">表示音乐的 <see cref="MediaPlaybackItem"/>。</param>
    public static void AddMusic(MediaPlaybackItem media)
    {
        bool shouldStartPlaying = !IsPlayerPlaylistHasMusic;

        try
        {
            if (shouldStartPlaying)
            {
                IsMusicPreparing = true;
            }

            CurrentMediaPlaybackList.Add(media);

            if (shouldStartPlaying)
            {
                PlayMusic();
            }
        }
        finally
        {
            if (shouldStartPlaying)
            {
                IsMusicPreparing = false;
            }
        }
    }

    /// <summary>
    /// 添加要播放的音乐。
    /// </summary>
    /// <param name="items">包含音乐的 <see cref="IAsyncEnumerable{T}"/>。</param>
    public static async Task AddMusic(IAsyncEnumerable<MediaPlaybackItem> items)
    {
        bool isStopping = false;
        bool isNoMusicInPlaylistBefore = !IsPlayerPlaylistHasMusic;

        try
        {
            if (isNoMusicInPlaylistBefore)
            {
                IsMusicPreparing = true;
            }

            MusicStopping += OnMusicStopping;

            MediaPlaybackItem firstItem = null;
            bool isFirst = true;

            await foreach (MediaPlaybackItem item in items)
            {
                if (isFirst)
                {
                    firstItem = item;
                }

                if (isStopping)
                {
                    break;
                }

                CurrentMediaPlaybackList.Add(item);

                if (isNoMusicInPlaylistBefore && isFirst)
                {
                    PlayMusic();
                    isFirst = false;
                }
            }

            if (IsPlayerShuffleEnabled && !isStopping && firstItem is not null
                && mediaPlaybackList.ShuffledItems.Contains(firstItem))
            {
                // 强制第一首歌曲在最前面
                List<MediaPlaybackItem> shuffleList = new(mediaPlaybackList.ShuffledItems);
                shuffleList.Remove(firstItem);
                shuffleList.Insert(0, firstItem);
                mediaPlaybackList.SetShuffledItems(shuffleList);
            }
        }
        finally
        {
            if (isNoMusicInPlaylistBefore)
            {
                IsMusicPreparing = false;
            }

            MusicStopping -= OnMusicStopping;
        }

        void OnMusicStopping() => isStopping = true;
    }

    /// <summary>
    /// 将歌曲插入到当前歌曲之后，即下一首播放。
    /// </summary>
    /// <param name="item">表示音乐的 <see cref="MediaPlaybackItem"/>。</param>
    public static void PlayNext(MediaPlaybackItem item)
    {
        if (!IsPlayerPlaylistHasMusic)
        {
            AddMusic(item);
            return;
        }
        else
        {
            List<MediaPlaybackItem> shuffledPlaybacklist = null;
            if (IsPlayerShuffleEnabled)
            {
                shuffledPlaybacklist = new(CurrentShuffledMediaPlaybackList.Count + 1);
                shuffledPlaybacklist.AddRange(CurrentShuffledMediaPlaybackList);
            }

            CurrentMediaPlaybackList.Insert((int)mediaPlaybackList.CurrentItemIndex + 1, item);

            if (shuffledPlaybacklist is not null)
            {
                int currentItemIndex = shuffledPlaybacklist.IndexOf(CurrentMediaPlaybackItem);
                shuffledPlaybacklist.Insert(currentItemIndex + 1, item);

                mediaPlaybackList.SetShuffledItems(shuffledPlaybacklist);
            }
        }
    }

    /// <summary>
    /// 将歌曲序列插入到当前歌曲之后，即下一首播放。
    /// </summary>
    /// <param name="items">包含音乐的 <see cref="IAsyncEnumerable{T}"/>。</param>
    public static async Task PlayNext(IAsyncEnumerable<MediaPlaybackItem> items)
    {
        bool isStopping = false;

        try
        {
            if (!IsPlayerPlaylistHasMusic)
            {
                await AddMusic(items);
                return;
            }

            int currentIndex = (int)mediaPlaybackList.CurrentItemIndex;

            int currentShuffledIndex = -1;
            List<MediaPlaybackItem> shuffledPlaybacklist = null;
            if (IsPlayerShuffleEnabled)
            {
                shuffledPlaybacklist = [.. CurrentShuffledMediaPlaybackList];
                currentShuffledIndex = shuffledPlaybacklist.IndexOf(CurrentMediaPlaybackItem);
            }
            bool shouldAddToShuffleList = shuffledPlaybacklist is not null;

            MusicStopping += OnMusicStopping;

            int indexOffset = 1;
            await foreach (MediaPlaybackItem item in items)
            {
                if (isStopping)
                {
                    break;
                }

                CurrentMediaPlaybackList.Insert(currentIndex + indexOffset, item);

                if (shouldAddToShuffleList)
                {
                    shuffledPlaybacklist.Insert(currentShuffledIndex + indexOffset, item);
                }

                indexOffset++;
            }

            if (IsPlayerShuffleEnabled && !isStopping && shouldAddToShuffleList)
            {
                mediaPlaybackList.SetShuffledItems(shuffledPlaybacklist);
            }
        }
        finally
        {
            MusicStopping -= OnMusicStopping;
        }

        void OnMusicStopping() => isStopping = true;
    }

    /// <summary>
    /// 将当前的音乐替换为指定的音乐。
    /// </summary>
    /// <param name="media">包含音乐的 <see cref="MediaPlaybackItem"/>。</param>
    public static async void ReplaceMusic(MediaPlaybackItem media)
    {
        try
        {
            await StopMusic();

            IsMusicPreparing = true;

            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerMediaReplacing?.Invoke();
                CurrentMediaPlaybackList.Add(media);
            });

            PlayMusic();
        }
        finally
        {
            IsMusicPreparing = false;
        }
    }

    /// <summary>
    /// 将当前的音乐列表替换为指定的音乐列表。
    /// </summary>
    /// <param name="items">包含音乐的 <see cref="IAsyncEnumerable{T}"/>。</param>
    public static async Task ReplaceMusic(IAsyncEnumerable<MediaPlaybackItem> items)
    {
        bool isStopping = false;

        try
        {
            IsMusicPreparing = true;
            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlayerMediaReplacing?.Invoke();
            });

            MediaPlaybackItem firstItem = null;
            bool isFirst = true;
            await foreach (MediaPlaybackItem media in items)
            {
                if (isFirst)
                {
                    await StopMusic();
                    MusicStopping += OnMusicStopping;
                    firstItem = media;
                }

                if (isStopping)
                {
                    break;
                }

                CurrentMediaPlaybackList.Add(media);

                if (isFirst)
                {
                    PlayMusic();
                    isFirst = false;
                }
            }

            if (IsPlayerShuffleEnabled && firstItem is not null && mediaPlaybackList.ShuffledItems.Contains(firstItem))
            {
                // 强制第一首歌曲在最前面
                List<MediaPlaybackItem> shuffleList = new(mediaPlaybackList.ShuffledItems);
                shuffleList.Remove(firstItem);
                shuffleList.Insert(0, firstItem);
                mediaPlaybackList.SetShuffledItems(shuffleList);
            }
        }
        finally
        {
            IsMusicPreparing = false;
            MusicStopping -= OnMusicStopping;
        }

        void OnMusicStopping() => isStopping = true;
    }

    /// <summary>
    /// 切换到上一个播放项。
    /// </summary>
    public static void PreviousMusic()
    {
        if (mediaPlaybackList.CurrentItemIndex > mediaPlaybackList.Items.Count)
        {
            mediaPlaybackList.MoveTo(0);
        }

        mediaPlaybackList.MovePrevious();
    }

    /// <summary>
    /// 切换到下一个播放项。
    /// </summary>
    public static void NextMusic()
    {
        if (mediaPlaybackList.CurrentItemIndex > mediaPlaybackList.Items.Count)
        {
            mediaPlaybackList.MoveTo(0);
        }

        mediaPlaybackList.MoveNext();
    }

    /// <summary>
    /// 将正在播放的项目更改为索引指向的项目。
    /// </summary>
    /// <param name="index">项目在正在播放列表的索引。</param>
    /// <exception cref="ArgumentOutOfRangeException">索引指向不存在的项目。</exception>
    public static void MoveTo(uint index)
    {
        if (index + 1 > CurrentMediaPlaybackList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        mediaPlaybackList.MoveTo(index);
    }

    /// <summary>
    /// 将正在播放的项目更改为指定的播放项。
    /// </summary>
    /// <remarks>
    /// 指定的播放项必须在正在播放列表中。
    /// </remarks>
    /// <param name="item">指定的播放项。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="item"/> 不在正在播放列表中。</exception>
    public static void MoveTo(MediaPlaybackItem item)
    {
        int index = CurrentMediaPlaybackList.IndexOf(item);

        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(item), "指定的项目不在正在播放列表中。");
        }

        mediaPlaybackList.MoveTo((uint)index);
    }

    /// <summary>
    /// 移除指定索引指向的播放项。
    /// </summary>
    /// <param name="index">项目在正在播放列表的索引。</param>
    /// <exception cref="ArgumentOutOfRangeException">索引为负，或指向不存在的项目。</exception>
    public static void RemoveAt(int index)
    {
        if (index < 0 || index + 1 > CurrentMediaPlaybackList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        CurrentMediaPlaybackList.RemoveAt(index);
    }

    /// <summary>
    /// 替换指定的播放项。
    /// </summary>
    /// <param name="oldItem">要被移除的播放项。</param>
    /// <param name="newItem">用于替换的新播放项。</param>
    /// <exception cref="ArgumentOutOfRangeException">索引为负，或指向不存在的项目。</exception>
    /// <exception cref="System.ArgumentNullException"><paramref name="oldItem"/> 或 <paramref name="newItem"/> 为 <see langword="null"/>。</exception>
    public static void ReplaceAt(MediaPlaybackItem oldItem, MediaPlaybackItem newItem)
    {
        if (oldItem is null)
        {
            throw new ArgumentNullException(nameof(oldItem));
        }

        if (newItem is null)
        {
            throw new ArgumentNullException(nameof(newItem));
        }

        int targetIndex = CurrentMediaPlaybackList.IndexOf(oldItem);

        if (targetIndex == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(oldItem), "指定项不在正在播放列表中。");
        }

        ReplaceAt(newItem, targetIndex);
    }

    /// <summary>
    /// 替换指定索引指向的播放项。
    /// </summary>
    /// <param name="item">替换后的项。</param>
    /// <param name="index">项目在正在播放列表的索引。</param>
    /// <exception cref="ArgumentOutOfRangeException">索引为负，或指向不存在的项目。</exception>
    /// <exception cref="System.ArgumentNullException"><paramref name="item"/> 为 <see langword="null"/>。</exception>
    public static void ReplaceAt(MediaPlaybackItem item, int index)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (index + 1 > CurrentMediaPlaybackList.Count || index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        CurrentMediaPlaybackList[index] = item;
    }

    /// <summary>
    /// 开始播放音乐。
    /// </summary>
    public static void PlayMusic()
    {
        switch (mediaPlayer.PlaybackSession.PlaybackState)
        {
            case MediaPlaybackState.None:
            case MediaPlaybackState.Paused:
            case MediaPlaybackState.Opening:
                mediaPlayer.Play();
                break;
            case MediaPlaybackState.Playing:
            default:
                break;
        }
    }
    
    /// <summary>
    /// 暂停播放音乐。
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
    /// 终止音乐播放。
    /// </summary>
    public static async Task StopMusic()
    {
        mediaPlayer.Pause();
        mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;

        await UIThreadHelper.RunOnUIThread(() =>
        {
            MusicStopping?.Invoke();
            CurrentMediaPlaybackList.Clear();
            MusicStopped?.Invoke();
        });
    }
}
