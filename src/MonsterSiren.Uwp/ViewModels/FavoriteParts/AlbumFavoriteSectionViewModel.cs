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

    public Func<ISongCidProvider> SongCidProviderFactory { get; }

    public AlbumFavoriteSectionViewModel(AlbumFavoriteSection albumFavoriteSection)
    {
        SongCidProviderFactory = GetSongCidProvider;
        view = albumFavoriteSection;
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
    private void StartMultipleSelection()
    {
        GridView albumGridView = view.AlbumGridView;
        albumGridView.SelectionMode = ListViewSelectionMode.Multiple;
        albumGridView.SelectedItem = SelectedAlbumItem;
        albumGridView.IsItemClickEnabled = false;
        SelectedAlbumInfoContextFlyout = view.AlbumSelectionFlyout;
    }

    [RelayCommand]
    private void StopMultipleSelection()
    {
        GridView albumGridView = view.AlbumGridView;
        albumGridView.SelectionMode = ListViewSelectionMode.None;
        albumGridView.IsItemClickEnabled = true;
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
        // TODO: 取消选择的方法存在先前选择项目残留的问题。
        view.AlbumGridView.DeselectRange(new ItemIndexRange(0, (uint)FavoriteService.AlbumFavoriteList.Count));
    }

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