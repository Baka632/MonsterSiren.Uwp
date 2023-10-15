using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.Services;

public sealed partial class MusicInfoService : ObservableRecipient
{
    /// <summary>
    /// 获取 <see cref="MusicInfoService"/> 的默认实例
    /// </summary>
    public static readonly MusicInfoService Default = new();

    private MusicDisplayProperties formerMusicDisplayProperties;

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
    private Color musicThemeColor;

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

        InitializeFromSettings();
        MusicThemeColor = (Color)Application.Current.Resources["SystemAccentColorDark2"];
        IsActive = true;
    }

    private static void InitializeFromSettings()
    {
        {
            if (SettingsService.TryGet(CommonValues.MusicVolumeSettingsKey, out double volume))
            {
                MusicService.PlayerVolume = volume;
            }
            else
            {
                MusicService.PlayerVolume = 1d;
                SettingsService.Set(CommonValues.MusicVolumeSettingsKey, 1d);
            }
        }

        {
            if (SettingsService.TryGet(CommonValues.MusicMuteStateSettingsKey, out bool isMute))
            {
                MusicService.IsPlayerMuted = isMute;
            }
            else
            {
                MusicService.IsPlayerMuted = false;
                SettingsService.Set(CommonValues.MusicMuteStateSettingsKey, false);
            }
        }
        
        {
            if (SettingsService.TryGet(CommonValues.MusicShuffleStateSettingsKey, out bool isShuffle))
            {
                MusicService.IsPlayerShuffleEnabled = isShuffle;
            }
            else
            {
                MusicService.IsPlayerShuffleEnabled = false;
                SettingsService.Set(CommonValues.MusicShuffleStateSettingsKey, false);
            }
        }

        {
            if (SettingsService.TryGet(CommonValues.MusicRepeatStateSettingsKey, out string enumString)
                && Enum.TryParse(enumString, out PlayerRepeatingState result))
            {
                MusicService.PlayerRepeatingState = result;
            }
            else
            {
                MusicService.PlayerRepeatingState = PlayerRepeatingState.None;
                SettingsService.Set(CommonValues.MusicRepeatStateSettingsKey, PlayerRepeatingState.None.ToString());
            }
        }
    }

    private void OnPlayerMediaReplacing()
    {
        IsLoadingMedia = true;
    }

    private void OnPlayerRepeatingStateChanged(PlayerRepeatingState state)
    {
        SettingsService.Set(CommonValues.MusicRepeatStateSettingsKey, state.ToString());
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
    }

    private void OnPlayerShuffleStateChanged(bool value)
    {
        SettingsService.Set(CommonValues.MusicShuffleStateSettingsKey, value);
        OnPropertyChanged(nameof(IsShuffle));
        ShuffleStateDescription = value switch
        {
            true => "ShuffleOnText".GetLocalized(),
            false => "ShuffleOffText".GetLocalized()
        };
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

        SettingsService.Set(CommonValues.MusicMuteStateSettingsKey, isMute);
        OnPropertyChanged(nameof(IsMute));
    }

    private void OnPlayerVolumeChanged(double volume)
    {
        if (MusicService.IsPlayerMuted != true)
        {
            ChangeVolumeIconByVolume(volume);
        }

        SettingsService.Set(CommonValues.MusicVolumeSettingsKey, volume);
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
        if (args.NewItem is not null)
        {
            IsLoadingMedia = true;

            MediaItemDisplayProperties props = args.NewItem.GetDisplayProperties();

            CurrentMusicProperties = props.MusicProperties;

            if (CacheHelper<AlbumDetail>.Default.TryQueryData(val => val.Name == props.MusicProperties.AlbumTitle, out IEnumerable<AlbumDetail> details))
            {
                AlbumDetail albumDetail = details.First();

                Uri uri = new(albumDetail.CoverUrl, UriKind.Absolute);
                CurrentMediaCover = new BitmapImage(uri);

                if (CacheHelper<Color>.Default.TryGetData(props.MusicProperties.AlbumTitle, out Color color))
                {
                    MusicThemeColor = color;
                }
                else
                {
                    MusicThemeColor = await ImageColorHelper.GetPaletteColor(uri);
                    CacheHelper<Color>.Default.Store(props.MusicProperties.AlbumTitle, MusicThemeColor);
                }
            }
            else if (props.Thumbnail is not null)
            {
                IRandomAccessStreamWithContentType stream = await props.Thumbnail.OpenReadAsync();
                await CurrentMediaCover.SetSourceAsync(stream);

                if (CacheHelper<Color>.Default.TryGetData(props.MusicProperties.AlbumTitle, out Color color))
                {
                    MusicThemeColor = color;
                }
                else
                {
                    MusicThemeColor = await ImageColorHelper.GetPaletteColor(stream);
                    CacheHelper<Color>.Default.Store(props.MusicProperties.AlbumTitle, MusicThemeColor);
                }
            }

            IsLoadingMedia = false;
        }
        else
        {
            CurrentMusicProperties = null;
        }
    }

    partial void OnIsLoadingMediaChanging(bool isMediaChanging)
    {
        if (isMediaChanging)
        {
            formerMusicDisplayProperties = CurrentMusicProperties;
            CurrentMusicProperties = null;
        }
    }

    [RelayCommand]
    private void PlayOrPauseMusic()
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
    private void StopMusic() => MusicService.StopMusic();

    [RelayCommand]
    private void NextMusic() => MusicService.NextMusic();

    [RelayCommand]
    private void PreviousMusic()
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
            IsLoadingMedia = false;
            CurrentMusicProperties = formerMusicDisplayProperties;
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
