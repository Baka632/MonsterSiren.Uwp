// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsTotalPlaylistEmpty => PlaylistService.TotalPlaylists.Count <= 0;
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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        PlaylistService.TotalPlaylists.CollectionChanged += OnTotalPlaylistsCollectionChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        PlaylistService.TotalPlaylists.CollectionChanged -= OnTotalPlaylistsCollectionChanged;
    }

    private void OnTotalPlaylistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsTotalPlaylistEmpty)));
    }

    private void OnGridViewItemRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedPlaylist = (Playlist)element.DataContext;
    }

    private void OnAddToPlaylistSubItemLoaded(object sender, RoutedEventArgs e)
    {
        if (PlaylistService.TotalPlaylists.Count > 0)
        {
            AddToPlaylistSubItem.Items.Clear();
            AddToPlaylistSubItem.IsEnabled = true;

            foreach (Playlist playlist in PlaylistService.TotalPlaylists)
            {
                MenuFlyoutItem item = new()
                {
                    DataContext = playlist,
                    Text = playlist.Title,
                    Icon = new FontIcon()
                    {
                        Glyph = "\uEC4F"
                    },
                    Command = ViewModel.AddPlaylistToAnotherPlaylistCommand,
                    CommandParameter = playlist,
                    IsEnabled = ViewModel.SelectedPlaylist != playlist
                };
                AddToPlaylistSubItem.Items.Add(item);
            }
        }
        else
        {
            AddToPlaylistSubItem.IsEnabled = false;
        }
    }
}
