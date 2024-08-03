namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Playlist currentPlaylist;
    [ObservableProperty]
    private bool isPlaylistEmpty;

    public void Initialize(Playlist model)
    {
        CurrentPlaylist = model ?? throw new ArgumentNullException(nameof(model));

        IsPlaylistEmpty = CurrentPlaylist.Items.Count <= 0;
    }

    [RelayCommand]
    private async Task PlayForCurrentPlaylist()
    {
        await PlaylistService.PlayForPlaylistAsync(CurrentPlaylist);
    }
}