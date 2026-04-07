using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="SongFavoriteItem"/> 提供服务的适配器。
/// </summary>
/// <param name="songFavoriteItem">指定的 <see cref="SongFavoriteItem"/> 实例。</param>
public sealed class SongFavoriteItemAdapter(SongFavoriteItem songFavoriteItem) : IPlayable, ICorruptible
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        yield return songFavoriteItem.SongCid;
    }

    public void MarkAsCorrupted()
    {
        int targetIndex = FavoriteService.SongFavoriteList.Items.IndexOf(songFavoriteItem);
        if (targetIndex != -1)
        {
            FavoriteService.SongFavoriteList.Items[targetIndex] = songFavoriteItem with { IsCorruptedItem = true };
        }
    }
}

/// <summary>
/// 为 <see cref="SongFavoriteItemAdapter"/> 提供扩展方法的类。
/// </summary>
public static class SongFavoriteItemAdapterExtensions
{
    extension(SongFavoriteItem songFavoriteItem)
    {
        /// <summary>
        /// 使用 <see cref="SongFavoriteItem"/> 获得一个 <see cref="SongFavoriteItemAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="SongFavoriteItemAdapter"/>。</returns>
        public SongFavoriteItemAdapter ToAdapter() => new(songFavoriteItem);
    }
}
