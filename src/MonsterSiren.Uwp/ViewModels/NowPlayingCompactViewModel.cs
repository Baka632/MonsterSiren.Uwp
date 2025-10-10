using Windows.UI.ViewManagement;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="NowPlayingCompactPage"/> 提供视图模型。
/// </summary>
public partial class NowPlayingCompactViewModel : ObservableObject
{
    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;

    [RelayCommand]
    private static async Task Back()
    {
        await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
        MainPageNavigationHelper.GoBack();
    }
}