namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class PlaylistListMenuFlyout : ListMenuFlyoutBase
{
    public PlaylistListMenuFlyout()
    {
        this.InitializeComponent();
    }

    private void OnPlaylistListMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData, null, MultipleOperationEndCallbackCommand, MultipleOperationEndCallbackCommand);
    }
}
