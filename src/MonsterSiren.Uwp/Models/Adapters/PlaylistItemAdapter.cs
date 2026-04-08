using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="PlaylistItem"/> 提供服务的适配器。
/// </summary>
/// <param name="playlistItem">指定的 <see cref="PlaylistItem"/> 实例。</param>
/// <param name="sourcePlaylist"><paramref name="playlistItem"/> 所属的播放列表。</param>
public sealed class PlaylistItemAdapter(PlaylistItem playlistItem, Playlist sourcePlaylist) : IPlayable, ICorruptible, IFavoriteAddable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        yield return playlistItem.SongCid;
    }

    public void MarkAsCorrupted()
    {
        int targetIndex = sourcePlaylist.Items.IndexOf(playlistItem);
        if (targetIndex != -1)
        {
            sourcePlaylist.Items[targetIndex] = playlistItem with { IsCorruptedItem = true };
        }
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        SongFavoriteItem item = new(playlistItem.SongCid,
                   playlistItem.AlbumCid,
                   playlistItem.SongTitle,
                   playlistItem.AlbumTitle,
                   playlistItem.SongDuration);
        await FavoriteService.AddSongToFavoriteAsync(item);
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveSongFromFavoriteAsync(playlistItem.SongCid);
    }
}

/// <summary>
/// 为 <see cref="PlaylistItemAdapter"/> 提供扩展方法的类。
/// </summary>
public static class PlaylistItemAdapterExtensions
{
    extension(PlaylistItem playlistItem)
    {
        /// <summary>
        /// 使用 <see cref="PlaylistItem"/> 获得一个 <see cref="PlaylistItemAdapter"/>。
        /// </summary>
        /// <param name="playlist">项目所属的播放列表。</param>
        /// <returns>转换后的 <see cref="PlaylistItemAdapter"/>。</returns>
        public PlaylistItemAdapter ToAdapter(Playlist playlist) => new(playlistItem, playlist);
    }
}