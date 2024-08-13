// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistDetailPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsPlaylistEmpty { get => (ViewModel.CurrentPlaylist.Items.Count) <= 0; }

    public PlaylistDetailViewModel ViewModel { get; } = new PlaylistDetailViewModel();

    public PlaylistDetailPage()
    {
        this.InitializeComponent();
    }

    public static string PlaylistTotalDurationToString(TimeSpan timeSpan)
    {
        TimeSpan span = timeSpan;
        if (span.Hours == 0)
        {
            return string.Format("MinutesAndSecondsFormat".GetLocalized(),
                                 span.Minutes,
                                 span.Seconds);
        }
        else
        {
            return string.Format("HoursAndMinutesFormat".GetLocalized(),
                                 (span.Days * 24) + span.Hours,
                                 span.Minutes);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Playlist model = (Playlist)e.Parameter;
        ViewModel.Initialize(model);
        ViewModel.CurrentPlaylist.Items.CollectionChanged += OnTotalPlaylistsCollectionChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        ViewModel.CurrentPlaylist.Items.CollectionChanged -= OnTotalPlaylistsCollectionChanged;
    }

    private void OnTotalPlaylistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaylistEmpty)));
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        PlaylistItem pack = (PlaylistItem)dataContext;

        string json = JsonSerializer.Serialize(pack);

        e.Data.SetData(CommonValues.MusicPlaylistItemFormatId, json);
    }

    private void OnListViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedItem = (PlaylistItem)element.DataContext;
    }

    private void OnMoreOptionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        Button button = (Button)sender;
        ViewModel.SelectedItem = (PlaylistItem)button.DataContext;
    }

    private void OnAddSongToAnotherPlaylistSubItemLoaded(object sender, RoutedEventArgs e)
    {
        MenuFlyoutSubItem subItem = (MenuFlyoutSubItem)sender;
        FillSubItemWithCommand(subItem, ViewModel.AddPackToAnotherPlaylistCommand);
    }

    private void OnAddPlaylistToAnotherPlaylistSubItemLoaded(object sender, RoutedEventArgs e)
    {
        MenuFlyoutSubItem subItem = (MenuFlyoutSubItem)sender;
        FillSubItemWithCommand(subItem, ViewModel.AddCurrentPlaylistToAnotherPlaylistCommandCommand);
    }

    private void FillSubItemWithCommand(MenuFlyoutSubItem subItem, ICommand command)
    {
        if (PlaylistService.TotalPlaylists.Count > 0)
        {
            subItem.Items.Clear();
            subItem.IsEnabled = true;

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
                    Command = command,
                    CommandParameter = playlist,
                    IsEnabled = ViewModel.CurrentPlaylist != playlist
                };
                subItem.Items.Add(item);
            }
        }
        else
        {
            subItem.IsEnabled = false;
        }
    }
}
