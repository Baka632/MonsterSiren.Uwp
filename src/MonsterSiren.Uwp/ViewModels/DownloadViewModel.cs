namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class DownloadViewModel : ObservableObject
{
    [RelayCommand]
    private static void PauseOrResumeDownload(DownloadItem item)
    {
        if (item.IsPaused)
        {
            item.ResumeDownload();
        }
        else
        {
            item.PauseDownload();
        }
    }
    
    [RelayCommand]
    private static void CancelDownload(DownloadItem item)
    {
        item.CancelToken.Cancel();
    }
}