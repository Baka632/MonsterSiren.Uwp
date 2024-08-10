using System.Net.Http;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Playlist currentPlaylist;
    [ObservableProperty]
    private SongDetailAndAlbumDetailPack selectedPack;

    public void Initialize(Playlist model)
    {
        CurrentPlaylist = model ?? throw new ArgumentNullException(nameof(model));
    }

    [RelayCommand]
    private async Task PlayForCurrentPlaylist()
    {
        await PlaylistService.PlayForPlaylistAsync(CurrentPlaylist);
    }

    [RelayCommand]
    private static async Task PlayForPack(SongDetailAndAlbumDetailPack pack)
    {
        try
        {
            (SongDetail songDetail, AlbumDetail albumDetail) = pack;

            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);
            await Task.Run(() => MusicService.ReplaceMusic(songDetail.ToMediaPlaybackItem(albumDetail)));
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private static async Task AddPackToNowPlaying(SongDetailAndAlbumDetailPack pack)
    {
        (SongDetail songDetail, AlbumDetail albumDetail) = pack;
        await Task.Run(() => MusicService.AddMusic(songDetail.ToMediaPlaybackItem(albumDetail)));
    }

    [RelayCommand]
    private async Task AddPackToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedPack);
    }

    [RelayCommand]
    private static void DownloadForPack(SongDetailAndAlbumDetailPack pack)
    {
        (SongDetail songDetail, AlbumDetail albumDetail) = pack;
        _ = DownloadService.DownloadSong(albumDetail, songDetail);
    }

    [RelayCommand]
    private void RemovePackFromPlaylist(SongDetailAndAlbumDetailPack pack)
    {
        PlaylistService.RemoveItemForPlaylist(CurrentPlaylist, pack);
    }
}