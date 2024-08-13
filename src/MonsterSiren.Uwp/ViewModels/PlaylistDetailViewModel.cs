using System.Net.Http;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Playlist currentPlaylist;
    [ObservableProperty]
    private PlaylistItem selectedItem;

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
    private static async Task PlayForPack(PlaylistItem item)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);

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
    private static async Task AddPackToNowPlaying(PlaylistItem item)
    {
        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
        AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);

        await Task.Run(() => MusicService.AddMusic(songDetail.ToMediaPlaybackItem(albumDetail)));
    }

    [RelayCommand]
    private async Task AddCurrentPlaylistToNowPlaying()
    {
        await PlaylistService.AddPlaylistToNowPlayingAsync(CurrentPlaylist);
    }

    [RelayCommand]
    private async Task AddPackToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedItem);
    }

    [RelayCommand]
    private async Task AddCurrentPlaylistToAnotherPlaylistCommand(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, CurrentPlaylist);
    }

    [RelayCommand]
    private static async Task DownloadForPack(PlaylistItem item)
    {
        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
        AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
        _ = DownloadService.DownloadSong(albumDetail, songDetail);
    }

    [RelayCommand]
    private async Task DownloadForCurrentPlaylist()
    {
        foreach (PlaylistItem item in CurrentPlaylist)
        {
            await DownloadForPack(item);
        }
    }

    [RelayCommand]
    private void RemovePackFromPlaylist(PlaylistItem item)
    {
        PlaylistService.RemoveItemForPlaylist(CurrentPlaylist, item);
    }
}