using Windows.ApplicationModel.DataTransfer;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistDetailViewModel(PlaylistDetailPage view) : ObservableObject
{
    [ObservableProperty]
    private Playlist currentPlaylist;
    [ObservableProperty]
    private PlaylistItem selectedItem;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public void Initialize(Playlist model)
    {
        CurrentPlaylist = model ?? throw new ArgumentNullException(nameof(model));
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
    }

    [RelayCommand]
    private async Task PlayForCurrentPlaylist()
    {
        await CommonValues.StartPlay(CurrentPlaylist);
    }

    [RelayCommand]
    private async Task PlayForItem(PlaylistItem item)
    {
        await CommonValues.StartPlay(item, CurrentPlaylist);
    }

    [RelayCommand]
    private async Task AddItemToNowPlaying(PlaylistItem item)
    {
        await CommonValues.AddToNowPlaying(item, CurrentPlaylist);
    }

    [RelayCommand]
    private async Task AddCurrentPlaylistToNowPlaying()
    {
        await CommonValues.AddToNowPlaying(CurrentPlaylist);
    }

    [RelayCommand]
    private async Task AddItemToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedItem);
    }

    [RelayCommand]
    private async Task AddCurrentPlaylistToAnotherPlaylistCommand(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, CurrentPlaylist);
    }

    [RelayCommand]
    private static async Task DownloadForItem(PlaylistItem item)
    {
        await CommonValues.StartDownload(item);
    }

    [RelayCommand]
    private async Task DownloadForCurrentPlaylist()
    {
        await CommonValues.StartDownload(CurrentPlaylist);
    }

    [RelayCommand]
    private static void CopySongNameToClipboard(PlaylistItem item)
    {
        DataPackage package = new()
        {
            RequestedOperation = DataPackageOperation.Copy
        };
        package.SetText(item.SongTitle);
        Clipboard.SetContent(package);
    }

    [RelayCommand]
    private async Task RemoveItemFromPlaylist(PlaylistItem item)
    {
        await PlaylistService.RemoveItemForPlaylistAsync(CurrentPlaylist, item);
    }

    [RelayCommand]
    private async Task ModifyPlaylist()
    {
        await CommonValues.ModifyPlaylist(CurrentPlaylist);
    }

    [RelayCommand]
    private async Task RemovePlaylist()
    {
        await CommonValues.RemovePlaylist(CurrentPlaylist);
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        // Single 模式只能选一个
        ItemIndexRange range = view.SongList.SelectedRanges.FirstOrDefault();

        view.SongList.SelectionMode = ListViewSelectionMode.Multiple;

        if (range is not null)
        {
            view.SongList.SelectRange(range);
        }

        SelectedSongListItemContextFlyout = view.SongSelectionFlyout;
    }

    [RelayCommand]
    private void StopMultipleSelection()
    {
        view.SongList.SelectionMode = ListViewSelectionMode.Single;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.SongList.SelectAll();
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentPlaylist.SongCount));
    }

    [RelayCommand]
    private async Task PlaySongListSelectedItem()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        IEnumerable<PlaylistItem> items = selectedItems.Where(obj => obj is PlaylistItem).Select(obj => (PlaylistItem)obj);

        bool isSuccess = await CommonValues.StartPlay(items);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToNowPlaying()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        IEnumerable<PlaylistItem> items = selectedItems.Where(obj => obj is PlaylistItem).Select(obj => (PlaylistItem)obj);

        bool isSuccess = await CommonValues.AddToNowPlaying(items);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToAnotherPlaylist(Playlist playlist)
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        IEnumerable<PlaylistItem> items = selectedItems.Where(item => item is PlaylistItem).Select(item => (PlaylistItem)item);
        await CommonValues.AddToPlaylist(playlist, items);
    }

    [RelayCommand]
    private async Task DownloadForSongListSelectedItem()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        IEnumerable<PlaylistItem> items = selectedItems.Where(item => item is PlaylistItem).Select(item => (PlaylistItem)item);
        bool isAllSuccess = await CommonValues.StartDownload(items);

        if (isAllSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task RemoveSongListSelectedItemFromPlaylist()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        PlaylistItem[] items = selectedItems.Where(item => item is PlaylistItem).Select(item => (PlaylistItem)item).ToArray();
        await PlaylistService.RemoveItemsForPlaylist(CurrentPlaylist, items);

        StopMultipleSelection();
    }
}