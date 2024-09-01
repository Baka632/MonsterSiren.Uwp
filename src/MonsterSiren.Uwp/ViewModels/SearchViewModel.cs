using System.Net.Http;
using Windows.Media.Playback;

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

            List<AlbumInfo> albumList = albumAndNewsWarpper.Albums.List.ToList();

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
                List<AlbumInfo> list = listPackage.List.ToList();

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
        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid).ConfigureAwait(false);
                List<MediaPlaybackItem> playbackItems = new(albumDetail.Songs.Count());

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                    playbackItems.Add(songDetail.ToMediaPlaybackItem(albumDetail));
                }

                if (playbackItems.Count != 0)
                {
                    MusicService.ReplaceMusic(playbackItems);
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
                }
            });
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private static async Task AddToNowPlayingForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid).ConfigureAwait(false);
                List<MediaPlaybackItem> playbackItems = new(albumDetail.Songs.Count());

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                    playbackItems.Add(songDetail.ToMediaPlaybackItem(albumDetail));
                }

                MusicService.AddMusic(playbackItems);
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddAlbumInfoToPlaylist(Playlist playlist)
    {
        try
        {
            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(SelectedAlbumInfo.Cid).ConfigureAwait(false);

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                    await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, albumDetail);
                }
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private static async Task DownloadForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid).ConfigureAwait(false);

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                    _ = DownloadService.DownloadSong(albumDetail, songDetail);
                }
            });
        }
        catch (HttpRequestException)
        {
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
}