using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
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

    [ObservableProperty]
    private bool isMediaInfoVisible;

    public MainViewModel()
    {
        MusicInfo.PropertyChanged += OnMusicInfoPropertyChanged;
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

    [RelayCommand]
    private async Task ToCompactNowPlayingPage()
    {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        
        ViewModePreferences preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
        preferences.CustomSize = new Size(300, 300);
        await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);

        MainPageNavigationHelper.Navigate(typeof(NowPlayingCompactPage), null, new SuppressNavigationTransitionInfo());
    }

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