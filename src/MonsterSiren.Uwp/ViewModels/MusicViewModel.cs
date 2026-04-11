using System.Net.Http;
using System.Threading;
using Microsoft.Toolkit.Collections;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MusicPage"/> 提供视图模型。
/// </summary>
public sealed partial class MusicViewModel(MusicPage view) : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private bool isRefreshing = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> albums;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedAlbumInfoContainsInFavorite))]
    private AlbumInfo selectedAlbumInfo;
    [ObservableProperty]
    private FlyoutBase selectedAlbumInfoContextFlyout;

    public bool IsSelectedAlbumInfoContainsInFavorite { get => FavoriteService.ContainsAlbum(SelectedAlbumInfo); }

    public async Task Initialize()
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;
        SelectedAlbumInfoContextFlyout = view.AlbumContextFlyout;

        try
        {
            Albums = await CommonValues.GetOrFetchAlbums();
            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException ex)
        {
            ShowInternetError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshAlbums()
    {
        IsRefreshing = true;
        ErrorVisibility = Visibility.Collapsed;
        try
        {
            IEnumerable<AlbumInfo> albumInfos = await CommonValues.GetAlbumsFromServer();

            if (Albums is null || !Albums.CollectionSource.AlbumInfos.SequenceEqual(albumInfos))
            {
                Albums = CommonValues.CreateAlbumInfoIncrementalLoadingCollection(albumInfos);
                MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.Store(CommonValues.AlbumInfoCacheKey, Albums);
            }

            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException ex)
        {
            if (Albums is not null && Albums.Count > 0)
            {
                await CommonValues.DisplayInternetErrorDialog();
            }
            else
            {
                ShowInternetError(ex);
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void ShowInternetError(HttpRequestException ex)
    {
        ErrorVisibility = Visibility.Visible;
        ErrorInfo = new ErrorInfo()
        {
            Title = "ErrorOccurred".GetLocalized(),
            Message = "InternetErrorMessage".GetLocalized(),
            Exception = ex
        };
    }

    [RelayCommand]
    private static async Task PlayAlbumForAlbumInfo(AlbumInfo albumInfo)
    {
        await CommonValues.StartPlay(albumInfo.ToAdapter());
    }

    [RelayCommand]
    private static async Task AddToNowPlayingForAlbumInfo(AlbumInfo albumInfo)
    {
        await CommonValues.AddToNowPlaying(albumInfo.ToAdapter());
    }

    [RelayCommand]
    private static async Task PlayNextForAlbumInfo(AlbumInfo albumInfo)
    {
        await CommonValues.PlayNext(albumInfo.ToAdapter());
    }

    [RelayCommand]
    private async Task AddAlbumInfoToPlaylist(Playlist playlist)
    {
        await CommonValues.AddToPlaylist(playlist, SelectedAlbumInfo.ToAdapter());
    }

    [RelayCommand]
    private static async Task DownloadForAlbumInfo(AlbumInfo albumInfo)
    {
        await CommonValues.StartDownload(albumInfo.ToAdapter());
    }

    [RelayCommand]
    private async Task AddAlbumToFavorite(AlbumInfo info)
    {
        await CommonValues.AddToFavorite(info.ToAdapter());
        OnPropertyChanged(nameof(IsSelectedAlbumInfoContainsInFavorite));
    }

    [RelayCommand]
    private async Task RemoveAlbumFromFavorite(AlbumInfo info)
    {
        await CommonValues.RemoveFromFavorite(info.ToAdapter());
        OnPropertyChanged(nameof(IsSelectedAlbumInfoContainsInFavorite));
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        view.ContentGridView.SelectionMode = ListViewSelectionMode.Multiple;
        view.ContentGridView.IsItemClickEnabled = false;
        SelectedAlbumInfoContextFlyout = view.AlbumSelectionFlyout;
    }

    [RelayCommand]
    private void StopMultipleSelection()
    {
        view.ContentGridView.SelectionMode = ListViewSelectionMode.None;
        view.ContentGridView.IsItemClickEnabled = true;
        SelectedAlbumInfoContextFlyout = view.AlbumContextFlyout;
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.ContentGridView.SelectRange(new ItemIndexRange(0, (uint)Albums.CollectionSource.AlbumInfos.Count()));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.ContentGridView.DeselectRange(new ItemIndexRange(0, (uint)Albums.CollectionSource.AlbumInfos.Count()));
    }

    [RelayCommand]
    private async Task PlayAlbumForSelectedItem()
    {
        List<AlbumInfo> selectedItems = GetSelectedItems(view.ContentGridView);

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
    private async Task AddToNowPlayingForSelectedItem()
    {
        List<AlbumInfo> selectedItems = GetSelectedItems(view.ContentGridView);

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
    private async Task PlayNextForSelectedItem()
    {
        List<AlbumInfo> selectedItems = GetSelectedItems(view.ContentGridView);

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
    private async Task AddSelectedItemToPlaylist(Playlist playlist)
    {
        List<AlbumInfo> selectedItems = GetSelectedItems(view.ContentGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count >= CommonValues.TooManyItemThresholdCount)
        {
            ContentDialogResult result = await CommonValues.DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                    "AddTooManyItemToPlaylistMessage".GetLocalized(),
                                                    "Continue".GetLocalized(), "Cancel".GetLocalized());

            if (result != ContentDialogResult.Primary)
            {
                StopMultipleSelection();
                return;
            }
        }

        await CommonValues.AddToPlaylist(playlist, selectedItems.ToAdapter());
    }

    [RelayCommand]
    private async Task DownloadForSelectedItem()
    {
        List<AlbumInfo> selectedItems = GetSelectedItems(view.ContentGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count >= CommonValues.TooManyItemThresholdCount)
        {
            ContentDialogResult result = await CommonValues.DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                    "DownloadTooManyItemMessage".GetLocalized(),
                                                    "Continue".GetLocalized(), "Cancel".GetLocalized());

            if (result != ContentDialogResult.Primary)
            {
                StopMultipleSelection();
                return;
            }
        }

        bool isSuccess = await CommonValues.StartDownload(selectedItems.ToAdapter());

        if (isSuccess)
        {
            StopMultipleSelection();
        }
    }

    [RelayCommand]
    private async Task AddSelectedItemsToFavorite()
    {
        List<AlbumInfo> selectedItems = GetSelectedItems(view.ContentGridView);

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

    private List<AlbumInfo> GetSelectedItems(GridView gridView)
    {
        List<AlbumInfo> selectedItems = new(5);

        foreach (ItemIndexRange range in gridView.SelectedRanges)
        {
            selectedItems.AddRange(Albums.CollectionSource.AlbumInfos.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }
}

public class AlbumInfoSource(IEnumerable<AlbumInfo> infos) : IIncrementalSource<AlbumInfo>
{
    public IEnumerable<AlbumInfo> AlbumInfos { get; } = new List<AlbumInfo>(infos);

    public async Task<IEnumerable<AlbumInfo>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            return AlbumInfos.Skip(pageIndex * pageSize).Take(pageSize);
        }, cancellationToken);
    }
}