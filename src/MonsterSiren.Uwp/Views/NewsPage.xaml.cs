// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 新闻动向页
/// </summary>
public sealed partial class NewsPage : Page
{
    public NewsViewModel ViewModel { get; } = new NewsViewModel();

    public NewsPage()
    {
        this.InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.Initialize();
    }
}
