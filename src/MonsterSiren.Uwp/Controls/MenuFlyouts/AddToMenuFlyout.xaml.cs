using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class AddToMenuFlyout : AddToMenuFlyoutBase
{
    public AddToMenuFlyout()
    {
        this.InitializeComponent();
    }

    private void OnAddToMenuFlyoutLoading(FrameworkElement sender, object args)
    {
        Playlist optionalPlaylist = SourceData as Playlist;

        CommonValues.InitializeAddToPlaylistSubItem(ViewModel.AddItemToPlaylistCommand,
                                                playlist => new CommandParameter((playlist, SourceData), null),
                                                optionalPlaylist, AddToPlaylistMenuFlyoutSubItem);
    }
}
