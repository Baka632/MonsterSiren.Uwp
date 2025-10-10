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
    public static async Task ShowModifyPlaylistDialog(Playlist playlist)
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

    /// <summary>
    /// 移除指定的播放列表。
    /// </summary>
    /// <remarks>
    /// 在移除指定的播放列表之前，会显示再次确认的对话框。
    /// </remarks>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="suppressWarning">指示是否要取消删除警告的值。</param>
    public static async Task RemovePlaylist(Playlist playlist, bool suppressWarning = false)
    {
        ContentDialogResult result = !suppressWarning
            ? await DisplayContentDialog("EnsureDelete".GetLocalized(), "", "OK".GetLocalized(),
                                                "Cancel".GetLocalized())
            : ContentDialogResult.None;

        if (suppressWarning || result == ContentDialogResult.Primary)
        {
            await PlaylistService.RemovePlaylistAsync(playlist);
        }
    }
}
