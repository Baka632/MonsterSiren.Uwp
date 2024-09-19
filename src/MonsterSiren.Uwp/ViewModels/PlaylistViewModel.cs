namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistViewModel : ObservableObject
{
    [ObservableProperty]
    private Playlist selectedPlaylist;

    [RelayCommand]
    private static async Task CreateNewPlaylist()
    {
        await CommonValues.CreatePlaylist();
    }

    [RelayCommand]
    private static async Task PlayPlaylist(Playlist playlist)
    {
        await CommonValues.StartPlay(playlist);
    }

    [RelayCommand]
    private static async Task AddToNowPlaying(Playlist playlist)
    {
        await CommonValues.AddToNowPlaying(playlist);
    }

    [RelayCommand]
    private async Task AddPlaylistToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedPlaylist);
    }

    [RelayCommand]
    private static async Task ModifyPlaylist(Playlist playlist)
    {
        await CommonValues.ModifyPlaylist(playlist);
    }

    [RelayCommand]
    private static async Task RemovePlaylist(Playlist playlist)
    {
        await CommonValues.RemovePlaylist(playlist);
    }
}