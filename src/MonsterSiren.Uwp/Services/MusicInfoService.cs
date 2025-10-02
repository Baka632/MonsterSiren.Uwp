#define GIFTS_FOR_SPECIAL_OPERATORS

#if GIFTS_FOR_SPECIAL_OPERATORS
#define SONG_FOR_PEPE
#define SONG_FOR_HARUKA
#define SONG_FOR_EUREKA
#endif

using System.Collections.Specialized;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 应用程序音乐信息服务。
/// </summary>
public sealed partial class MusicInfoService : ObservableObject
{
    /// <summary>
    /// 获取 <see cref="MusicInfoService"/> 的默认实例。
    /// </summary>
    public static readonly MusicInfoService Default = new();

    private bool isPlaylistItemErrorDialogShowing;
    private readonly ThemeListener themeListener = new();
    private readonly Dictionary<MediaPlaybackItem, int> errorItemErrorCountPair = new(5);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MusicDuration))]
    [NotifyPropertyChangedFor(nameof(MusicPosition))]
    private MusicDisplayProperties currentMusicProperties;
    [ObservableProperty]
    private BitmapImage currentMediaCover;
    [ObservableProperty]
    private string volumeIconGlyph = "\uE995";
    [ObservableProperty]
    private string playIconGlyph = "\uE102";
    [ObservableProperty]
    private string repeatIconGlyph = "\uE8EE";
    [ObservableProperty]
    private TimeSpan musicDuration;
    [ObservableProperty]
    private TimeSpan musicPosition;
    [ObservableProperty]
    private bool isModifyingMusicPositionBySlider;
    [ObservableProperty]
    private bool isMusicBufferingOrOpening; // 是否正在下载/打开音乐
    [ObservableProperty]
    private bool isLoadingMedia; // 是否正在加载音乐信息
    [ObservableProperty]
    private string repeatStateDescription = "RepeatOffText".GetLocalized();
    [ObservableProperty]
    private string shuffleStateDescription = "ShuffleOffText".GetLocalized();
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MusicThemeColorLight1))]
    [NotifyPropertyChangedFor(nameof(MusicThemeColorLight2))]
    [NotifyPropertyChangedFor(nameof(MusicThemeColorLight3))]
    [NotifyPropertyChangedFor(nameof(MusicThemeColorDark1))]
    [NotifyPropertyChangedFor(nameof(MusicThemeColorDark2))]
    [NotifyPropertyChangedFor(nameof(MusicThemeColorDark3))]
    [NotifyPropertyChangedFor(nameof(MusicThemeColorThemeAware))]
    private Color musicThemeColor;
    [ObservableProperty]
    private bool hasMusic;
    [ObservableProperty]
    private bool showOrEnableMusicControl;

    public Color MusicThemeColorLight1 { get => MusicThemeColor.LighterBy(0.3f); }
    public Color MusicThemeColorLight2 { get => MusicThemeColor.LighterBy(0.6f); }
    public Color MusicThemeColorLight3 { get => MusicThemeColor.LighterBy(0.9f); }
    public Color MusicThemeColorDark1 { get => MusicThemeColor.DarkerBy(0.3f); }
    public Color MusicThemeColorDark2 { get => MusicThemeColor.DarkerBy(0.6f); }
    public Color MusicThemeColorDark3 { get => MusicThemeColor.DarkerBy(0.9f); }
    public Color MusicThemeColorThemeAware => themeListener.CurrentTheme switch
    {
        ApplicationTheme.Dark => MusicThemeColorLight1,
        _ => MusicThemeColorDark1
    };

    /// <summary>
    /// 获取或设置播放器音量。
    /// </summary>
    public double Volume
    {
        get => MusicService.PlayerVolume * 100;
        set
        {
            if (MusicService.IsPlayerMuted)
            {
                MusicService.IsPlayerMuted = false;
            }

            MusicService.PlayerVolume = value / 100;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 获取或设置播放器的静音状态。
    /// </summary>
    public bool? IsMute
    {
        get => MusicService.IsPlayerMuted;
        set
        {
            MusicService.IsPlayerMuted = value.Value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 获取或设置播放器的重复播放状态。
    /// </summary>
    public bool? IsRepeat
    {
        get
        {
            return MusicService.PlayerRepeatingState switch
            {
                PlayerRepeatingState.RepeatSingle => null,
                PlayerRepeatingState.RepeatAll => true,
                _ => false,
            };
        }
        set
        {
            MusicService.PlayerRepeatingState = value switch
            {
                true => PlayerRepeatingState.RepeatAll,
                false => PlayerRepeatingState.None,
                null => PlayerRepeatingState.RepeatSingle,
            };
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 获取或设置播放器的随机播放状态。
    /// </summary>
    public bool? IsShuffle
    {
        get => MusicService.IsPlayerShuffleEnabled;
        set
        {
            MusicService.IsPlayerShuffleEnabled = value.Value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 构造 <see cref="MusicInfoService"/> 的新实例。
    /// </summary>
    public MusicInfoService()
    {
        MusicService.PlayerPlayItemChanged += OnPlayerPlayItemChanged;
        MusicService.PlayerVolumeChanged += OnPlayerVolumeChanged;
        MusicService.PlayerMuteStateChanged += OnPlayerMuteStateChanged;
        MusicService.PlayerPlaybackStateChanged += OnPlayerPlaybackStateChanged;
        MusicService.MusicDurationChanged += OnEventMusicDurationChanged;
        MusicService.PlayerPositionChanged += OnPlayerPositionChanged;
        MusicService.PlayerShuffleStateChanged += OnPlayerShuffleStateChanged;
        MusicService.PlayerRepeatingStateChanged += OnPlayerRepeatingStateChanged;
        MusicService.PlayerMediaFailed += OnPlayerMediaFailed;
        MusicService.MusicPrepareModeChanged += OnMusicPrepareModeChanged;
        MusicService.MusicStopped += OnMusicStopped;
        MusicService.PlaylistChanged += OnPlaylistChanged;
        MusicService.PlaylistItemFailed += OnPlaylistItemFailed;
        MusicService.PlayerHasMusicStateChanged += OnPlayerHasMusicStateChanged;

        themeListener.ThemeChanged += OnThemeChanged;

        InitializeFromSettings();
        MusicThemeColor = (Color)Application.Current.Resources["SystemAccentColor"];
    }

    private static void InitializeFromSettings()
    {
        #region Volume
        {
            if (SettingsHelper.TryGet(CommonValues.MusicVolumeSettingsKey, out double volume))
            {
                MusicService.PlayerVolume = volume;
            }
            else
            {
                MusicService.PlayerVolume = 1d;
                SettingsHelper.Set(CommonValues.MusicVolumeSettingsKey, 1d);
            }
        }
        #endregion

        #region Mute State
        {
            if (SettingsHelper.TryGet(CommonValues.MusicMuteStateSettingsKey, out bool isMute))
            {
                MusicService.IsPlayerMuted = isMute;
            }
            else
            {
                MusicService.IsPlayerMuted = false;
                SettingsHelper.Set(CommonValues.MusicMuteStateSettingsKey, false);
            }
        }
        #endregion

        #region Shuffle State
        {
            if (SettingsHelper.TryGet(CommonValues.MusicShuffleStateSettingsKey, out bool isShuffle))
            {
                MusicService.IsPlayerShuffleEnabled = isShuffle;
            }
            else
            {
                MusicService.IsPlayerShuffleEnabled = false;
                SettingsHelper.Set(CommonValues.MusicShuffleStateSettingsKey, false);
            }
        }
        #endregion

        #region Repeat State
        {
            if (SettingsHelper.TryGet(CommonValues.MusicRepeatStateSettingsKey, out string enumString)
                && Enum.TryParse(enumString, out PlayerRepeatingState result))
            {
                MusicService.PlayerRepeatingState = result;
            }
            else
            {
                MusicService.PlayerRepeatingState = PlayerRepeatingState.None;
                SettingsHelper.Set(CommonValues.MusicRepeatStateSettingsKey, PlayerRepeatingState.None.ToString());
            }
        }
        #endregion
    }

    private async void OnPlayerMediaFailed(MediaPlayerFailedEventArgs args)
    {
        MessageDialog dialog = new(args.ErrorMessage, "PlayerError_Title".GetLocalized());
        await dialog.ShowAsync();
    }

    private async void OnPlaylistItemFailed(MediaPlaybackItemFailedEventArgs args)
    {
        MediaPlaybackItem item = args.Item;
        MediaItemDisplayProperties props = item?.GetDisplayProperties();

        if (props is not null)
        {
            if (errorItemErrorCountPair.TryGetValue(item, out int errorCount))
            {
                errorItemErrorCountPair[item]++;
            }
            else
            {
                errorItemErrorCountPair[item] = errorCount = 1;
            }

            if (errorCount <= 3
                && MemoryCacheHelper<SongDetail>.Default.TryQueryData(detail => detail.Name == props.MusicProperties.Title, out IEnumerable<SongDetail> details)
                && details.Any())
            {
                try
                {
                    SongDetail oldSongDetail = details.Single();
                    MediaPlaybackItem newPlaybackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(oldSongDetail.Cid, true);

                    MusicService.ReplaceAt(item, newPlaybackItem);
                    System.Diagnostics.Debug.WriteLine($"已替换：{oldSongDetail.Name}");
                    return;
                }
                catch
                {
                    // Do nothing ;-)
                }
            }
        }

        EnsurePlayRelatedPropertyIsCorrect();

        if (isPlaylistItemErrorDialogShowing)
        {
            return;
        }

        isPlaylistItemErrorDialogShowing = true;
        string musicTitle = props?.MusicProperties?.Title;
        string errorInfo = $"{args.Error.ErrorCode}\n{args.Error.ExtendedError.Message}";

        string message = string.Format("PlaylistItemFailed_Message".GetLocalized(), musicTitle, errorInfo);

        MessageDialog dialog = new(message, "PlaylistItemFailed_Title".GetLocalized());
        await dialog.ShowAsync();

        isPlaylistItemErrorDialogShowing = false;
    }

    private void OnThemeChanged(ThemeListener sender)
    {
        OnPropertyChanged(nameof(MusicThemeColorThemeAware));
    }

    private async void OnPlaylistChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (isUpdatingTile != true && e.Action != NotifyCollectionChangedAction.Reset)
        {
            await CreateNowPlayingTile();
        }
    }

    private void OnMusicPrepareModeChanged()
    {
        if (MusicService.IsMusicPreparing)
        {
            IsLoadingMedia = true;
            ShowOrEnableMusicControl = false;
        }
    }

    /// <summary>
    /// 确保 <see cref="IsLoadingMedia"/>、<see cref="ShowOrEnableMusicControl"/> 这些与指示播放状态相关的属性的值正确。
    /// </summary>
    public void EnsurePlayRelatedPropertyIsCorrect()
    {
        bool isMusicPreparing = MusicService.IsMusicPreparing;
        IsLoadingMedia = isMusicPreparing;

        if (MusicService.IsPlayerPlaylistHasMusic)
        {
            ShowOrEnableMusicControl = !isMusicPreparing;
        }
        else
        {
            ShowOrEnableMusicControl = false;
        }
    }

    private void OnPlayerHasMusicStateChanged()
    {
        bool isPlayerPlaylistHasMusic = MusicService.IsPlayerPlaylistHasMusic;
        HasMusic = isPlayerPlaylistHasMusic;

        if (!isPlayerPlaylistHasMusic)
        {
            ShowOrEnableMusicControl = false;
        }
    }

    private void OnMusicStopped()
    {
        if (CurrentMediaCover != null)
        {
            CurrentMediaCover = null;
        }
    }

    private async void OnPlayerRepeatingStateChanged(PlayerRepeatingState state)
    {
        SettingsHelper.Set(CommonValues.MusicRepeatStateSettingsKey, state.ToString());
        OnPropertyChanged(nameof(IsRepeat));
        RepeatIconGlyph = state switch
        {
            PlayerRepeatingState.RepeatSingle => "\uE8ED",
            _ => "\uE8EE",
        };

        RepeatStateDescription = state switch
        {
            PlayerRepeatingState.RepeatAll => "RepeatAllText".GetLocalized(),
            PlayerRepeatingState.RepeatSingle => "RepeatSingleText".GetLocalized(),
            _ => "RepeatOffText".GetLocalized(),
        };

        await CreateNowPlayingTile();
    }

    private async void OnPlayerShuffleStateChanged(bool value)
    {
        SettingsHelper.Set(CommonValues.MusicShuffleStateSettingsKey, value);
        OnPropertyChanged(nameof(IsShuffle));
        ShuffleStateDescription = value switch
        {
            true => "ShuffleOnText".GetLocalized(),
            false => "ShuffleOffText".GetLocalized()
        };

        await CreateNowPlayingTile();
    }

    private void OnPlayerPositionChanged(TimeSpan span)
    {
        if (IsModifyingMusicPositionBySlider != true)
        {
            MusicPosition = span;
        }
    }

    private void OnEventMusicDurationChanged(TimeSpan span)
    {
        MusicDuration = span;
    }

    private void OnPlayerPlaybackStateChanged(MediaPlaybackState state)
    {
        switch (state)
        {
            case MediaPlaybackState.Playing:
                PlayIconGlyph = "\uE103";
                IsMusicBufferingOrOpening = false;
                break;
            case MediaPlaybackState.Paused:
                PlayIconGlyph = "\uE102";
                IsMusicBufferingOrOpening = false;
                break;
            case MediaPlaybackState.Opening or MediaPlaybackState.Buffering:
                PlayIconGlyph = "\uE118";
                IsMusicBufferingOrOpening = true;
                break;
        }
    }

    private void OnPlayerMuteStateChanged(bool isMute)
    {
        if (isMute)
        {
            VolumeIconGlyph = "\uE198";
        }
        else
        {
            ChangeVolumeIconByVolume(MusicService.PlayerVolume);
        }

        SettingsHelper.Set(CommonValues.MusicMuteStateSettingsKey, isMute);
        OnPropertyChanged(nameof(IsMute));
    }

    private void OnPlayerVolumeChanged(double volume)
    {
        if (MusicService.IsPlayerMuted != true)
        {
            ChangeVolumeIconByVolume(volume);
        }

        SettingsHelper.Set(CommonValues.MusicVolumeSettingsKey, volume);
    }

    private void ChangeVolumeIconByVolume(double volume)
    {
        switch (volume)
        {
            case > 0.6:
                VolumeIconGlyph = "\uE995";
                break;
            case > 0.3 and < 0.6:
                VolumeIconGlyph = "\uE994";
                break;
            case > 0 and < 0.3:
                VolumeIconGlyph = "\uE993";
                break;
            case 0:
                VolumeIconGlyph = "\uE992";
                break;
        }
    }

    private async void OnPlayerPlayItemChanged(CurrentMediaPlaybackItemChangedEventArgs args)
    {
        MediaPlaybackItem newItem = args.NewItem;

        if (newItem is not null)
        {
            _ = errorItemErrorCountPair.Remove(newItem);

            MediaItemDisplayProperties props = newItem.GetDisplayProperties();

            CurrentMusicProperties = props.MusicProperties;

#if GIFTS_FOR_SPECIAL_OPERATORS
            GiftForSpecialOperatorsSelector.TrySelectGift(props);
#endif

            if (MemoryCacheHelper<AlbumDetail>.Default.TryQueryData(val => val.Name == props.MusicProperties.AlbumTitle, out IEnumerable<AlbumDetail> details))
            {
                AlbumDetail albumDetail = details.First();

                Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumDetail);
                if (fileCoverUri == null)
                {
                    try
                    {
                        fileCoverUri = await FileCacheHelper.StoreAlbumCoverAsync(albumDetail);
                    }
                    catch
                    {
                        fileCoverUri = new(albumDetail.CoverUrl, UriKind.Absolute);
                    }
                }

                if (CurrentMediaCover?.UriSource == fileCoverUri)
                {
                    return;
                }

                CurrentMediaCover = new BitmapImage(fileCoverUri)
                {
                    DecodePixelHeight = 250,
                    DecodePixelWidth = 250,
                    DecodePixelType = DecodePixelType.Logical,
                };

                if (MemoryCacheHelper<Color>.Default.TryGetData(props.MusicProperties.AlbumTitle, out Color color))
                {
                    MusicThemeColor = color;
                }
                else
                {
                    MusicThemeColor = await ImageColorHelper.GetPaletteColor(fileCoverUri);
                    MemoryCacheHelper<Color>.Default.Store(props.MusicProperties.AlbumTitle, MusicThemeColor);
                }
            }
            else if (props.Thumbnail is not null)
            {
                IRandomAccessStreamWithContentType stream = await props.Thumbnail.OpenReadAsync();
                CurrentMediaCover = new BitmapImage()
                {
                    DecodePixelHeight = 250,
                    DecodePixelWidth = 250,
                    DecodePixelType = DecodePixelType.Logical,
                };
                await CurrentMediaCover.SetSourceAsync(stream);

                if (MemoryCacheHelper<Color>.Default.TryGetData(props.MusicProperties.AlbumTitle, out Color color))
                {
                    MusicThemeColor = color;
                }
                else
                {
                    MusicThemeColor = await ImageColorHelper.GetPaletteColor(stream);
                    MemoryCacheHelper<Color>.Default.Store(props.MusicProperties.AlbumTitle, MusicThemeColor);
                }
            }

            ShowOrEnableMusicControl = true;
            IsLoadingMedia = false;
            await CreateNowPlayingTile();
        }
        else
        {
            CurrentMusicProperties = null;
            DeleteNowPlayingTile();
        }
    }

    [RelayCommand]
    private static void PlayOrPauseMusic()
    {
        switch (MusicService.PlayerPlayBackState)
        {
            case MediaPlaybackState.Playing:
                MusicService.PauseMusic();
                break;
            case MediaPlaybackState.None:
            case MediaPlaybackState.Paused:
                MusicService.PlayMusic();
                break;
            case MediaPlaybackState.Opening:
            case MediaPlaybackState.Buffering:
            default:
                return;
        }
    }

    [RelayCommand]
    private static async Task StopMusic()
    {
        await MusicService.StopMusic();
    }

    [RelayCommand]
    private static void NextMusic() => MusicService.NextMusic();

    [RelayCommand]
    private static void PreviousMusic()
    {
        if (MusicService.PlayerPosition.TotalSeconds > 5)
        {
            MusicService.PlayerPosition = TimeSpan.Zero;
        }
        else
        {
            MusicService.PreviousMusic();
        }
    }

#if GIFTS_FOR_SPECIAL_OPERATORS
    /// <summary>
    /// 礼物选择器。
    /// </summary>
    /// <remarks>
    /// Special designed by Baka632.
    /// </remarks>
    private static class GiftForSpecialOperatorsSelector
    {
        private static readonly IReadOnlyList<Gift> gifts = [
#if SONG_FOR_PEPE
            new GiftForPepe(),
#endif
#if SONG_FOR_HARUKA
            new GiftForHaruka(),
#endif
#if SONG_FOR_EUREKA
            new GiftForEureka(),
#endif
        ];

        /// <summary>
        /// 尝试为干员选择符合条件的礼物。
        /// </summary>
        /// <param name="props">当前播放器的媒体播放信息。</param>
        internal static void TrySelectGift(MediaItemDisplayProperties props)
        {
            foreach (Gift gift in gifts)
            {
                gift.TryOpen(props);
            }
        }

        /// <summary>
        /// 表示给干员的一份礼物。
        /// </summary>
        /// <remarks>
        /// Special designed by Baka632.
        /// </remarks>
        private abstract class Gift
        {
            /// <summary>
            /// 对触发条件的计数。
            /// </summary>
            public int HitCount { get; private set; }

            /// <summary>
            /// 尝试打开这份礼物。
            /// </summary>
            /// <param name="props">当前正在播放歌曲的信息。</param>
            public abstract void TryOpen(MediaItemDisplayProperties props);

            /// <summary>
            /// 根据指定的委托，添加或重置计数。
            /// </summary>
            /// <param name="predicate">指定的一个返回 <see langword="bool"/> 的委托。</param>
            /// <exception cref="ArgumentNullException"><paramref name="predicate"/> 为 <see langword="null"/></exception>
            protected void AddCountOrReset(Func<bool> predicate)
            {
                if (predicate is null)
                {
                    throw new ArgumentNullException(nameof(predicate));
                }

                if (predicate())
                {
                    HitCount++;
                }
                else
                {
                    HitCount = 0;
                }
            }

            /// <summary>
            /// 发送 <see cref="ToastContent"/>。
            /// </summary>
            /// <param name="toastContent">指定的 <see cref="ToastContent"/>。</param>
            protected static void SendToast(ToastContent toastContent)
            {
                if (toastContent is not null)
                {
                    Windows.UI.Notifications.ToastNotification toastNotif = new(toastContent.GetXml());
                    Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                }
            }
        }

#if SONG_FOR_PEPE
        /// <summary>
        /// 属于佩佩的类！
        /// </summary>
        private class GiftForPepe : Gift
        {
            private const string SongName = "Mystic Light Quest";

            private static readonly ToastContent pepe5Toast = new()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = "佩佩佩佩佩佩佩佩！" },
                            new AdaptiveText() { Text = "循环播放《Mystic Light Quest》达到 5 次！" }
                        }
                    }
                }
            };

            private static readonly ToastContent pepe10Toast = new()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = EnvironmentHelper.IsSystemBuildVersionEqualOrGreaterThan(18362)
                                    ? "已进入年代👉希望年代·扩张期🥰"
                                    : "已进入年代👉希望年代·扩张期💖"
                            },
                            new AdaptiveText()
                            {
                                Text = "在这个年代，美好茁壮成长，二次元从不消逝，溜佩佩 EP 是无害的病症，猫冰满足了一切需求😋"
                            }
                        },
                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = "来自 B 站 PV 评论区"
                        }
                    }
                }
            };

            public override void TryOpen(MediaItemDisplayProperties props)
            {
                AddCountOrReset(() => props.MusicProperties.Title.Contains(SongName));
                if (HitCount == 0)
                {
                    return;
                }

                ToastContent toastContent = null;
                if (HitCount == 5)
                {
                    toastContent = pepe5Toast;
                }
                else if (HitCount == 10)
                {
                    toastContent = pepe10Toast;
                }

                SendToast(toastContent);
            }
        }
#endif

#if SONG_FOR_HARUKA
        /// <summary>
        /// 属于萌萌香的类！
        /// </summary>
        private class GiftForHaruka : Gift
        {
            private const string SongName = "Little Wish";

            private static readonly ToastContent harukaToast = new()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = "Tell Me👏 Tell Me👏" },
                            new AdaptiveText() { Text = "鏡👏よ鏡👏" },
                            new AdaptiveText() { Text = "一番👏好きな👏私👏👏になるの👏👏" }
                        },
                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = "循环播放《Little Wish》达到 5 次！"
                        }
                    }
                }
            };

            public override void TryOpen(MediaItemDisplayProperties props)
            {
                AddCountOrReset(() => props.MusicProperties.Title.Contains(SongName));
                if (HitCount == 0)
                {
                    return;
                }

                if (HitCount == 5)
                {
                    SendToast(harukaToast);
                }
            }
        }
#endif

#if SONG_FOR_EUREKA
        /// <summary>
        /// 属于尤里卡的类！
        /// </summary>
        private class GiftForEureka : Gift
        {
            private readonly string[] songNames = ["FIVE超新星☆全能live时刻！", "冉冉升起，直播新星"];
            private readonly ToastContent eurekaToast = new()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = "尤里卡~尤里卡~" },
                            new AdaptiveText() { Text = "原来你也是尤里卡厨！" }
                        },
                        Attribution = new ToastGenericAttributionText() { Text = "来自 Baka632 的消息" }
                    }
                }
            };

            public override void TryOpen(MediaItemDisplayProperties props)
            {
                AddCountOrReset(() => songNames.Any(name => name == props.MusicProperties.Title));
                if (HitCount == 0)
                {
                    return;
                }

                if (HitCount == 20)
                {
                    SendToast(eurekaToast);
                }
            }
        }
#else
#warning Do not disable Eureka! Otherwise you will get unlucky!
#error Eureka is my beloved, do not leave her alone! ——Baka632
#endif
    }
#endif
}
