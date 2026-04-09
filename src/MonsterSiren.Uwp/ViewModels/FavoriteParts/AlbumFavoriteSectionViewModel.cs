using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
using MonsterSiren.Uwp.Views.FavoritePageParts;

namespace MonsterSiren.Uwp.ViewModels.FavoriteParts;

public partial class AlbumFavoriteSectionViewModel(AlbumFavoriteSection view) : ObservableObject
{
    [ObservableProperty]
    private AlbumFavoriteItem selectedAlbumItem;
    [ObservableProperty]
    private FlyoutBase selectedAlbumInfoContextFlyout;

    [RelayCommand]
    private static async Task PlayForAlbumFavorite()
    {
        await CommonValues.StartPlayAlbumFavorite();
    }

    [RelayCommand]
    private static async Task AddAlbumFavoriteToNowPlaying()
    {
        await CommonValues.AddAlbumFavoriteToNowPlaying();
    }

    [RelayCommand]
    private static async Task AddAlbumFavoriteToPlaylistCommand(Playlist target)
    {
        await PlaylistService.AddItemsForPlaylistAsync(target, FavoriteService.AlbumFavoriteList.Items);
    }

    [RelayCommand]
    private static async Task DownloadForAlbumFavorite()
    {
        await CommonValues.StartDownloadAlbumFavorites();
    }

    [RelayCommand]
    private static async Task PlayAlbumForAlbumItem(AlbumFavoriteItem item)
    {
        await CommonValues.StartPlay(item.ToAdapter());
    }

    [RelayCommand]
    private static async Task PlayNextForAlbumItem(AlbumFavoriteItem item)
    {
        await CommonValues.PlayNext(item.ToAdapter());
    }

    [RelayCommand]
    private static async Task DownloadForAlbumItem(AlbumFavoriteItem item)
    {
        await CommonValues.StartDownload(item);
    }

    [RelayCommand]
    private static async Task RemoveAlbumFromFavorite(AlbumFavoriteItem item)
    {
        await CommonValues.RemoveFromFavorite(item.ToAdapter());
    }

    [RelayCommand]
    private static async Task AddAlbumToNowPlaying(AlbumFavoriteItem favoriteItem)
    {
        await CommonValues.AddToNowPlaying(favoriteItem.ToAdapter());
    }

    [RelayCommand]
    private async Task AddAlbumToPlaylist(Playlist target)
    {
        await CommonValues.AddToPlaylist(target, SelectedAlbumItem);
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        view.AlbumGridView.SelectionMode = ListViewSelectionMode.Multiple;
        view.AlbumGridView.IsItemClickEnabled = false;
        SelectedAlbumInfoContextFlyout = view.AlbumSelectionFlyout;
    }

    [RelayCommand]
    private void StopMultipleSelection()
    {
        view.AlbumGridView.SelectionMode = ListViewSelectionMode.None;
        view.AlbumGridView.IsItemClickEnabled = true;
        SelectedAlbumInfoContextFlyout = view.AlbumContextFlyout;
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.AlbumGridView.SelectRange(new ItemIndexRange(0, (uint)FavoriteService.AlbumFavoriteList.Count));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.AlbumGridView.DeselectRange(new ItemIndexRange(0, (uint)FavoriteService.AlbumFavoriteList.Count));
    }

    [RelayCommand]
    private async Task PlayAlbumForSelectedItem()
    {
        List<AlbumFavoriteItem> selectedItems = GetSelectedItems(view.AlbumGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.StartPlay(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddToNowPlayingForSelectedItem()
    {
        List<AlbumFavoriteItem> selectedItems = GetSelectedItems(view.AlbumGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task PlayNextForSelectedItem()
    {
        List<AlbumFavoriteItem> selectedItems = GetSelectedItems(view.AlbumGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.PlayNext(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSelectedItemToPlaylist(Playlist playlist)
    {
        List<AlbumFavoriteItem> selectedItems = GetSelectedItems(view.AlbumGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count >= CommonValues.TooManyItemThresholdCount)
        {
            ContentDialogResult result = await CommonValues.DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                    "AddTooManyItemToPlaylistMessage".GetLocalized(),
                                                    "Continue".GetLocalized(), "Cancel".GetLocalized());

            if (result != ContentDialogResult.Primary)
            {
                StopMultipleSelection();
                return;
            }
        }

        await CommonValues.AddToPlaylist(playlist, selectedItems);
    }

    [RelayCommand]
    private async Task DownloadForSelectedItem()
    {
        List<AlbumFavoriteItem> selectedItems = GetSelectedItems(view.AlbumGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count >= CommonValues.TooManyItemThresholdCount)
        {
            ContentDialogResult result = await CommonValues.DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                    "DownloadTooManyItemMessage".GetLocalized(),
                                                    "Continue".GetLocalized(), "Cancel".GetLocalized());

            if (result != ContentDialogResult.Primary)
            {
                StopMultipleSelection();
                return;
            }
        }

        bool isSuccess = await CommonValues.StartDownload(selectedItems);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task RemoveSelectedItemsFromFavorite()
    {
        List<AlbumFavoriteItem> selectedItems = GetSelectedItems(view.AlbumGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        await CommonValues.RemoveFromFavorite(selectedItems.ToAdapter());

        StopMultipleSelection();
    }

    private List<AlbumFavoriteItem> GetSelectedItems(GridView gridView)
    {
        List<AlbumFavoriteItem> selectedItems = new(5);

        foreach (ItemIndexRange range in gridView.SelectedRanges)
        {
            selectedItems.AddRange(FavoriteService.AlbumFavoriteList.Items.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }
}