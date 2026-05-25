using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="PlaylistPage"/> 提供视图模型。
/// </summary>
public sealed partial class PlaylistViewModel : ObservableObject
{
    private readonly PlaylistPage view;

    [ObservableProperty]
    private Playlist selectedPlaylist;
    [ObservableProperty]
    private FlyoutBase selectedPlaylistContextFlyout;

    public Func<IEnumerable<Playlist>> PlaylistFactory { get; }

    public PlaylistViewModel(PlaylistPage playlistPage)
    {
        view = playlistPage;
        PlaylistFactory = GetSelectedItems;
    }

    [RelayCommand]
    private static async Task CreateNewPlaylist()
    {
        await CommonValues.ShowCreatePlaylistDialog();
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        GridView playlistGridView = view.PlaylistGridView;
        playlistGridView.SelectionMode = ListViewSelectionMode.Multiple;
        playlistGridView.SelectedItem = SelectedPlaylist;
        playlistGridView.IsItemClickEnabled = false;
        SelectedPlaylistContextFlyout = view.PlaylistSelectionFlyout;
    }

    [RelayCommand]
    private void StopMultipleSelection()
    {
        view.PlaylistGridView.SelectionMode = ListViewSelectionMode.None;
        view.PlaylistGridView.IsItemClickEnabled = true;
        SelectedPlaylistContextFlyout = view.PlaylistContextFlyout;
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.PlaylistGridView.SelectRange(new ItemIndexRange(0, (uint)PlaylistService.TotalPlaylists.Count));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.PlaylistGridView.DeselectRange(new ItemIndexRange(0, (uint)PlaylistService.TotalPlaylists.Count));
    }

    private List<Playlist> GetSelectedItems()
    {
        GridView gridView = view.PlaylistGridView;
        List <Playlist> selectedItems = new(5);

        foreach (ItemIndexRange range in gridView.SelectedRanges)
        {
            selectedItems.AddRange(PlaylistService.TotalPlaylists.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }
}