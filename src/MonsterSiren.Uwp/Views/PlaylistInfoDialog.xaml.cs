// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace MonsterSiren.Uwp.Views;

[INotifyPropertyChanged]
public sealed partial class PlaylistInfoDialog : ContentDialog
{
    private static readonly char[] invalidChars = Path.GetInvalidFileNameChars();

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
    [ObservableProperty]
    private bool showRenameInFileSystemWarning;

    public Playlist TargetPlaylist { get; set; }

    public PlaylistInfoDialog()
    {
        this.InitializeComponent();
    }

    partial void OnPlaylistTitleChanging(string value)
    {
        if (!string.IsNullOrWhiteSpace(PlaylistTitle))
        {
            ShowInfoBar = false;
        }

        foreach (char item in invalidChars)
        {
            if (value.Contains(item))
            {
                ShowRenameInFileSystemWarning = true;
                return;
            }
        }

        ShowRenameInFileSystemWarning = false;
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
        else if (PlaylistService.TotalPlaylists.Any(item => item != TargetPlaylist && item.Title == PlaylistTitle))
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
