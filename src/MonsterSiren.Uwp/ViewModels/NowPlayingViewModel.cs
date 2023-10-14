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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisplayContent))]
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
    private bool isMusicBufferingOrOpening;
    [ObservableProperty]
    private string repeatStateDescription = "RepeatOffText".GetLocalized();
    [ObservableProperty]
    private string shuffleStateDescription = "ShuffleOffText".GetLocalized();
    [ObservableProperty]
    private Color musicThemeColor;

    public bool IsDisplayContent
    {
        get => CurrentMusicProperties is not null;
    }

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

    public bool? IsMute
    {
        get => MusicService.IsPlayerMuted;
        set
        {
            MusicService.IsPlayerMuted = value.Value;
            OnPropertyChanged();
        }
    }

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

    public bool? IsShuffle
    {
        get => MusicService.IsPlayerShuffleEnabled;
        set
        {
            MusicService.IsPlayerShuffleEnabled = value.Value;
            OnPropertyChanged();
        }
    }

    public NowPlayingViewModel()
    {
        //HACK: Modify it by settings
        MusicService.PlayerVolume = 1d;

        MusicService.PlayerPlayItemChanged += OnPlayerPlayItemChanged;
        MusicService.PlayerVolumeChanged += OnPlayerVolumeChanged;
        MusicService.PlayerMuteStateChanged += OnPlayerMuteStateChanged;
        MusicService.PlayerPlaybackStateChanged += OnPlayerPlaybackStateChanged;
        MusicService.MusicDurationChanged += OnEventMusicDurationChanged;
        MusicService.PlayerPositionChanged += OnPlayerPositionChanged;
        MusicService.PlayerShuffleStateChanged += OnPlayerShuffleStateChanged;
        MusicService.PlayerRepeatingStateChanged += OnPlayerRepeatingStateChanged;

        MusicThemeColor = (Color)Application.Current.Resources["SystemAccentColorDark2"];
    }

    private void OnPlayerRepeatingStateChanged(PlayerRepeatingState state)
    {
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
        OnPropertyChanged(nameof(IsMute));
    }

    private void OnPlayerVolumeChanged(double volume)
    {
        if (MusicService.IsPlayerMuted != true)
        {
            ChangeVolumeIconByVolume(volume);
        }
    }

    private void ChangeVolumeIconByVolume(double volume)
    {
        if (volume > 0.6)
        {
            VolumeIconGlyph = "\uE995";
        }
        else if (volume > 0.3 && volume < 0.6)
        {
            VolumeIconGlyph = "\uE994";
        }
        else if (volume > 0 && volume < 0.3)
        {
            VolumeIconGlyph = "\uE993";
        }
        else if (volume == 0)
        {
            VolumeIconGlyph = "\uE992";
        }
    }

    private async void OnPlayerPlayItemChanged(CurrentMediaPlaybackItemChangedEventArgs args)
    {
        if (args.NewItem is not null)
        {
            MediaItemDisplayProperties props = args.NewItem.GetDisplayProperties();

            CurrentMusicProperties = props.MusicProperties;

            if (CacheHelper<AlbumDetail>.Default.TryQueryData(val => val.Name == props.MusicProperties.AlbumTitle, out IEnumerable<AlbumDetail> details))
            {
                AlbumDetail albumDetail = details.First();

                Uri uri = new(albumDetail.CoverUrl, UriKind.Absolute);
                CurrentMediaCover = new BitmapImage(uri);

                MusicThemeColor = await ImageColorHelper.GetPaletteColor(uri);
            }
            else if (props.Thumbnail is not null)
            {
                IRandomAccessStreamWithContentType stream = await props.Thumbnail.OpenReadAsync();
                await CurrentMediaCover.SetSourceAsync(stream);

                MusicThemeColor = await ImageColorHelper.GetPaletteColor(stream);
            }
        }
        else
        {
            CurrentMusicProperties = null;
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

    /// <summary>
    /// 使用指定的 <see cref="TimeSpan"/> 更新音乐播放位置的相关属性
    /// </summary>
    /// <param name="timeSpan">指定的 <see cref="TimeSpan"/></param>
    public void UpdateMusicPosition(TimeSpan timeSpan)
    {
        MusicPosition = MusicService.PlayerPosition = timeSpan;
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
}