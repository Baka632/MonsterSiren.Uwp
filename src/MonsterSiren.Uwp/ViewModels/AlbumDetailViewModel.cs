using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 提供视图模型
/// </summary>
public partial class AlbumDetailViewModel(AlbumDetailPage view) : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private AlbumInfo _currentAlbumInfo;
    [ObservableProperty]
    private AlbumDetail _currentAlbumDetail;
    [ObservableProperty]
    private bool isSongsEmpty;
    [ObservableProperty]
    private SongInfo selectedSongInfo;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public async Task Initialize(AlbumInfo albumInfo)
    {
        IsLoading = true;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
        CurrentAlbumInfo = albumInfo;
        AlbumDetail albumDetail;

        try
        {
            albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);

            CurrentAlbumDetail = albumDetail;
            ErrorVisibility = Visibility.Collapsed;

            IsSongsEmpty = CurrentAlbumDetail.Songs.Any() != true;
        }
        catch (HttpRequestException ex)
        {
            ErrorVisibility = Visibility.Visible;
            ErrorInfo = new ErrorInfo()
            {
                Title = "ErrorOccurred".GetLocalized(),
                Message = "InternetErrorMessage".GetLocalized(),
                Exception = ex
            };
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PlayForCurrentAlbumDetail()
    {
        await CommonValues.StartPlay(CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task AddToNowPlayingForCurrentAlbumDetail()
    {
        await CommonValues.AddToNowPlaying(CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task AddToPlaylistForCurrentAlbumDetail(Playlist playlist)
    {
        await CommonValues.AddToPlaylist(playlist, CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task DownloadForCurrentAlbumDetail()
    {
        await CommonValues.StartDownload(CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task PlayForSongInfo(SongInfo songInfo)
    {
        await CommonValues.StartPlay(songInfo, CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task AddToNowPlayingForSongInfo(SongInfo songInfo)
    {
        await CommonValues.AddToNowPlaying(songInfo, CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task AddToPlaylistForSongInfo(Playlist playlist)
    {
        await CommonValues.AddToPlaylist(playlist, SelectedSongInfo, CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task DownloadForSongInfo(SongInfo songInfo)
    {
        await CommonValues.StartDownload(songInfo, CurrentAlbumDetail);
    }

    [RelayCommand]
    private static void CopySongNameToClipboard(SongInfo songInfo)
    {
        DataPackage package = new()
        {
            RequestedOperation = DataPackageOperation.Copy
        };
        package.SetText(songInfo.Name);
        Clipboard.SetContent(package);
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        // Single 模式只能选一个
        ItemIndexRange range = view.SongList.SelectedRanges.FirstOrDefault();

        view.SongList.SelectionMode = ListViewSelectionMode.Multiple;

        if (range is not null)
        {
            view.SongList.SelectRange(range);
        }

        SelectedSongListItemContextFlyout = view.SongSelectionFlyout;
    }

    [RelayCommand]
    private void StopMultipleSelection()
    {
        view.SongList.SelectionMode = ListViewSelectionMode.Single;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.SongList.SelectAll();
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentAlbumDetail.Songs.Count()));
    }

    [RelayCommand]
    private async Task PlayForListViewSelectedItem()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        IEnumerable<SongInfo> songInfos = selectedItems.Where(obj => obj is SongInfo).Select(obj => (SongInfo)obj);
        bool isSuccess = await CommonValues.StartPlay(songInfos, CurrentAlbumDetail);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddToNowPlayingForListViewSelectedItem()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        IEnumerable<SongInfo> songInfos = selectedItems.Where(obj => obj is SongInfo).Select(obj => (SongInfo)obj);
        bool isSuccess = await CommonValues.AddToNowPlaying(songInfos, CurrentAlbumDetail);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistForListViewSelectedItem(Playlist playlist)
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        IEnumerable<SongInfo> songInfos = selectedItems.Where(obj => obj is SongInfo).Select(obj => (SongInfo)obj);
        await CommonValues.AddToPlaylist(playlist, songInfos, CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task DownloadForListViewSelectedItem()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        IEnumerable<SongInfo> songInfos = selectedItems.Where(obj => obj is SongInfo).Select(obj => (SongInfo)obj);
        bool isSuccess = await CommonValues.StartDownload(songInfos, CurrentAlbumDetail);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }
}