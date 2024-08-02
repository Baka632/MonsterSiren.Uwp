using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.ViewModels;

public sealed class PlaylistViewModel : ObservableObject
{
    public static ImageSource GetCoverImageForPlaylist(ObservableCollection<SongDetailAndAlbumDetailPack> items, int count = 0)
    {
        if (count <= 0)
        {
            //TODO
            return null;
        }
        else
        {
            SongDetailAndAlbumDetailPack pack = items.First();

            Uri uri = FileCacheHelper.GetAlbumCoverUriAsync(pack.AlbumDetail).Result
                ?? new(pack.AlbumDetail.CoverUrl, UriKind.Absolute);

            return new BitmapImage(uri);
        }
    }
}