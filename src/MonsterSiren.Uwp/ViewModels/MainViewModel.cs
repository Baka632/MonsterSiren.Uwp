using System.Net.Http;
using System.ComponentModel;
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
    private static async Task CreateNewPlaylist()
    {
        await CommonValues.CreatePlaylist();
    }

    [RelayCommand]
    private static async Task PlayForPlaylist(Playlist playlist)
    {
        await CommonValues.StartPlay(playlist);
    }

    [RelayCommand]
    private static async Task AddPlaylistToNowPlaying(Playlist playlist)
    {
        await CommonValues.AddToNowPlaying(playlist);
    }

    [RelayCommand]
    private async Task AddPlaylistToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedPlaylist);
    }

    [RelayCommand]
    private static async Task ModifyPlaylist(Playlist playlist)
    {
        await CommonValues.ModifyPlaylist(playlist);
    }

    [RelayCommand]
    private static async Task RemovePlaylist(Playlist playlist)
    {
        await CommonValues.RemovePlaylist(playlist);
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
            List<AlbumInfo> albums = searchedAlbums.List.ToList();
            await MsrModelsHelper.TryFillArtistAndCachedCoverForAlbum(albums);

            AutoSuggestBoxSuggestion = albums;
        }
        catch (HttpRequestException)
        {
            // Just ignore it...
        }
    }
}