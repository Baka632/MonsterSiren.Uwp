using Windows.Storage;
using Windows.System;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class DownloadViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isDownloadListEmpty = DownloadService.DownloadList.Count <= 0;

    [RelayCommand]
    private static void PauseOrResumeDownload(DownloadItem item)
    {
        if (item.State == DownloadItemState.Paused)
        {
            item.ResumeDownload();
        }
        else if (item.State == DownloadItemState.Downloading)
        {
            item.PauseDownload();
        }
    }
    
    [RelayCommand]
    private static void CancelOrRemoveDownload(DownloadItem item)
    {
        if (item.State == DownloadItemState.Canceled)
        {
            DownloadService.DownloadList.Remove(item);
        }
        else
        {
            item.CancelDownload();
        }
    }

    [RelayCommand]
    private static void PauseAllDownload()
    {
        foreach (DownloadItem item in DownloadService.DownloadList)
        {
            if (item.State == DownloadItemState.Downloading)
            {
                item.PauseDownload();
            }
        }
    }
    
    [RelayCommand]
    private static void ResumeAllDownload()
    {
        foreach (DownloadItem item in DownloadService.DownloadList)
        {
            if (item.State == DownloadItemState.Paused)
            {
                item.ResumeDownload();
            }
        }
    }
    
    [RelayCommand]
    private static void CancelAllDownload()
    {
        foreach (DownloadItem item in DownloadService.DownloadList)
        {
            item.CancelDownload();
        }
    }

    [RelayCommand]
    private static async Task OpenDownloadFolder()
    {
        StorageFolder asyncOperation = await StorageFolder.GetFolderFromPathAsync(DownloadService.DownloadPath);
        await Launcher.LaunchFolderAsync(asyncOperation);
    }
}