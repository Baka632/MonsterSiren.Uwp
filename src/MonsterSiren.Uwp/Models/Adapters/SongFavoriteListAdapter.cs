using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

public sealed class SongFavoriteListAdapter(SongFavoriteList songFavorite) : ISongCidProvider, IContentCorruptible, IContentContainer
{
    public bool IsEmpty => songFavorite.Count == 0;
    public int Count => songFavorite.Count;

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        foreach (SongFavoriteItem item in songFavorite)
        {
            yield return item.SongCid;
        }
    }

    public void MarkItemAsCorrupted(string cid)
    {
        SongFavoriteItem item = songFavorite.Items.First(item => item.SongCid == cid);
        int targetIndex = songFavorite.Items.IndexOf(item);
        if (targetIndex != -1)
        {
            songFavorite.Items[targetIndex] = item with { IsCorruptedItem = true };
        }
    }
}

/// <summary>
/// 为 <see cref="SongFavoriteListAdapter"/> 提供扩展方法的类。
/// </summary>
public static class SongFavoriteListAdapterExtensions
{
    extension(SongFavoriteList songFavorite)
    {
        /// <summary>
        /// 使用 <see cref="SongFavoriteList"/> 获得一个 <see cref="SongFavoriteListAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="SongFavoriteListAdapter"/>。</returns>
        public SongFavoriteListAdapter ToAdapter() => new(songFavorite);
    }
}
