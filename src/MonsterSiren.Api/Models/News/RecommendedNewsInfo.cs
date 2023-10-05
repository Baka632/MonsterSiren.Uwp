using System.Diagnostics;

namespace MonsterSiren.Api.Models.News;

/// <summary>
/// 提供推荐新闻的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct RecommendedNewsInfo : IEquatable<RecommendedNewsInfo>
{
    /// <summary>
    /// 新闻标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// 新闻封面 Uri
    /// </summary>
    public string CoverUrl { get; set; } = string.Empty;
    /// <summary>
    /// 新闻封面的相关信息
    /// </summary>
    public RecommendedNewsCoverInfo Cover { get; set; }
    /// <summary>
    /// 新闻描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// 新闻类型
    /// </summary>
    public int Type { get; set; } = -1;
    /// <summary>
    /// 新闻数据（CID）
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// 以默认参数构造 <see cref="RecommendedNewsInfo"/> 的新实例
    /// </summary>
    public RecommendedNewsInfo(string title, string coverUrl, RecommendedNewsCoverInfo cover, string description, int type, string data)
    {
        Title = title;
        CoverUrl = coverUrl;
        Cover = cover;
        Description = description;
        Type = type;
        Data = data;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is RecommendedNewsInfo info && Equals(info);
    }

    /// <inheritdoc/>
    public readonly bool Equals(RecommendedNewsInfo other)
    {
        return Title == other.Title &&
               CoverUrl == other.CoverUrl &&
               Cover.Equals(other.Cover) &&
               Description == other.Description &&
               Type == other.Type &&
               Data == other.Data;
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        int hashCode = -982869227;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CoverUrl);
        hashCode = hashCode * -1521134295 + Cover.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
        hashCode = hashCode * -1521134295 + Type.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Data);
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="RecommendedNewsInfo"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="RecommendedNewsInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="RecommendedNewsInfo"/> 实例</param>
    public static bool operator ==(RecommendedNewsInfo left, RecommendedNewsInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="RecommendedNewsInfo"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="RecommendedNewsInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="RecommendedNewsInfo"/> 实例</param>
    public static bool operator !=(RecommendedNewsInfo left, RecommendedNewsInfo right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(Title)} = {Title}, {nameof(CoverUrl)} = {CoverUrl}, {nameof(Description)} = {Description}, {nameof(Type)} = {Type}, {nameof(Data)} = {Data}";
    }
}

/// <summary>
/// 提供推荐新闻封面相关信息的结构
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct RecommendedNewsCoverInfo : IEquatable<RecommendedNewsCoverInfo>
{
    /// <summary>
    /// 使用指定参数构造 <see cref="RecommendedNewsCoverInfo"/> 的新实例
    /// </summary>
    public RecommendedNewsCoverInfo(bool isPrivate, string path)
    {
        IsPrivate = isPrivate;
        Path = path;
    }

    /// <summary>
    /// 指示此封面是否为私密的值
    /// </summary>
    [JsonPropertyName("private")]
    public bool IsPrivate { get; set; } = false;
    /// <summary>
    /// 封面相对于服务器的路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is RecommendedNewsCoverInfo info && Equals(info);
    }

    /// <inheritdoc/>
    public readonly bool Equals(RecommendedNewsCoverInfo other)
    {
        return IsPrivate == other.IsPrivate &&
               Path == other.Path;
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        int hashCode = 1514157746;
        hashCode = hashCode * -1521134295 + IsPrivate.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
        return hashCode;
    }

    /// <summary>
    /// 确定两个 <see cref="RecommendedNewsCoverInfo"/> 实例是否相等
    /// </summary>
    /// <param name="left">第一个 <see cref="RecommendedNewsCoverInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="RecommendedNewsCoverInfo"/> 实例</param>
    public static bool operator ==(RecommendedNewsCoverInfo left, RecommendedNewsCoverInfo right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 确定两个 <see cref="RecommendedNewsCoverInfo"/> 实例是否不同
    /// </summary>
    /// <param name="left">第一个 <see cref="RecommendedNewsCoverInfo"/> 实例</param>
    /// <param name="right">第二个 <see cref="RecommendedNewsCoverInfo"/> 实例</param>
    public static bool operator !=(RecommendedNewsCoverInfo left, RecommendedNewsCoverInfo right)
    {
        return !(left == right);
    }

    private readonly string GetDebuggerDisplay()
    {
        return $"{nameof(IsPrivate)} = {IsPrivate}, {nameof(Path)} = {Path}";
    }
}

