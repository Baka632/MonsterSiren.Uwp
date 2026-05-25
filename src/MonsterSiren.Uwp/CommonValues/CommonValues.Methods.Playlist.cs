using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 显示新建播放列表的对话框。
    /// </summary>
    public static async Task ShowCreatePlaylistDialog()
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

    /// <summary>
    /// 显示修改播放列表的对话框。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    public static async Task<bool> ShowModifyPlaylistDialog(Playlist playlist)
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
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 移除指定的播放列表。
    /// </summary>
    /// <remarks>
    /// 在移除指定的播放列表之前，会显示再次确认的对话框。
    /// </remarks>
    /// <param name="playlist">目标播放列表。</param>
    public static async Task<bool> RemovePlaylist(Playlist playlist)
    {
        ContentDialogResult result = await DisplayContentDialog("EnsureDelete".GetLocalized(), "", "OK".GetLocalized(),
                                                "Cancel".GetLocalized());

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.RemovePlaylistAsync(playlist);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 移除指定的播放列表序列。
    /// </summary>
    /// <remarks>
    /// 在移除指定的播放列表之前，会显示再次确认的对话框。
    /// </remarks>
    /// <param name="playlist">目标播放列表序列。</param>
    public static async Task<bool> RemovePlaylists(IEnumerable<Playlist> playlists)
    {
        if (!playlists.Any())
        {
            return false;
        }

        ContentDialogResult result = await DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                    "DeleteMultiplePlaylistsMessage".GetLocalized(),
                                                    "OK".GetLocalized(), "Cancel".GetLocalized());

        if (result == ContentDialogResult.Primary)
        {
            foreach (Playlist playlist in playlists)
            {
                await PlaylistService.RemovePlaylistAsync(playlist);
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
