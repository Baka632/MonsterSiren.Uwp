namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public sealed partial class AlbumMenuFlyout : ItemMenuFlyoutBase
{
    public AlbumMenuFlyout()
    {
        this.InitializeComponent();
    }

    private void OnAlbumMenuFlyoutOpening(object sender, object e)
    {
        InitializeAddToMenuFlyoutItem(SourceData);
    }
}
