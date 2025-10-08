using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 提供视图模型。
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

    public async Task Initialize(AlbumDetail albumDetail)
    {
        IsLoading = true;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;

        try
        {
            CurrentAlbumInfo = (await CommonValues.GetOrFetchAlbums()).CollectionSource.AlbumInfos
                .Single(info => info.Cid == albumDetail.Cid);

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
    private async Task PlayNextForSongInfo(SongInfo songInfo)
    {
        await CommonValues.PlayNext(songInfo, CurrentAlbumDetail);
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
        view.SongList.SelectRange(new ItemIndexRange(0, (uint)CurrentAlbumDetail.Songs.Count()));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentAlbumDetail.Songs.Count()));
    }

    [RelayCommand]
    private async Task PlayForListViewSelectedItem()
    {
        List<SongInfo> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.StartPlay(selectedItems, CurrentAlbumDetail);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddToNowPlayingForListViewSelectedItem()
    {
        List<SongInfo> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems, CurrentAlbumDetail);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task PlayNextForListViewSelectedItem()
    {
        List<SongInfo> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.PlayNext(selectedItems, CurrentAlbumDetail);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistForListViewSelectedItem(Playlist playlist)
    {
        List<SongInfo> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        await CommonValues.AddToPlaylist(playlist, selectedItems, CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task DownloadForListViewSelectedItem()
    {
        List<SongInfo> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.StartDownload(selectedItems, CurrentAlbumDetail);

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    private List<SongInfo> GetSelectedItem(ListView listView)
    {
        List<SongInfo> selectedItems = new(5);

        foreach (ItemIndexRange range in listView.SelectedRanges)
        {
            selectedItems.AddRange(CurrentAlbumDetail.Songs.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }
}