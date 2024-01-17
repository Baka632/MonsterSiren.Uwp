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
}
