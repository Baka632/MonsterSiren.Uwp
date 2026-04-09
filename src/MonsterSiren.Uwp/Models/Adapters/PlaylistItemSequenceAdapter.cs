using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="PlaylistItem"/> 序列提供服务的适配器。
/// </summary>
/// <param name="playlistItems">指定的 <see cref="PlaylistItem"/> 序列实例。</param>
public sealed class PlaylistItemSequenceAdapter(IEnumerable<PlaylistItem> playlistItems) : IPlayable, IFavoriteAddable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        foreach (PlaylistItem item in playlistItems.ToArray())
        {
            yield return item.SongCid;
        }
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        await FavoriteService.AddSongsToFavoriteAsync(GetAsyncEnumerable());
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveSongsFromFavoriteAsync(GetAsyncEnumerable());
    }

    private async IAsyncEnumerable<SongFavoriteItem> GetAsyncEnumerable()
    {
        foreach (PlaylistItem playlistItem in playlistItems.ToArray())
        {
            SongFavoriteItem item = new(playlistItem.SongCid,
                   playlistItem.AlbumCid,
                   playlistItem.SongTitle,
                   playlistItem.AlbumTitle,
                   playlistItem.SongDuration);
            yield return item;
        }
    }
}

/// <summary>
/// 为 <see cref="PlaylistItemSequenceAdapter"/> 提供扩展方法的类。
/// </summary>
public static class PlaylistItemSequenceAdapterExtensions
{
    extension(IEnumerable<PlaylistItem> playlistItems)
    {
        /// <summary>
        /// 使用 <see cref="PlaylistItem"/> 序列获得一个 <see cref="PlaylistItemSequenceAdapter"/>。
        /// </summary>
        /// <param name="playlist">项目所属的播放列表。</param>
        /// <returns>转换后的 <see cref="PlaylistItemSequenceAdapter"/>。</returns>
        public PlaylistItemSequenceAdapter ToAdapter() => new(playlistItems);
    }
}
