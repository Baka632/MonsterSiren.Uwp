namespace MonsterSiren.Api.Models;

/// <summary>
/// 为响应中的列表数据提供通用模型
/// </summary>
/// <typeparam name="T">列表中数据的类型</typeparam>
public struct ListPackage<T>
{
    /// <summary>
    /// 响应中的列表
    /// </summary>
    public IEnumerable<T> List { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// 指示列表是否结束的值
    /// </summary>
    [JsonPropertyName("end")]
    public bool? IsEnd { get; set; } = null;

    /// <summary>
    /// 指示列表中的歌曲是否自动播放的值
    /// </summary>
    [JsonPropertyName("autoplay")]
    public bool? IsAutoplay { get; set; } = null;

    /// <summary>
    /// 以默认参数构造 <see cref="ListPackage{T}"/> 的新实例
    /// </summary>
    public ListPackage(IEnumerable<T> list, bool? isEnd, bool? isAutoplay)
    {
        List = list;
        IsEnd = isEnd;
        IsAutoplay = isAutoplay;
    }

    /// <summary>
    /// 获取一个 <see cref="IEnumerator{T}"/>
    /// </summary>
    /// <returns>一个 <see cref="IEnumerator{T}"/></returns>
    public readonly IEnumerator<T> GetEnumerator()
    {
        return List.GetEnumerator();
    }
}
