using System.Windows.Input;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class AlbumListMenuFlyout : ListMenuFlyoutBase
{
    public AlbumListMenuFlyout()
    {
        this.InitializeComponent();
    }

    private void OnAlbumListMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData, addToNowPlayingCommandCallback: MultipleOperationEndCallbackCommand, playlistCommandCallback: MultipleOperationEndCallbackCommand);
    }
}
