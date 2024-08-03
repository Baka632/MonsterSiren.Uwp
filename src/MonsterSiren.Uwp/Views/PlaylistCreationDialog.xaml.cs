// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace MonsterSiren.Uwp.Views;

[INotifyPropertyChanged]
public sealed partial class PlaylistCreationDialog : ContentDialog
{
    [ObservableProperty]
    private string playlistTitle;
    [ObservableProperty]
    private string playlistDescription;
    [ObservableProperty]
    private bool showInfoBar;
    [ObservableProperty]
    private string infoBarTitle;
    [ObservableProperty]
    private string infoBarMessage;

    public PlaylistCreationDialog()
    {
        this.InitializeComponent();
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(PlaylistTitle))
        {
            ShowInfoBar = true;
            InfoBarTitle = "PlaylistTitleEmptyTitle".GetLocalized();
            InfoBarMessage = "PlaylistTitleEmptyMessage".GetLocalized();
            args.Cancel = true;
        }
        else if (PlaylistService.TotalPlaylists.Any(item => item.Title == PlaylistTitle))
        {
            ShowInfoBar = true;
            InfoBarTitle = "PlaylistTitleAlreadyExistsTitle".GetLocalized();
            InfoBarMessage = "PlaylistTitleAlreadyExistsMessage".GetLocalized();
            args.Cancel = true;
        }
        else
        {
            ShowInfoBar = false;
        }
    }
}
