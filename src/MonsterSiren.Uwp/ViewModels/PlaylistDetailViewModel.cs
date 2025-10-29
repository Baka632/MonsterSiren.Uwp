using Windows.ApplicationModel.DataTransfer;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="PlaylistDetailPage"/> 提供视图模型。
/// </summary>
public sealed partial class PlaylistDetailViewModel(PlaylistDetailPage view) : ObservableObject
{
    [ObservableProperty]
    private Playlist currentPlaylist;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedItemContainsInFavorite))]
    private PlaylistItem selectedItem;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public bool IsSelectedItemContainsInFavorite { get => FavoriteService.ContainsItem(SelectedItem); }

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
    private async Task AddSongToFavorite(PlaylistItem item)
    {
        await CommonValues.AddToFavorite(item);
        OnPropertyChanged(nameof(IsSelectedItemContainsInFavorite));
    }

    [RelayCommand]
    private async Task RemoveSongFromFavorite(PlaylistItem item)
    {
        await CommonValues.RemoveFromFavorite(item);
        OnPropertyChanged(nameof(IsSelectedItemContainsInFavorite));
    }

    [RelayCommand]
    private async Task PlayNextForItem(PlaylistItem item)
    {
        await CommonValues.PlayNext(item, CurrentPlaylist);
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
        await CommonValues.ShowModifyPlaylistDialog(CurrentPlaylist);
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
        view.SongList.SelectRange(new ItemIndexRange(0, (uint)CurrentPlaylist.SongCount));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentPlaylist.SongCount));
    }

    [RelayCommand]
    private async Task PlaySongListSelectedItem()
    {
        List<PlaylistItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.StartPlay(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToNowPlaying()
    {
        List<PlaylistItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task PlayNextForSelectedItem()
    {
        List<PlaylistItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.PlayNext(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToAnotherPlaylist(Playlist playlist)
    {
        List<PlaylistItem> selectedItems = GetSelectedItem(view.SongList);
        await CommonValues.AddToPlaylist(playlist, selectedItems);
    }

    [RelayCommand]
    private async Task AddSongsToFavoriteForSelectedItem()
    {
        List<PlaylistItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.AddToFavorite(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task DownloadForSongListSelectedItem()
    {
        List<PlaylistItem> selectedItems = GetSelectedItem(view.SongList);
        bool isAllSuccess = await CommonValues.StartDownload(selectedItems);

        if (isAllSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task RemoveSongListSelectedItemFromPlaylist()
    {
        List<PlaylistItem> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        await PlaylistService.RemoveItemsForPlaylistAsync(CurrentPlaylist, selectedItems);

        StopMultipleSelection();
    }

    private List<PlaylistItem> GetSelectedItem(ListView listView)
    {
        List<PlaylistItem> selectedItems = new(5);

        foreach (ItemIndexRange range in listView.SelectedRanges)
        {
            selectedItems.AddRange(CurrentPlaylist.Items.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }
}