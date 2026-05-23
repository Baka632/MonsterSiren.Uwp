using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
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
        await CommonValues.StartPlaySongFavorite();
    }

    [RelayCommand]
    private static async Task AddSongFavoriteToNowPlaying()
    {
        await CommonValues.AddSongFavoriteToNowPlaying();
    }

    [RelayCommand]
    private static async Task AddSongFavoriteToPlaylist(Playlist target)
    {
        await CommonValues.AddToPlaylist(target, FavoriteService.SongFavoriteList.ToAdapter());
    }

    [RelayCommand]
    private static async Task DownloadForSongFavorite()
    {
        await CommonValues.StartDownloadSongFavorites();
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
        view.SongList.SelectRange(new ItemIndexRange(0, (uint)FavoriteService.SongFavoriteList.Count));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)FavoriteService.SongFavoriteList.Count));
    }

    [RelayCommand]
    private async Task PlaySongListSelectedItem()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.StartPlay(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopSongListMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task PlayNextForSelectedItem()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.PlayNext(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopSongListMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToNowPlaying()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);
        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopSongListMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToPlaylist(Playlist playlist)
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);

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
                StopSongListMultipleSelection();
                return;
            }
        }

        await CommonValues.AddToPlaylist(playlist, selectedItems.ToAdapter());
    }

    [RelayCommand]
    private async Task RemoveSongListSelectedItemFromFavorite()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        await CommonValues.RemoveFromFavorite(selectedItems.ToAdapter());

        StopSongListMultipleSelection();
    }

    [RelayCommand]
    private async Task DownloadForSongListSelectedItem()
    {
        List<SongFavoriteItem> selectedItems = GetSelectedItem(view.SongList);

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
                StopSongListMultipleSelection();
                return;
            }
        }

        bool isAllSuccess = await CommonValues.StartDownload(selectedItems.ToAdapter());

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