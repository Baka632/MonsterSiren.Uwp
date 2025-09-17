using System.Net.Http;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private MsrIncrementalCollection<NewsInfo> _newsList;
    [ObservableProperty]
    private MsrIncrementalCollection<AlbumInfo> _albumList;
    [ObservableProperty]
    private bool isAlbumListEmpty;
    [ObservableProperty]
    private bool isNewsListEmpty;
    [ObservableProperty]
    private AlbumInfo selectedAlbumInfo;

    partial void OnAlbumListChanged(MsrIncrementalCollection<AlbumInfo> value)
    {
        IsAlbumListEmpty = !value.Any();
    }

    partial void OnNewsListChanged(MsrIncrementalCollection<NewsInfo> value)
    {
        IsNewsListEmpty = !value.Any();
    }

    public async Task Initialize(string keyword)
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;

        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            SearchAlbumAndNewsResult albumAndNewsWarpper = await SearchService.SearchAlbumAndNewsAsync(keyword);

            List<AlbumInfo> albumList = [.. albumAndNewsWarpper.Albums.List];

            if (await MsrModelsHelper.TryFillArtistAndCachedCoverForAlbum(albumList))
            {
                albumAndNewsWarpper = albumAndNewsWarpper with
                {
                    Albums = albumAndNewsWarpper.Albums with { List = albumList }
                };
            }

            NewsList = new MsrIncrementalCollection<NewsInfo>(albumAndNewsWarpper.News, async lastInfo => await SearchService.SearchNewsAsync(keyword, lastInfo.Cid));
            AlbumList = new MsrIncrementalCollection<AlbumInfo>(albumAndNewsWarpper.Albums, async lastInfo =>
            {
                ListPackage<AlbumInfo> listPackage = await SearchService.SearchAlbumAsync(keyword, lastInfo.Cid);
                List<AlbumInfo> list = [.. listPackage.List];

                return await MsrModelsHelper.TryFillArtistAndCachedCoverForAlbum(list)
                ? listPackage with { List = list }
                : listPackage;
            });

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
}