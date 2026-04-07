using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="PlaylistItem"/> 序列提供服务的适配器。
/// </summary>
/// <param name="playlistItems">指定的 <see cref="PlaylistItem"/> 序列实例。</param>
public sealed class PlaylistItemSequenceAdapter(IEnumerable<PlaylistItem> playlistItems) : IPlayable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        foreach (PlaylistItem item in playlistItems.ToArray())
        {
            yield return item.SongCid;
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
