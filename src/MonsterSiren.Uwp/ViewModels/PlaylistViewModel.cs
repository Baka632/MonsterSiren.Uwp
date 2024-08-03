namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistViewModel : ObservableObject
{
    [RelayCommand]
    private static async Task CreateNewPlaylist()
    {
        PlaylistCreationDialog dialog = new();
        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            PlaylistService.CreateNewPlaylist(dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }
}