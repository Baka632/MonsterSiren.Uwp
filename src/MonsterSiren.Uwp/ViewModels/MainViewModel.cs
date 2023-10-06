using Microsoft.UI.Xaml.Controls;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MainPage"/> 提供视图模型
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMedia))]
    private MusicDisplayProperties currentMusicProperties;
    [ObservableProperty]
    private BitmapSource currentMediaCover = new BitmapImage();

    public bool HasMedia => CurrentMusicProperties != null;

    public MainViewModel()
    {
        MusicService.PlayerPlayItemChanged += OnPlayerPlayItemChanged;
    }

    private async void OnPlayerPlayItemChanged(CurrentMediaPlaybackItemChangedEventArgs args)
    {
        MediaItemDisplayProperties props = args.NewItem.GetDisplayProperties();
        IRandomAccessStreamWithContentType stream = await props.Thumbnail.OpenReadAsync();

        await MainThreadHelper.RunOnMainThread(async () =>
        {
            CurrentMusicProperties = props.MusicProperties;
            await CurrentMediaCover.SetSourceAsync(stream);
        });
    }

    [RelayCommand]
    private void PlayOrPauseMusic()
    {
        if (MusicService.PlayerPlayBackState == MediaPlaybackState.Playing)
        {
            MusicService.PauseMusic();
        }
        else
        {
            MusicService.PlayMusic();
        }
    }

    [RelayCommand]
    private void StopMusic() => MusicService.StopMusic();
    [RelayCommand]
    private void NextMusic() => MusicService.NextMusic();
    [RelayCommand]
    private void PreviousMusic() => MusicService.PreviousMusic();

    #region InfoBar
    [ObservableProperty]
    private bool _InfoBarOpen;
    [ObservableProperty]
    private string _InfoBarTitle = string.Empty;
    [ObservableProperty]
    private string _InfoBarMessage = string.Empty;
    [ObservableProperty]
    private InfoBarSeverity _InfoBarSeverity;
    private void SetInfoBar(string title, string message, InfoBarSeverity severity)
    {
        InfoBarTitle = title;
        InfoBarMessage = message;
        InfoBarSeverity = severity;
        InfoBarOpen = true;
    }
    #endregion
}