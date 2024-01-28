// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class DownloadPage : Page
{
    public DownloadViewModel ViewModel { get; } = new DownloadViewModel();

    public DownloadPage()
    {
        this.InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DownloadService.DownloadList.CollectionChanged += OnCollectionChanged;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DownloadService.DownloadList.CollectionChanged -= OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (DownloadService.DownloadList.Count > 0)
        {
            ViewModel.IsDownloadListEmpty = false;
        }
        else
        {
            ViewModel.IsDownloadListEmpty = true;
        }
    }

    internal static bool IsDownloadItemPaused(DownloadItemState state)
    {
        return state == DownloadItemState.Paused;
    }
    
    internal static Visibility IsDownloadItemPausedReturnVisibility(DownloadItemState state)
    {
        return XamlHelper.ToVisibility(state == DownloadItemState.Paused);
    }
    
    internal static Visibility IsDownloadItemPausedReverseVisibility(DownloadItemState state)
    {
        return XamlHelper.ReverseVisibility(state == DownloadItemState.Paused);
    }

    internal static bool IsPausableOrResumable(DownloadItemState state)
    {
        return state switch
        {
            DownloadItemState.Downloading or DownloadItemState.Paused => true,
            _ => false
        };
    }

    internal static bool IsError(DownloadItemState state)
    {
        return state == DownloadItemState.Error;
    }
    
    internal static bool IsDisplayAsIndeterminate(DownloadItemState state)
    {
        return state is DownloadItemState.WritingTag or DownloadItemState.Error;
    }
}
