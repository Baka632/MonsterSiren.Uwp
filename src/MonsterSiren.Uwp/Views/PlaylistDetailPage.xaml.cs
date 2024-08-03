// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Net.Http;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistDetailPage : Page
{
    public PlaylistDetailViewModel ViewModel { get; } = new PlaylistDetailViewModel();

    public PlaylistDetailPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Playlist model = (Playlist)e.Parameter;
        ViewModel.Initialize(model);
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {

    }

    private async void OnSongDurationTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        SongDetail detail = (SongDetail)textBlock.DataContext;
        textBlock.Text = "-:-";

        try
        {
            TimeSpan? span = await SongDetailHelper.GetSongDurationAsync(detail);

            if (span.HasValue)
            {
                textBlock.Text = span.Value.ToString(@"m\:ss");
            }
            else
            {
                textBlock.Visibility = Visibility.Collapsed;
            }
        }
        catch (HttpRequestException)
        {
            textBlock.Visibility = Visibility.Collapsed;
        }
    }
}
