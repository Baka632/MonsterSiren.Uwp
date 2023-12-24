using MonsterSiren.Api.Models.Album;
using MonsterSiren.Api.Models.News;

namespace MonsterSiren.Api.Models;

/// <summary>
/// 表示搜索专辑及新闻的结果
/// </summary>
public struct SearchAlbumAndNewsResult(ListPackage<AlbumInfo> albums, ListPackage<NewsInfo> news)
{
    /// <summary>
    /// 专辑信息列表
    /// </summary>
    public ListPackage<AlbumInfo> Albums { get; set; } = albums;
    /// <summary>
    /// 新闻信息列表
    /// </summary>
    public ListPackage<NewsInfo> News { get; set; } = news;
}
