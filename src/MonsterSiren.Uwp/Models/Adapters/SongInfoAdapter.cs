using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="SongInfo"/> 提供服务的适配器。
/// </summary>
/// <param name="songInfo">指定的 <see cref="SongInfo"/> 实例。</param>
public sealed class SongInfoAdapter(SongInfo songInfo) : ISongCidProvider, IFavoriteAddable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        yield return songInfo.Cid;
    }

    public async Task AddToFavoriteAsync(ExceptionBox box)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
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
        await FavoriteService.RemoveSongFromFavoriteAsync(songInfo.Cid);
    }
}

/// <summary>
/// 为 <see cref="SongInfoAdapter"/> 提供扩展方法的类。
/// </summary>
public static class SongInfoAdapterExtensions
{
    extension(SongInfo songInfo)
    {
        /// <summary>
        /// 使用 <see cref="SongInfo"/> 获得一个 <see cref="SongInfoAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="SongInfoAdapter"/>。</returns>
        public SongInfoAdapter ToAdapter() => new(songInfo);
    }
}
