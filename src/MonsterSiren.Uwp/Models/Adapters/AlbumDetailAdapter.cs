using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp.Models.Adapters;

/// <summary>
/// 为 <see cref="AlbumDetail"/> 提供服务的适配器。
/// </summary>
/// <param name="albumDetail">指定的 <see cref="AlbumDetail"/> 实例。</param>
public sealed class AlbumDetailAdapter(AlbumDetail albumDetail) : IPlayable
{
    public async IAsyncEnumerable<string> GetSongCidsAsync(ExceptionBox box)
    {
        if (albumDetail.Songs is null)
        {
            yield break;
        }

        foreach (SongInfo song in albumDetail.Songs)
        {
            yield return song.Cid;
        }
    }
}

/// <summary>
/// 为 <see cref="AlbumDetailAdapter"/> 提供扩展方法的类。
/// </summary>
public static class AlbumDetailAdapterExtensions
{
    extension(AlbumDetail detail)
    {
        /// <summary>
        /// 使用 <see cref="AlbumDetail"/> 获得一个 <see cref="AlbumDetailAdapter"/>。
        /// </summary>
        /// <returns>转换后的 <see cref="AlbumDetailAdapter"/>。</returns>
        public AlbumDetailAdapter ToAdapter() => new(detail);
    }
}