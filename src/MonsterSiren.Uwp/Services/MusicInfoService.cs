#define SONG_FOR_PEPE

using System.Collections.Specialized;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 应用程序音乐信息服务
/// </summary>
public sealed partial class MusicInfoService : ObservableRecipient
{
    /// <summary>
    /// 获取 <see cref="MusicInfoService"/> 的默认实例
    /// </summary>
    public static readonly MusicInfoService Default = new();

    private MusicDisplayProperties formerMusicDisplayProperties;
    private readonly ThemeListener themeListener = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentMusicPropertiesExists))]
    [NotifyPropertyChangedFor(nameof(MusicDuration))]
    [NotifyPropertyChangedFor(nameof(MusicPosition))]
    private MusicDisplayProperties currentMusicProperties;
    [ObservableProperty]
    private BitmapImage currentMediaCover = new()
    {
        DecodePixelHeight = 250,
        DecodePixelWidth = 250,
        DecodePixelType = DecodePixelType.Logical,
    };
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
    /// 获取或设置播放器音量
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
    /// 获取或设置播放器的静音状态
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
    /// 获取或设置播放器的重复播放状态
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
    /// 获取或设置播放器的随机播放状态
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
    /// 确定当前的音乐属性是否存在的值
    /// </summary>
    public bool CurrentMusicPropertiesExists => CurrentMusicProperties is not null;

    /// <summary>
    /// 构造 <see cref="MusicInfoService"/> 的新实例
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
        MusicService.PlayerMediaReplacing += OnPlayerMediaReplacing;
        MusicService.MusicStopped += OnMusicStopped;
        MusicService.PlaylistChanged += OnPlayListChanged;

        themeListener.ThemeChanged += OnThemeChanged;

        InitializeFromSettings();
        MusicThemeColor = (Color)Application.Current.Resources["SystemAccentColor"];
        IsActive = true;
    }

    private void OnThemeChanged(ThemeListener sender)
    {
        OnPropertyChanged(nameof(MusicThemeColorThemeAware));
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

    private async void OnPlayListChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (CurrentMusicPropertiesExists && isUpdatingTile != true && e.Action != NotifyCollectionChangedAction.Reset)
        {
            await CreateNowPlayingTile();
        }
    }

    private void OnPlayerMediaReplacing()
    {
        IsLoadingMedia = true;
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

        if (CurrentMusicPropertiesExists)
        {
            await CreateNowPlayingTile();
        }
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

        if (CurrentMusicPropertiesExists)
        {
            await CreateNowPlayingTile();
        }
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

#if SONG_FOR_PEPE
    private int countForPepe = 0;
#endif

    private async void OnPlayerPlayItemChanged(CurrentMediaPlaybackItemChangedEventArgs args)
    {
        if (args.NewItem is not null)
        {
            IsLoadingMedia = true;

            MediaItemDisplayProperties props = args.NewItem.GetDisplayProperties();

            CurrentMusicProperties = props.MusicProperties;

#if SONG_FOR_PEPE
            if (props.MusicProperties.Title == "Mystic Light Quest")
            {
                countForPepe++;
            }
            else
            {
                countForPepe = 0;
            }

            if (countForPepe >= 5)
            {
                Microsoft.Toolkit.Uwp.Notifications.ToastContent toastContent = null;

                if (countForPepe == 5)
                {
                    toastContent = new()
                    {
                        Visual = new Microsoft.Toolkit.Uwp.Notifications.ToastVisual()
                        {
                            BindingGeneric = new Microsoft.Toolkit.Uwp.Notifications.ToastBindingGeneric()
                            {
                                Children =
                                {
                                    new Microsoft.Toolkit.Uwp.Notifications.AdaptiveText()
                                    {
                                        Text = "佩佩佩佩佩佩佩佩！"
                                    },
                                    new Microsoft.Toolkit.Uwp.Notifications.AdaptiveText()
                                    {
                                        Text = "循环播放《Mystic Light Quest》达到 5 次！"
                                    }
                                }
                            }
                        }
                    };
                }
                else if (countForPepe == 10)
                {
                    toastContent = new Microsoft.Toolkit.Uwp.Notifications.ToastContent()
                    {
                        Visual = new Microsoft.Toolkit.Uwp.Notifications.ToastVisual()
                        {
                            BindingGeneric = new Microsoft.Toolkit.Uwp.Notifications.ToastBindingGeneric()
                            {
                                Children =
                                {
                                    new Microsoft.Toolkit.Uwp.Notifications.AdaptiveText()
                                    {
                                        Text = EnvironmentHelper.IsSystemBuildVersionEqualOrGreaterThan(18362)
                                        ? "已进入年代👉希望年代·扩张期🥰"
                                        : "已进入年代👉希望年代·扩张期💖"
                                    },
                                    new Microsoft.Toolkit.Uwp.Notifications.AdaptiveText()
                                    {
                                        Text = "在这个年代，美好茁壮成长，二次元从不消逝，溜佩佩EP是无害的病症，猫冰满足了一切需求😋"
                                    }
                                },
                                Attribution = new Microsoft.Toolkit.Uwp.Notifications.ToastGenericAttributionText()
                                {
                                    Text = "来自 B 站 PV 评论区"
                                }
                            }
                        }
                    };
                }

                if (toastContent is not null)
                {
                    Windows.UI.Notifications.ToastNotification toastNotif = new(toastContent.GetXml());
                    Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
                }
            }
#endif

            if (MemoryCacheHelper<AlbumDetail>.Default.TryQueryData(val => val.Name == props.MusicProperties.AlbumTitle, out IEnumerable<AlbumDetail> details))
            {
                AlbumDetail albumDetail = details.First();

                Uri uri;

                Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumDetail);
                if (fileCoverUri != null)
                {
                    uri = fileCoverUri;
                }
                else
                {
                    uri = new(albumDetail.CoverUrl, UriKind.Absolute);
                    await FileCacheHelper.StoreAlbumCoverAsync(albumDetail);
                }

                CurrentMediaCover = new BitmapImage(uri)
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
                    MusicThemeColor = await ImageColorHelper.GetPaletteColor(uri);
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

            IsLoadingMedia = false;
            await CreateNowPlayingTile();
        }
        else
        {
            CurrentMusicProperties = null;
            DeleteNowPlayingTile();
        }
    }

    partial void OnIsLoadingMediaChanging(bool isMediaChanging)
    {
        if (isMediaChanging)
        {
            // TODO: 优化一下？
            formerMusicDisplayProperties = CurrentMusicProperties;
            CurrentMusicProperties = null;
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
    private static void StopMusic()
    {
        MusicService.StopMusic();
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

    protected override void OnActivated()
    {
        base.OnActivated();
        WeakReferenceMessenger.Default.Register<string, string>(this, CommonValues.NotifyWillUpdateMediaMessageToken, OnWillUpdateMedia);
        WeakReferenceMessenger.Default.Register<string, string>(this, CommonValues.NotifyUpdateMediaFailMessageToken, OnUpdateMediaFail);
    }

    private async void OnUpdateMediaFail(object recipient, string message)
    {
        MusicService.PlayMusic();
        await UIThreadHelper.RunOnUIThread(() =>
        {
            CurrentMusicProperties = formerMusicDisplayProperties;
            IsLoadingMedia = false;
        });
    }

    private async void OnWillUpdateMedia(object recipient, string message)
    {
        MusicService.PauseMusic();
        await UIThreadHelper.RunOnUIThread(() =>
        {
            IsLoadingMedia = true;
        });
    }
}
