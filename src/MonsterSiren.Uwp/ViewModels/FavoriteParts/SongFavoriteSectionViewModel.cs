using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Views.FavoritePageParts;

namespace MonsterSiren.Uwp.ViewModels.FavoriteParts;

/// <summary>
/// 为 <see cref="SongFavoriteSection"/> 提供视图模型。
/// </summary>
public partial class SongFavoriteSectionViewModel : ObservableObject
{
    private readonly SongFavoriteSection view;

    [ObservableProperty]
    private SongFavoriteItem selectedSongItem;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public Func<ISongCidProvider> SongCidProviderFactory { get; }

    public SongFavoriteSectionViewModel(SongFavoriteSection songFavoriteSection)
    {
        view = songFavoriteSection;
        SongCidProviderFactory = GetSongCidProvider;
    }

    [RelayCommand]
    private static async Task PlayForSongFavorite()
    {
        await CommonValues.StartPlaySongFavorite();
    }

    [RelayCommand]
    private static async Task DownloadForSongFavorite()
    {
        await CommonValues.StartDownloadSongFavorites();
    }

    [RelayCommand]
    private void StartSongListMultipleSelection()
    {
        ListView songList = view.SongList;
        songList.SelectionMode = ListViewSelectionMode.Multiple;
        songList.SelectedItem = SelectedSongItem;

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

    private List<SongFavoriteItem> GetSelectedItems()
    {
        ListView listView = view.SongList;

        List <SongFavoriteItem> selectedItems = new(5);

        foreach (ItemIndexRange range in listView.SelectedRanges)
        {
            selectedItems.AddRange(FavoriteService.SongFavoriteList.Items.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }

    private SongFavoriteItemSequenceAdapter GetSongCidProvider() => GetSelectedItems().ToAdapter();
}