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
    private SelectionHelper selectionHelper;

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

    public void Initialize()
    {
        selectionHelper = new(view.SongList, view.SongSelectionFlyout, view.SongContextFlyout, flyout => SelectedSongListItemContextFlyout = flyout);
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
    private void StartSongListMultipleSelection() => selectionHelper.StartMultipleSelection(SelectedSongItem);

    [RelayCommand]
    private void StopSongListMultipleSelection() => selectionHelper.StopMultipleSelection();

    [RelayCommand]
    private void SelectAllSongList() => selectionHelper.SelectList(FavoriteService.SongFavoriteList.Count);

    [RelayCommand]
    private void DeselectAllSongList() => selectionHelper.DeselectList(FavoriteService.SongFavoriteList.Count);

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