using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 将 <see cref="ISongCidProvider"/> 表示的内容添加到指定的播放列表中。
    /// </summary>
    /// <param name="playlist">目标播放列表。</param>
    /// <param name="provider"><see cref="ISongCidProvider"/> 实例。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, ISongCidProvider provider)
    {
        if (provider is IContentContainer container)
        {
            if (container.IsEmpty)
            {
                return false;
            }
            else if (container.Count >= TooManyItemThresholdCount)
            {
                ContentDialogResult result = await DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                        "AddTooManyItemToPlaylistMessage".GetLocalized(),
                                                        "Continue".GetLocalized(), "Cancel".GetLocalized());

                if (result != ContentDialogResult.Primary)
                {
                    return false;
                }
            }
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(provider, box);
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (AggregateException ex)
        {
            await DisplayAggregateExceptionErrorDialog(ex);
        }

        return false;
    }
}
