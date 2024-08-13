using System.Net.Http;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 提供视图模型
/// </summary>
public partial class AlbumDetailViewModel : ObservableObject
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

    public async Task Initialize(AlbumInfo albumInfo)
    {
        IsLoading = true;
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
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            await Task.Run(async () =>
            {
                List<MediaPlaybackItem> items = new(CurrentAlbumDetail.Songs.Count());

                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    items.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
                }

                if (items.Count != 0)
                {
                    MusicService.ReplaceMusic(items);
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
    private async Task AddToNowPlayingForCurrentAlbumDetail()
    {
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        try
        {
            await Task.Run(async () =>
            {
                List<MediaPlaybackItem> playbackItems = new(CurrentAlbumDetail.Songs.Count());
                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    playbackItems.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
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
    private async Task AddToPlaylistForCurrentAlbumDetail(Playlist playlist)
    {
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        try
        {
            await Task.Run(async () =>
            {
                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, CurrentAlbumDetail);
                }
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task DownloadForCurrentAlbumDetail()
    {
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        try
        {
            await Task.Run(async () =>
            {
                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    _ = DownloadService.DownloadSong(CurrentAlbumDetail, songDetail);
                }
            });
        }
        catch (HttpRequestException)
        {
        }
    }

    [RelayCommand]
    private async Task PlayForSongInfo(SongInfo songInfo)
    {
        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                MusicService.ReplaceMusic(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
            });
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToNowPlayingForSongInfo(SongInfo songInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                MusicService.AddMusic(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistForSongInfo(Playlist playlist)
    {
        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(SelectedSongInfo.Cid).ConfigureAwait(false);
                await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, CurrentAlbumDetail);
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task DownloadForSongInfo(SongInfo songInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                _ = DownloadService.DownloadSong(CurrentAlbumDetail, songDetail);
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }
}