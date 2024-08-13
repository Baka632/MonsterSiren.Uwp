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
    private static async Task PlayForItem(PlaylistItem item)
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
    private static async Task AddItemToNowPlaying(PlaylistItem item)
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
    private async Task AddItemToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedItem);
    }

    [RelayCommand]
    private async Task AddCurrentPlaylistToAnotherPlaylistCommand(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, CurrentPlaylist);
    }

    [RelayCommand]
    private static async Task DownloadForItem(PlaylistItem item)
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
            await DownloadForItem(item);
        }
    }

    [RelayCommand]
    private void RemoveItemFromPlaylist(PlaylistItem item)
    {
        PlaylistService.RemoveItemForPlaylist(CurrentPlaylist, item);
    }

    [RelayCommand]
    private async Task ModifyPlaylist()
    {
        PlaylistInfoDialog dialog = new()
        {
            Title = "PlaylistModifyTitle".GetLocalized(),
            PrimaryButtonText = "PlaylistModifyPrimaryButtonText".GetLocalized(),
            PlaylistTitle = CurrentPlaylist.Title,
            PlaylistDescription = CurrentPlaylist.Description,
            CheckDuplicatePlaylist = false,
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.ModifyPlaylistAsync(CurrentPlaylist, dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }

    [RelayCommand]
    private async Task RemovePlaylist()
    {
        ContentDialogResult result = await CommonValues.DisplayContentDialog("EnsureDelete".GetLocalized(), "",
            "OK".GetLocalized(), "Cancel".GetLocalized());

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.RemovePlaylistAsync(CurrentPlaylist);
            ContentFrameNavigationHelper.GoBack();
        }
    }
}