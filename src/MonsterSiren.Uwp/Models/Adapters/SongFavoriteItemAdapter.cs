using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="SongFavoriteItem"/> 提供服务的适配器。
/// </summary>
/// <param name="songFavoriteItem">指定的 <see cref="SongFavoriteItem"/> 实例。</param>
public sealed class SongFavoriteItemAdapter(SongFavoriteItem songFavoriteItem) : ISongCidProvider, ICorruptible, IFavoriteAddable, INameProvider
{
    public string Name => songFavoriteItem.SongTitle;

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

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        await FavoriteService.AddSongToFavoriteAsync(songFavoriteItem);
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveSongFromFavoriteAsync(songFavoriteItem);
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
