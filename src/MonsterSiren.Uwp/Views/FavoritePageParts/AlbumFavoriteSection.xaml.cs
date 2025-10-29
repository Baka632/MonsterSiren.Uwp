using MonsterSiren.Uwp.ViewModels.FavoriteParts;

namespace MonsterSiren.Uwp.Views.FavoritePageParts;

public sealed partial class AlbumFavoriteSection : UserControl
{
    public AlbumFavoriteSectionViewModel ViewModel { get; }

    public AlbumFavoriteSection()
    {
        ViewModel = new();
        this.InitializeComponent();
    }
}
