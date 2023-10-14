using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="NowPlayingCompactPage"/> 提供视图模型
/// </summary>
public partial class NowPlayingCompactViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MusicDuration))]
    [NotifyPropertyChangedFor(nameof(MusicPosition))]
    [NotifyPropertyChangedFor(nameof(IsDisplayContent))]
    private MusicDisplayProperties currentMusicProperties;
    [ObservableProperty]
    private BitmapImage currentMediaCover = new()
    {
        DecodePixelHeight = 250,
        DecodePixelWidth = 250,
        DecodePixelType = DecodePixelType.Logical,
    };
    [ObservableProperty]
    private string playIconGlyph = "\uE102";
    [ObservableProperty]
    private bool isMusicBufferingOrOpening;
    [ObservableProperty]
    private TimeSpan musicDuration;
    [ObservableProperty]
    private TimeSpan musicPosition;

    public bool IsDisplayContent
    {
        get => CurrentMusicProperties is not null;
    }

    public NowPlayingCompactViewModel()
    {
        if (MusicService.CurrentMediaPlaybackItem is not null)
        {
            MediaItemDisplayProperties props = MusicService.CurrentMediaPlaybackItem.GetDisplayProperties();
            CurrentMusicProperties = props.MusicProperties;

            if (CacheHelper<AlbumDetail>.Default.TryQueryData(val => val.Name == props.MusicProperties.AlbumTitle, out IEnumerable<AlbumDetail> details))
            {
                AlbumDetail albumDetail = details.First();

                Uri uri = new(albumDetail.CoverUrl, UriKind.Absolute);
                CurrentMediaCover = new BitmapImage(uri);
            }
            else if (props.Thumbnail is not null)
            {
                SetCoverByThumbnail(props);
            }
        }

        //刷新当前视图模型的状态
        OnPlayerPlaybackStateChanged(MusicService.PlayerPlayBackState);
        OnEventMusicDurationChanged(MusicService.MusicDuration);
        OnPlayerPositionChanged(MusicService.PlayerPosition);

        MusicService.PlayerPlayItemChanged += OnPlayerPlayItemChanged;
        MusicService.PlayerPlaybackStateChanged += OnPlayerPlaybackStateChanged;
        MusicService.MusicDurationChanged += OnEventMusicDurationChanged;
        MusicService.PlayerPositionChanged += OnPlayerPositionChanged;

        async void SetCoverByThumbnail(MediaItemDisplayProperties props)
        {
            IRandomAccessStreamWithContentType stream = await props.Thumbnail.OpenReadAsync();
            await CurrentMediaCover.SetSourceAsync(stream);
        }
    }

    private void OnPlayerPositionChanged(TimeSpan span)
    {
        MusicPosition = span;
    }

    private void OnEventMusicDurationChanged(TimeSpan span)
    {
        MusicDuration = span;
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
            }
            else if (props.Thumbnail is not null)
            {
                IRandomAccessStreamWithContentType stream = await props.Thumbnail.OpenReadAsync();
                await CurrentMediaCover.SetSourceAsync(stream);
            }
        }
        else
        {
            CurrentMusicProperties = null;
        }
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

    [RelayCommand]
    private async Task Back()
    {
        MusicService.PlayerPlayItemChanged -= OnPlayerPlayItemChanged;
        MusicService.PlayerPlaybackStateChanged -= OnPlayerPlaybackStateChanged;
        MusicService.MusicDurationChanged -= OnEventMusicDurationChanged;
        MusicService.PlayerPositionChanged -= OnPlayerPositionChanged;

        await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

        MainPageNavigationHelper.GoBack();
    }
}