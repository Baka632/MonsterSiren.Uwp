using Windows.Storage;
using Windows.System;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="DownloadPage"/> 提供视图模型。
/// </summary>
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
        if (item.State is DownloadItemState.Canceled or DownloadItemState.Error or DownloadItemState.Done or DownloadItemState.Skipped)
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

        RemoveAllSkippedDownload();
    }

    [RelayCommand]
    private static void RemoveAllSkippedDownload()
    {
        DownloadItem[] skippedItem = [.. DownloadService.DownloadList.Where(item => item.State == DownloadItemState.Skipped)];

        foreach (DownloadItem item in skippedItem)
        {
            DownloadService.DownloadList.Remove(item);
        }
    }

    [RelayCommand]
    private static async Task OpenDownloadFolder()
    {
        StorageFolder downloadFolder = await StorageFolder.GetFolderFromPathAsync(DownloadService.DownloadPath);
        await Launcher.LaunchFolderAsync(downloadFolder);
    }
}