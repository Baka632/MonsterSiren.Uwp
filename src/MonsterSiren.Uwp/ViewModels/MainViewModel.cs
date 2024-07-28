using System.ComponentModel;
using System.Net.Http;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MainPage"/> 提供视图模型
/// </summary>
public partial class MainViewModel : ObservableRecipient
{
    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;

    private readonly MainPage view;

    [ObservableProperty]
    private bool isMediaInfoVisible;
    [ObservableProperty]
    private IEnumerable<AlbumInfo> autoSuggestBoxSuggestion = [];
    [ObservableProperty]
    private Playlist selectedPlaylist;

    public MainViewModel(MainPage mainPage)
    {
        view = mainPage ?? throw new ArgumentNullException(nameof(mainPage));
        MusicInfo.PropertyChanged += OnMusicInfoPropertyChanged;

        IsActive = true;
    }

    private void OnMusicInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MusicInfo.CurrentMusicPropertiesExists))
        {
            IsMediaInfoVisible = MusicInfo.CurrentMusicPropertiesExists;
        }
    }

    /// <summary>
    /// 使用指定的 <see cref="TimeSpan"/> 更新音乐播放位置的相关属性
    /// </summary>
    /// <param name="timeSpan">指定的 <see cref="TimeSpan"/></param>
    public void UpdateMusicPosition(TimeSpan timeSpan)
    {
        MusicInfo.MusicPosition = MusicService.PlayerPosition = timeSpan;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        WeakReferenceMessenger.Default.Register<string, string>(this, CommonValues.NotifyAppBackgroundChangedMessageToken, OnAppBackgroundChanged);
    }

    private void OnAppBackgroundChanged(object recipient, string message)
    {
        if (Enum.TryParse(message, out AppBackgroundMode mode))
        {
            view.SetMainPageBackground(mode);
        }
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
        SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
        navigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        MainPageNavigationHelper.Navigate(typeof(GlanceViewPage), null, new SuppressNavigationTransitionInfo());
    }

    [RelayCommand]
    private static async Task PlayForPlaylist(Playlist playlist)
    {
        await PlaylistService.PlayForPlaylist(playlist);
    }

    public static async Task AddToPlaylistForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await GetAlbumDetail(albumInfo).ConfigureAwait(false);
                List<MediaPlaybackItem> playbackItems = new(albumDetail.Songs.Count());

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                    playbackItems.Add(songDetail.ToMediaPlaybackItem(albumDetail));
                }

                MusicService.AddMusic(playbackItems);
            });
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    public static async Task AddToPlaylistForSongInfo(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                MusicService.AddMusic(songDetail.ToMediaPlaybackItem(albumDetail));
            });
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    internal static async Task<AlbumDetail> GetAlbumDetail(AlbumInfo albumInfo)
    {
        AlbumDetail albumDetail;
        if (MemoryCacheHelper<AlbumDetail>.Default.TryGetData(albumInfo.Cid, out AlbumDetail detail))
        {
            albumDetail = detail;
        }
        else
        {
            albumDetail = await AlbumService.GetAlbumDetailedInfoAsync(albumInfo.Cid);

            bool shouldUpdate = false;
            foreach (SongInfo item in albumDetail.Songs)
            {
                if (item.Artists is null || item.Artists.Any() != true)
                {
                    shouldUpdate = true;
                    break;
                }
            }

            if (shouldUpdate)
            {
                List<SongInfo> songs = albumDetail.Songs.ToList();
                for (int i = 0; i < songs.Count; i++)
                {
                    SongInfo songInfo = songs[i];
                    if (songInfo.Artists is null || songInfo.Artists.Any() != true)
                    {
                        songs[i] = songInfo with { Artists = ["MSR".GetLocalized()] };
                    }
                }

                albumDetail = albumDetail with { Songs = songs };
            }

            if (albumDetail.Songs.Any())
            {
                MemoryCacheHelper<AlbumDetail>.Default.Store(albumInfo.Cid, albumDetail);
            }
        }

        return albumDetail;
    }

    internal static async Task<SongDetail> GetSongDetail(SongInfo songInfo)
    {
        if (MemoryCacheHelper<SongDetail>.Default.TryGetData(songInfo.Cid, out SongDetail detail))
        {
            return detail;
        }
        else
        {
            SongDetail songDetail = await SongService.GetSongDetailedInfoAsync(songInfo.Cid);
            MemoryCacheHelper<SongDetail>.Default.Store(songInfo.Cid, songDetail);

            return songDetail;
        }
    }

    public async Task UpdateAutoSuggestBoxSuggestion(string keyword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                AutoSuggestBoxSuggestion = null;
                return;
            }

            ListPackage<AlbumInfo> searchedAlbums = await SearchService.SearchAlbumAsync(keyword);
            AutoSuggestBoxSuggestion = searchedAlbums.List;
        }
        catch (HttpRequestException)
        {
            // Just ignore it...
        }
    }

    private static async Task DisplayContentDialog(string title, string message, string primaryButtonText = "", string closeButtonText = "")
    {
        await UIThreadHelper.RunOnUIThread(async () =>
        {
            ContentDialog contentDialog = new()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText
            };

            await contentDialog.ShowAsync();
        });
    }
}