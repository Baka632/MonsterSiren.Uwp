using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Views.FavoritePageParts;

namespace MonsterSiren.Uwp.ViewModels.FavoriteParts;

public partial class AlbumFavoriteSectionViewModel : ObservableObject
{
    [ObservableProperty]
    private AlbumFavoriteItem selectedAlbumItem;
    [ObservableProperty]
    private FlyoutBase selectedAlbumInfoContextFlyout;

    private readonly AlbumFavoriteSection view;
    private SelectionHelper selectionHelper;

    public Func<ISongCidProvider> SongCidProviderFactory { get; }

    public AlbumFavoriteSectionViewModel(AlbumFavoriteSection albumFavoriteSection)
    {
        SongCidProviderFactory = GetSongCidProvider;
        view = albumFavoriteSection;
    }

    public void Initialize()
    {
        selectionHelper = new(view.AlbumGridView, view.AlbumSelectionFlyout, view.AlbumContextFlyout, flyout => SelectedAlbumInfoContextFlyout = flyout);
    }

    [RelayCommand]
    private static async Task PlayForAlbumFavorite()
    {
        await CommonValues.StartPlayAlbumFavorite();
    }

    [RelayCommand]
    private static async Task DownloadForAlbumFavorite()
    {
        await CommonValues.StartDownloadAlbumFavorites();
    }

    [RelayCommand]
    private void StartMultipleSelection() => selectionHelper.StartMultipleSelection(SelectedAlbumItem);

    [RelayCommand]
    private void StopMultipleSelection() => selectionHelper.StopMultipleSelection();

    [RelayCommand]
    private void SelectAllSongList() => selectionHelper.SelectList(FavoriteService.AlbumFavoriteList.Count);

    [RelayCommand]
    private void DeselectAllSongList() => selectionHelper.DeselectList(FavoriteService.AlbumFavoriteList.Count);

    private List<AlbumFavoriteItem> GetSelectedItems()
    {
        GridView gridView = view.AlbumGridView;

        List <AlbumFavoriteItem> selectedItems = new(5);

        foreach (ItemIndexRange range in gridView.SelectedRanges)
        {
            selectedItems.AddRange(FavoriteService.AlbumFavoriteList.Items.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }

    private AlbumFavoriteItemSequenceAdapter GetSongCidProvider() => GetSelectedItems().ToAdapter();
}