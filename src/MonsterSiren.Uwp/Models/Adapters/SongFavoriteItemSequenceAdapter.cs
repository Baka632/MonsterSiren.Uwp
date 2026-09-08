using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="SongFavoriteItem"/> 序列提供服务的适配器。
/// </summary>
/// <param name="songFavoriteItems">指定的 <see cref="SongFavoriteItem"/> 序列实例。</param>
public sealed class SongFavoriteItemSequenceAdapter(IEnumerable<SongFavoriteItem> songFavoriteItems) : ISongCidProvider, IFavoriteAddable, IContentContainer
{
    public bool IsEmpty => !songFavoriteItems.Any();
    public int Count => songFavoriteItems.Count();

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        foreach (SongFavoriteItem item in songFavoriteItems.ToArray())
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
        foreach (SongFavoriteItem item in songFavoriteItems)
        {
            yield return item;
        }
    }
}

/// <summary>
/// 为 <see cref="SongFavoriteItemSequenceAdapter"/> 提供扩展方法的类。
/// </summary>
public static class SongFavoriteItemSequenceAdapterExtensions
{
    extension(IEnumerable<SongFavoriteItem> songFavoriteItems)
    {
        /// <summary>
        /// 使用 <see cref="SongFavoriteItem"/> 序列获得一个 <see cref="SongFavoriteItemSequenceAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="SongFavoriteItemSequenceAdapter"/>。</returns>
        public SongFavoriteItemSequenceAdapter ToAdapter() => new(songFavoriteItems);
    }
}