using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;

namespace MonsterSiren.Uwp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string downloadPath = DownloadService.DownloadPath;
    [ObservableProperty]
    private bool isFolderRedirected = DownloadService.DownloadPathRedirected;
    [ObservableProperty]
    private bool downloadLryic = DownloadService.DownloadLyric;
    [ObservableProperty]
    private bool transcodeDownloadedMusic = DownloadService.TranscodeDownloadedItem;

    partial void OnDownloadLryicChanged(bool value)
    {
        DownloadService.DownloadLyric = value;
    }

    partial void OnTranscodeDownloadedMusicChanged(bool value)
    {
        DownloadService.TranscodeDownloadedItem = value;
    }

    [RelayCommand]
    private async Task PickDownloadFolderAsync()
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        StorageFolder downloadFolder = await folderPicker.PickSingleFolderAsync();

        if (downloadFolder is null)
        {
            // 用户取消了文件夹选择
            return;
        }

        StorageApplicationPermissions.FutureAccessList.AddOrReplace(CommonValues.MusicDownloadPathSettingsKey, downloadFolder);
        DownloadService.DownloadPath = DownloadPath = downloadFolder.Path;
        IsFolderRedirected = false;
    }
}