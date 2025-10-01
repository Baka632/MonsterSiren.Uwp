namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="PlaylistPage"/> 提供视图模型。
/// </summary>
public sealed partial class PlaylistViewModel(PlaylistPage view) : ObservableObject
{
    [ObservableProperty]
    private Playlist selectedPlaylist;
    [ObservableProperty]
    private FlyoutBase selectedPlaylistContextFlyout;

    [RelayCommand]
    private static async Task CreateNewPlaylist()
    {
        await CommonValues.ShowCreatePlaylistDialog();
    }

    [RelayCommand]
    private static async Task PlayPlaylist(Playlist playlist)
    {
        await CommonValues.StartPlay(playlist);
    }

    [RelayCommand]
    private static async Task AddToNowPlaying(Playlist playlist)
    {
        await CommonValues.AddToNowPlaying(playlist);
    }

    [RelayCommand]
    private static async Task PlayNext(Playlist playlist)
    {
        await CommonValues.PlayNext(playlist);
    }

    [RelayCommand]
    private async Task AddPlaylistToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedPlaylist);
    }

    [RelayCommand]
    private static async Task ModifyPlaylist(Playlist playlist)
    {
        await CommonValues.ShowModifyPlaylistDialog(playlist);
    }

    [RelayCommand]
    private static async Task RemovePlaylist(Playlist playlist)
    {
        await CommonValues.RemovePlaylist(playlist);
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        view.PlaylistGridView.SelectionMode = ListViewSelectionMode.Multiple;
        view.PlaylistGridView.IsItemClickEnabled = false;
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

    [RelayCommand]
    private async Task PlayPlaylistForSelectedItem()
    {
        List<Playlist> selectedItems = GetSelectedItems(view.PlaylistGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.StartPlay(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddToNowPlayingForSelectedItem()
    {
        List<Playlist> selectedItems = GetSelectedItems(view.PlaylistGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task PlayNextForSelectedItem()
    {
        List<Playlist> selectedItems = GetSelectedItems(view.PlaylistGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.PlayNext(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSelectedItemToPlaylist(Playlist playlist)
    {
        List<Playlist> selectedItems = GetSelectedItems(view.PlaylistGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        await PlaylistService.AddItemsForPlaylistAsync(playlist, selectedItems);
    }

    [RelayCommand]
    private async Task RemoveSelectedPlaylist()
    {
        List<Playlist> selectedItems = GetSelectedItems(view.PlaylistGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        ContentDialogResult result = await CommonValues.DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                    "DeleteMultiplePlaylistsMessage".GetLocalized(),
                                                    "OK".GetLocalized(), "Cancel".GetLocalized());

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        foreach (Playlist playlist in selectedItems)
        {
            await CommonValues.RemovePlaylist(playlist, true);
        }
    }

    private static List<Playlist> GetSelectedItems(GridView gridView)
    {
        List<Playlist> selectedItems = new(5);

        foreach (ItemIndexRange range in gridView.SelectedRanges)
        {
            selectedItems.AddRange(PlaylistService.TotalPlaylists.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }
}