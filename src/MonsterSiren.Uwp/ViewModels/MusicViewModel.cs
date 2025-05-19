using System.Net.Http;
using System.Threading;
using Microsoft.Toolkit.Collections;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MusicPage"/> 提供视图模型
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
    private AlbumInfo selectedAlbumInfo;
    [ObservableProperty]
    private FlyoutBase selectedAlbumInfoContextFlyout;

    private readonly int tooManyItemThresholdCount = EnvironmentHelper.IsWindowsMobile ? 5 : 10;

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
                await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
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
        await CommonValues.StartPlay(albumInfo);
    }

    [RelayCommand]
    private static async Task AddToNowPlayingForAlbumInfo(AlbumInfo albumInfo)
    {
        await CommonValues.AddToNowPlaying(albumInfo);
    }

    [RelayCommand]
    private async Task AddAlbumInfoToPlaylist(Playlist playlist)
    {
        await CommonValues.AddToPlaylist(playlist, SelectedAlbumInfo);
    }

    [RelayCommand]
    private static async Task DownloadForAlbumInfo(AlbumInfo albumInfo)
    {
        await CommonValues.StartDownload(albumInfo);
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

        bool isSuccess = await CommonValues.StartPlay(selectedItems);

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

        bool isSuccess = await CommonValues.AddToNowPlaying(selectedItems);

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

        if (selectedItems.Count >= tooManyItemThresholdCount)
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

        await CommonValues.AddToPlaylist(playlist, selectedItems);
    }

    [RelayCommand]
    private async Task DownloadForSelectedItem()
    {
        List<AlbumInfo> selectedItems = GetSelectedItems(view.ContentGridView);

        if (selectedItems.Count == 0)
        {
            return;
        }

        if (selectedItems.Count >= tooManyItemThresholdCount)
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

        bool isSuccess = await CommonValues.StartDownload(selectedItems);

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