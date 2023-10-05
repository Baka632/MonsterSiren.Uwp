using System.Diagnostics;

namespace MonsterSiren.Api.Models.News;

/// <summary>
/// 提供新闻详细信息的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct NewsDetail : IEquatable<NewsDetail>
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
    /// 新闻作者
    /// </summary>
    public string Author { get; set; } = string.Empty;
    /// <summary>
    /// 新闻的 HTML 内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>
    /// 新闻日期
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// 使用指定的参数构造 <see cref="NewsDetail"/> 的新实例
    /// </summary>
    public NewsDetail(string cid, string title, int category, string author, string content, DateTimeOffset date)
    {
        Cid = cid;
        Title = title;
        Category = category;
        Author = author;
        Content = content;
        Date = date;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is NewsDetail detail && Equals(detail);
    }

    /// <inheritdoc/>
    public readonly bool Equals(NewsDetail other)
    {
        return Cid == other.Cid &&
               Title == other.Title &&
               Category == other.Category &&
               Author == other.Author &&
               Content == other.Content &&
               Date.Equals(other.Date);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        int hashCode = 629981130;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Cid);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
        hashCode = hashCode * -1521134295 + Category.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Author);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Content);
        hashCode = hashCode * -1521134295 + Date.GetHashCode();
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="NewsDetail"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="NewsDetail"/> 实例</param>
    /// <param name="right">第二个 <see cref="NewsDetail"/> 实例</param>
    public static bool operator ==(NewsDetail left, NewsDetail right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="NewsDetail"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="NewsDetail"/> 实例</param>
    /// <param name="right">第二个 <see cref="NewsDetail"/> 实例</param>
    public static bool operator !=(NewsDetail left, NewsDetail right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(Cid)} = {Cid}, {nameof(Title)} = {Title}, {nameof(Date)} = {Date}, {nameof(Category)} = {Category}, {nameof(Author)} = {Author}";
    }
}
