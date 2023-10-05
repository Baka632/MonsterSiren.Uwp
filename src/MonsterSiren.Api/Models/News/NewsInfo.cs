using System.Diagnostics;

namespace MonsterSiren.Api.Models.News;

/// <summary>
/// 提供新闻基本信息的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct NewsInfo : IEquatable<NewsInfo>
{
    /// <summary>
    /// 新闻的 CID
    /// </summary>
    public string Cid { get; set; } = string.Empty;
    /// <summary>
    /// 新闻标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 新闻分类
    /// </summary>
    [JsonPropertyName("cate")]
    public int Category { get; set; } = -1;
    /// <summary>
    /// 新闻日期
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// 以默认参数构造 <see cref="NewsInfo"/> 的新实例
    /// </summary>
    public NewsInfo(string cid, string title, int category, DateTimeOffset date)
    {
        Cid = cid;
        Title = title;
        Category = category;
        Date = date;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is NewsInfo info && Equals(info);
    }

    /// <inheritdoc/>
    public readonly bool Equals(NewsInfo other)
    {
        return Cid == other.Cid &&
               Title == other.Title &&
               Category == other.Category &&
               Date.Equals(other.Date);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        int hashCode = -442794702;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Cid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
        hashCode = hashCode * -1521134295 + Category.GetHashCode();
        hashCode = hashCode * -1521134295 + Date.GetHashCode();
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="NewsInfo"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="NewsInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="NewsInfo"/> 实例</param>
    public static bool operator ==(NewsInfo left, NewsInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="NewsInfo"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="NewsInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="NewsInfo"/> 实例</param>
    public static bool operator !=(NewsInfo left, NewsInfo right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(Cid)} = {Cid}, {nameof(Title)} = {Title}, {nameof(Date)} = {Date}, {nameof(Category)} = {Category}";
    }
}
