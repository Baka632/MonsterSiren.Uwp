using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="SongDetail"/> 提供服务的适配器。
/// </summary>
/// <param name="songDetail">指定的 <see cref="SongDetail"/> 实例。</param>
public sealed class SongDetailAdapter(SongDetail songDetail) : ISongCidProvider, IFavoriteAddable, INameProvider
{
    public string Name => songDetail.Name;

    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        yield return songDetail.Cid;
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        try
        {
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(songDetail.AlbumCid);
            TimeSpan duration = await MsrModelsHelper.GetSongDurationAsync(songDetail) ?? TimeSpan.Zero;
            SongFavoriteItem songFavoriteItem = new(songDetail.Cid, albumDetail.Cid, songDetail.Name, albumDetail.Name, duration);

            await FavoriteService.AddSongToFavoriteAsync(songFavoriteItem);
        }
        catch (Exception ex)
        {
            box.InboxException = ex;
        }
    }

    public async Task RemoveFromFavoriteAsync()
    {
        await FavoriteService.RemoveSongFromFavoriteAsync(songDetail.Cid);
    }
}

/// <summary>
/// 为 <see cref="SongDetailAdapter"/> 提供扩展方法的类。
/// </summary>
public static class SongDetailAdapterExtensions
{
    extension(SongDetail songDetail)
    {
        /// <summary>
        /// 使用 <see cref="SongDetail"/> 获得一个 <see cref="SongDetailAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="SongDetailAdapter"/>。</returns>
        public SongDetailAdapter ToAdapter() => new(songDetail);
    }
}
