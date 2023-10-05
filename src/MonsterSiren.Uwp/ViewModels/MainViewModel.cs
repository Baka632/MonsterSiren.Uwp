using Microsoft.UI.Xaml.Controls;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MainPage"/> 提供视图模型
/// </summary>
public partial class MainViewModel : ObservableObject
{
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
}