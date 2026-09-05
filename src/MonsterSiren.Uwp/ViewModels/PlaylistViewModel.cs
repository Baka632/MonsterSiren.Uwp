using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="PlaylistPage"/> 提供视图模型。
/// </summary>
public sealed partial class PlaylistViewModel : ObservableObject
{
    private readonly PlaylistPage view;
    private SelectionHelper selectionHelper;

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

    public void Initialize()
    {
        selectionHelper = new(view.PlaylistGridView, view.PlaylistSelectionFlyout, view.PlaylistContextFlyout, flyout => SelectedPlaylistContextFlyout = flyout);
    }

    [RelayCommand]
    private static async Task CreateNewPlaylist()
    {
        await CommonValues.ShowCreatePlaylistDialog();
    }

    [RelayCommand]
    private void StartMultipleSelection() => selectionHelper.StartMultipleSelection(SelectedPlaylist);

    [RelayCommand]
    private void StopMultipleSelection() => selectionHelper.StopMultipleSelection();

    [RelayCommand]
    private void SelectAllSongList() => selectionHelper.SelectList(PlaylistService.TotalPlaylists.Count);

    [RelayCommand]
    private void DeselectAllSongList() => selectionHelper.DeselectList(PlaylistService.TotalPlaylists.Count);

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