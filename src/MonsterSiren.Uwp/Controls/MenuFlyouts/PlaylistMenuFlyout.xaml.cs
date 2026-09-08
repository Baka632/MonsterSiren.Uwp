using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistMenuFlyout : ItemMenuFlyoutBase
{
    public PlaylistMenuFlyout()
    {
        this.InitializeComponent();
    }

    private void OnPlaylistMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData, (Playlist)SourceData);
    }
}
