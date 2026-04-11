using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="Playlist"/> 提供服务的适配器。
/// </summary>
/// <param name="playlist">指定的 <see cref="Playlist"/> 实例。</param>
public sealed class PlaylistAdapter(Playlist playlist) : ISongCidProvider, IContentCorruptible, IContentContainer
{
    public bool IsEmpty => playlist.Items.Count == 0;

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        // 这里创建一个播放列表项目的复制，这样可以避免在标记为错误时出现集合被修改的异常。
        PlaylistItem[] items = [.. playlist.Items];
        foreach (PlaylistItem item in items)
        {
            yield return item.SongCid;
        }
    }

    public void MarkItemAsCorrupted(string cid)
    {
        PlaylistItem item = playlist.Items.First(playlistItem => playlistItem.SongCid == cid);
        int targetIndex = playlist.Items.IndexOf(item);
        playlist.Items[targetIndex] = item with { IsCorruptedItem = true };
    }
}

/// <summary>
/// 为 <see cref="PlaylistAdapter"/> 提供扩展方法的类。
/// </summary>
public static class PlaylistAdapterExtensions
{
    extension(Playlist playlist)
    {
        /// <summary>
        /// 使用 <see cref="Playlist"/> 获得一个 <see cref="PlaylistAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="PlaylistAdapter"/>。</returns>
        public PlaylistAdapter ToAdapter() => new(playlist);
    }
}