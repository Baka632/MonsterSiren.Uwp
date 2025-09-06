using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="NowPlayingPage"/> 提供视图模型
/// </summary>
public partial class NowPlayingViewModel : ObservableObject
{
    private readonly NowPlayingPage view;

    [ObservableProperty]
    private string nowPlayingListExpandButtonGlyph = "\uE010";
    [ObservableProperty]
    private bool isMediaCasting = MediaCastService.IsMediaCasting;
    [ObservableProperty]
    private string mediaCastingButtonString = "CastToDevice".GetLocalized();

    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;

    public NowPlayingViewModel(NowPlayingPage nowPlaying)
    {
        view = nowPlaying;
        DetermineMediaCastingButtonString(MediaCastService.IsMediaCasting);
        MediaCastService.MediaCastingStateChanged += OnMediaCastServiceMediaCastingStateChanged;
    }

    ~NowPlayingViewModel()
    {
        MediaCastService.MediaCastingStateChanged -= OnMediaCastServiceMediaCastingStateChanged;
    }

    private void OnMediaCastServiceMediaCastingStateChanged(bool isCasting)
    {
        IsMediaCasting = isCasting;
        DetermineMediaCastingButtonString(isCasting);
    }

    private void DetermineMediaCastingButtonString(bool isCasting)
    {
        if (isCasting)
        {
            MediaCastingButtonString = "OpenCastToPanel".GetLocalized();
        }
        else
        {
            MediaCastingButtonString = "CastToDevice".GetLocalized();
        }
    }

    [RelayCommand]
    private void ShowCastToPicker()
    {
        Button target = view.MoreOperationButton;

        GeneralTransform transform = target.TransformToVisual(Window.Current.Content);
        Point pt = transform.TransformPoint(new Point(0, 0));
        Rect selection = new(pt.X, pt.Y, target.ActualWidth, target.ActualHeight);

        MediaCastService.ShowCastingDevicePicker(selection);
    }

    [RelayCommand]
    private static void StopCasting()
    {
        MediaCastService.StopCasting();
    }

    /// <summary>
    /// 使用指定的 <see cref="TimeSpan"/> 更新音乐播放位置的相关属性
    /// </summary>
    /// <param name="timeSpan">指定的 <see cref="TimeSpan"/></param>
    public void UpdateMusicPosition(TimeSpan timeSpan)
    {
        MusicInfo.MusicPosition = MusicService.PlayerPosition = timeSpan;
    }

    [RelayCommand]
    private static async Task ToCompactNowPlayingPage()
    {
        SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
        navigationManager.BackRequested -= MainPage.BackRequested;
        navigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

        ViewModePreferences preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
        preferences.CustomSize = new Size(300, 300);
        await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);

        MainPageNavigationHelper.Navigate(typeof(NowPlayingCompactPage), null, new SuppressNavigationTransitionInfo());
    }

    [RelayCommand]
    private static void ToGlanceViewPage()
    {
        MainPageNavigationHelper.Navigate(typeof(GlanceViewPage), null, new SuppressNavigationTransitionInfo());
    }

    [RelayCommand]
    private void MoveToIndex(MediaPlaybackItem playbackItem)
    {
        int index = view.NowPlayingListView.Items.IndexOf(playbackItem);

        MusicService.MoveTo((uint)index);
        MusicService.PlayMusic();
    }
    
    [RelayCommand]
    private void RemoveAt(MediaPlaybackItem playbackItem)
    {
        int index = view.NowPlayingListView.Items.IndexOf(playbackItem);

        MusicService.RemoveAt(index);
    }
}