namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistViewModel : ObservableObject
{
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
            PlaylistService.CreateNewPlaylist(dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }

    [RelayCommand]
    private static async Task PlayPlaylist(Playlist playlist)
    {
        await PlaylistService.PlayForPlaylistAsync(playlist);
    }

    [RelayCommand]
    private static async Task AddToNowPlaying(Playlist playlist)
    {
        await PlaylistService.AddPlaylistToNowPlayingAsync(playlist);
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
            CheckDuplicatePlaylist = false,
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            playlist.Title = dialog.PlaylistTitle;
            playlist.Description = dialog.PlaylistDescription;
        }
    }

    [RelayCommand]
    private static async Task RemovePlaylist(Playlist playlist)
    {
        ContentDialogResult result = await CommonValues.DisplayContentDialog("EnsureDelete".GetLocalized(), "",
            "OK".GetLocalized(), "Cancel".GetLocalized());

        if (result == ContentDialogResult.Primary)
        {
            PlaylistService.RemovePlaylist(playlist);
        }
    }
}