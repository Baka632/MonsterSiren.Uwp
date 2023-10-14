// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 画中画模式的正在播放页
/// </summary>
public sealed partial class NowPlayingCompactPage : Page
{
    public NowPlayingCompactViewModel ViewModel { get; } = new();

    public NowPlayingCompactPage()
    {
        this.InitializeComponent();
    }
}
