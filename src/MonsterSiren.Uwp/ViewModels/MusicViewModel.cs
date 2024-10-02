using System.Net.Http;
using System.Threading;
using Microsoft.Toolkit.Collections;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MusicPage"/> 提供视图模型
/// </summary>
public sealed partial class MusicViewModel : ObservableObject
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

    public async Task Initialize()
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;
        try
        {
            if (MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.TryGetData(CommonValues.AlbumInfoCacheKey, out CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> infos))
            {
                Albums = infos;
            }
            else
            {
                IEnumerable<AlbumInfo> albums = await GetAlbumsFromServer();
                Albums = CreateIncrementalLoadingCollection(albums);
                MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.Store(CommonValues.AlbumInfoCacheKey, Albums);
            }

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
            IEnumerable<AlbumInfo> albumInfos = await GetAlbumsFromServer();

            if (Albums is null || !Albums.CollectionSource.AlbumInfos.SequenceEqual(albumInfos))
            {
                Albums = CreateIncrementalLoadingCollection(albumInfos);
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

    private static CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> CreateIncrementalLoadingCollection(IEnumerable<AlbumInfo> albums)
    {
        int loadCount = EnvironmentHelper.IsWindowsMobile ? 5 : 10;
        return new(new AlbumInfoSource(albums), loadCount);
    }

    private async static Task<IEnumerable<AlbumInfo>> GetAlbumsFromServer()
    {
        List<AlbumInfo> albums = await Task.Run(async () =>
        {
            List<AlbumInfo> albumList = (await AlbumService.GetAllAlbumsAsync()).ToList();
            await MsrModelsHelper.TryFillArtistAndCachedCoverForAlbum(albumList);

            return albumList;
        });

        return albums;
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