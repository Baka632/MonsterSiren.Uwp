using System.Net.Http;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
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
    [NotifyPropertyChangedFor(nameof(IsSelectedSongInfoContainsInFavorite))]
    private SongInfo selectedSongInfo;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public bool IsSelectedSongInfoContainsInFavorite { get => FavoriteService.ContainsSong(SelectedSongInfo); }

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
            // 先用比较准确的，计算出来的 AlbumInfo（不那么准确的地方在专辑艺术家这里，笨笨 yj 的锅）。
            // 如果这里不先顶上，那么会出现异常，
            // 因为查询准确的 AlbumInfo 是异步操作，因此 UI 线程在查询过程中会先去处理 UI 的其他事情，
            // 而由于 AlbumInfo 的内容为空，视图方面相关操作会出现问题。
            CurrentAlbumInfo = new(albumDetail.Cid,
                                   albumDetail.Name,
                                   albumDetail.Intro,
                                   albumDetail.Belong,
                                   albumDetail.CoverUrl,
                                   albumDetail.CoverDeUrl,
                                   [.. albumDetail.Songs.SelectMany(info => info.Artists).Distinct()]);
            
            CurrentAlbumDetail = albumDetail;
            IsSongsEmpty = CurrentAlbumDetail.Songs.Any() != true;

            // 之后再去查完全准确的 AlbumInfo
            CurrentAlbumInfo = (await CommonValues.GetOrFetchAlbums()).CollectionSource.AlbumInfos
                .Single(info => info.Cid == albumDetail.Cid);

            ErrorVisibility = Visibility.Collapsed;
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

    public async Task Initialize(AlbumFavoriteItem favoriteItem)
    {
        IsLoading = true;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;

        try
        {
            CurrentAlbumInfo = new(favoriteItem.AlbumCid,
                                   favoriteItem.AlbumName,
                                   string.Empty,
                                   string.Empty,
                                   string.Empty,
                                   string.Empty,
                                   favoriteItem.Artistes);
            
            CurrentAlbumDetail = await MsrModelsHelper.GetAlbumDetailAsync(favoriteItem.AlbumCid);
            IsSongsEmpty = CurrentAlbumDetail.Songs.Any() != true;

            // 之后再去查完全准确的 AlbumInfo
            CurrentAlbumInfo = (await CommonValues.GetOrFetchAlbums()).CollectionSource.AlbumInfos
                .Single(info => info.Cid == favoriteItem.AlbumCid);

            ErrorVisibility = Visibility.Collapsed;
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
        await CommonValues.StartPlay(CurrentAlbumDetail.ToAdapter());
    }

    [RelayCommand]
    private async Task AddToNowPlayingForCurrentAlbumDetail()
    {
        await CommonValues.AddToNowPlaying(CurrentAlbumDetail.ToAdapter());
    }

    [RelayCommand]
    private async Task AddToPlaylistForCurrentAlbumDetail(Playlist playlist)
    {
        await CommonValues.AddToPlaylist(playlist, CurrentAlbumDetail);
    }

    [RelayCommand]
    private async Task DownloadForCurrentAlbumDetail()
    {
        await CommonValues.StartDownload(CurrentAlbumDetail.ToAdapter());
    }

    [RelayCommand]
    private static async Task PlayForSongInfo(SongInfo songInfo)
    {
        await CommonValues.StartPlay(songInfo.ToAdapter());
    }

    [RelayCommand]
    private static async Task AddToNowPlayingForSongInfo(SongInfo songInfo)
    {
        await CommonValues.AddToNowPlaying(songInfo.ToAdapter());
    }
    
    [RelayCommand]
    private async Task PlayNextForSongInfo(SongInfo songInfo)
    {
        await CommonValues.PlayNext(songInfo.ToAdapter());
    }

    [RelayCommand]
    private async Task AddSongToFavorite(SongInfo songInfo)
    {
        await CommonValues.AddToFavorite(songInfo.ToAdapter());
        OnPropertyChanged(nameof(IsSelectedSongInfoContainsInFavorite));
    }

    [RelayCommand]
    private async Task RemoveSongFromFavorite(SongInfo songInfo)
    {
        await CommonValues.RemoveFromFavorite(songInfo.ToAdapter());
        OnPropertyChanged(nameof(IsSelectedSongInfoContainsInFavorite));
    }

    [RelayCommand]
    private async Task AddToPlaylistForSongInfo(Playlist playlist)
    {
        await CommonValues.AddToPlaylist(playlist, SelectedSongInfo, CurrentAlbumDetail);
    }

    [RelayCommand]
    private static async Task DownloadForSongInfo(SongInfo songInfo)
    {
        await CommonValues.StartDownload(songInfo.ToAdapter());
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

        bool isSuccess = await CommonValues.StartPlay(selectedItems.ToAdapter());

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

        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems.ToAdapter());

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

        bool isSuccess = await CommonValues.PlayNext(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSongsToFavoriteForListViewSelectedItem()
    {
        List<SongInfo> selectedItems = GetSelectedItem(view.SongList);

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool isSuccess = await CommonValues.AddToFavorite(selectedItems.ToAdapter());

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

        bool isSuccess = await CommonValues.StartDownload(selectedItems.ToAdapter());

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