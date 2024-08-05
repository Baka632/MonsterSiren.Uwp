// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Text.Json;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistPage : Page
{
    public PlaylistViewModel ViewModel { get; } = new PlaylistViewModel();

    public PlaylistPage()
    {
        this.InitializeComponent();
    }

    private void OnPlaylistItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Playlist playlist)
        {
            ContentFrameNavigationHelper.Navigate(typeof(PlaylistDetailPage), playlist, CommonValues.DefaultTransitionInfo);
        }
    }

    private void OnPlaylistItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        string json = JsonSerializer.Serialize((Playlist)dataContext);
        e.Data.SetData(CommonValues.MusicPlaylistFormatId, json);
    }
}
