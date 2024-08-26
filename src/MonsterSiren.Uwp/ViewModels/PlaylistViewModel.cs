namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistViewModel : ObservableObject
{
    [ObservableProperty]
    private Playlist selectedPlaylist;

    [RelayCommand]
    private static async Task CreateNewPlaylist()
    {
        PlaylistInfoDialog dialog = new()
        {
            Title = "PlaylistCreationTitle".GetLocalized(),
            PrimaryButtonText = "PlaylistCreationPrimaryButtonText".GetLocalized()
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.CreateNewPlaylistAsync(dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }

    [RelayCommand]
    private static async Task PlayPlaylist(Playlist playlist)
    {
        if (playlist.SongCount == 0)
        {
            await CommonValues.DisplayContentDialog("NoSongPlayed_Title".GetLocalized(),
                                        "NoSongPlayed_PlaylistEmpty".GetLocalized(),
                                        "OK".GetLocalized());
        }
        else
        {
            await PlaylistService.PlayForPlaylistAsync(playlist);
        }
    }

    [RelayCommand]
    private static async Task AddToNowPlaying(Playlist playlist)
    {
        await PlaylistService.AddPlaylistToNowPlayingAsync(playlist);
    }

    [RelayCommand]
    private async Task AddPlaylistToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedPlaylist);
    }

    [RelayCommand]
    private static async Task ModifyPlaylist(Playlist playlist)
    {
        PlaylistInfoDialog dialog = new()
        {
            Title = "PlaylistModifyTitle".GetLocalized(),
            PrimaryButtonText = "PlaylistModifyPrimaryButtonText".GetLocalized(),
            PlaylistTitle = playlist.Title,
            PlaylistDescription = playlist.Description,
            TargetPlaylist = playlist,
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.ModifyPlaylistAsync(playlist, dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }

    [RelayCommand]
    private static async Task RemovePlaylist(Playlist playlist)
    {
        ContentDialogResult result = await CommonValues.DisplayContentDialog("EnsureDelete".GetLocalized(), "",
            "OK".GetLocalized(), "Cancel".GetLocalized());

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.RemovePlaylistAsync(playlist);
        }
    }
}