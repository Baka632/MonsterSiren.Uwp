namespace MonsterSiren.Uwp.Models.Abstracts;

/// <summary>
/// 表示可被收藏的内容。
/// </summary>
public interface IFavoriteAddable
{
    /// <summary>
    /// 添加到收藏夹。
    /// </summary>
    /// <param name="box">收集异常的 <see cref="ExceptionBox"/>。</param>
    Task AddToFavoriteAsync(ExceptionBox box);

    /// <summary>
    /// 从收藏夹移除。
    /// </summary>
    Task RemoveFromFavoriteAsync();
}
