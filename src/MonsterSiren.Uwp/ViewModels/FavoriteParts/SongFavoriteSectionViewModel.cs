using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Views.FavoritePageParts;
using Windows.ApplicationModel.DataTransfer;

namespace MonsterSiren.Uwp.ViewModels.FavoriteParts;

/// <summary>
/// 为 <see cref="SongFavoriteSection"/> 提供视图模型。
/// </summary>
public partial class SongFavoriteSectionViewModel(SongFavoriteSection view) : ObservableObject
{
    [ObservableProperty]
    private SongFavoriteItem selectedSongItem;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    [RelayCommand]
    private static async Task PlayForSongFavorite()
    {
        await CommonValues.StartPlaySongFavorites();
    }

    [RelayCommand]
    private static async Task AddSongFavoriteToNowPlaying()
    {
        await CommonValues.AddSongFavoriteToNowPlaying();
    }

    [RelayCommand]
    private static async Task AddSongFavoriteToPlaylistCommand(Playlist target)
    {
        await PlaylistService.AddItemsForPlaylistAsync(target, FavoriteService.SongFavoriteList.Items);
    }

    [RelayCommand]
    private static async Task DownloadForSongFavorite()
    {
        await CommonValues.StartDownloadSongFavorites();
    }

    [RelayCommand]
    private static async Task PlayForSongItem(SongFavoriteItem item)
    {
        await CommonValues.StartPlay(item);
    }

    [RelayCommand]
    private static async Task PlayNextForSongItem(SongFavoriteItem item)
    {
        await CommonValues.PlayNext(item);
    }

    [RelayCommand]
    private static async Task RemoveSongFromFavorite(SongFavoriteItem item)
    {
        await CommonValues.RemoveFromFavorite(item);
    }

    [RelayCommand]
    private static async Task DownloadForSongItem(SongFavoriteItem item)
    {
        await CommonValues.StartDownload(item);
    }

    [RelayCommand]
    private static void CopySongNameToClipboard(SongFavoriteItem item)
    {
        DataPackage package = new()
        {
            RequestedOperation = DataPackageOperation.Copy
        };
        package.SetText(item.SongTitle);
        Clipboard.SetContent(package);
    }

    [RelayCommand]
    private static async Task AddSongToNowPlaying(SongFavoriteItem favoriteItem)
    {
        await CommonValues.AddToNowPlaying(favoriteItem);
    }

    [RelayCommand]
    private async Task AddSongToPlaylist(Playlist target)
    {
        await CommonValues.AddToPlaylist(target, SelectedSongItem);
    }

    [RelayCommand]
    private void StartSongListMultipleSelection()
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
    private void StopSongListMultipleSelection()
    {
        view.SongList.SelectionMode = ListViewSelectionMode.Single;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.SongList.SelectRange(new ItemIndexRange(0, (uint)FavoriteService.SongFavoriteList.SongCount));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)FavoriteService.SongFavoriteList.SongCount));
    }

    [RelayCommand]
    private async Task PlaySongListSelectedItem()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.StartPlay(selectedItems);

        if (isSuccess)
        {
            StopSongListMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task PlayNextForSelectedItem()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.PlayNext(selectedItems);

        if (isSuccess)
        {
            StopSongListMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToNowPlaying()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems);

        if (isSuccess)
        {
            StopSongListMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToPlaylist(Playlist playlist)
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        await CommonValues.AddToPlaylist(playlist, selectedItems);
    }

    [RelayCommand]
    private async Task RemoveSongListSelectedItemFromFavorite()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        await FavoriteService.RemoveSongsFromFavoriteAsync(selectedItems);

        StopSongListMultipleSelection();
    }

    [RelayCommand]
    private async Task DownloadForSongListSelectedItem()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        bool isAllSuccess = await CommonValues.StartDownload(selectedItems);

        if (isAllSuccess)
        {
            StopSongListMultipleSelection();
        }
    }

    private static List<SongFavoriteItem> GetSelectedItem(ListView listView)
    {
        List<SongFavoriteItem> selectedItems = new(5);

        foreach (ItemIndexRange range in listView.SelectedRanges)
        {
            selectedItems.AddRange(FavoriteService.SongFavoriteList.Items.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }
}