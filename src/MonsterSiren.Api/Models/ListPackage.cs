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
    /// 指示要自动播放的歌曲 CID 的值
    /// </summary>
    [JsonPropertyName("autoplay")]
    public string? AutoplaySongCid { get; set; } = null;

    /// <summary>
    /// 使用指定的参数构造 <see cref="ListPackage{T}"/> 的新实例
    /// </summary>
    public ListPackage(IEnumerable<T> list, bool? isEnd, string? autoplaySongCid)
    {
        List = list;
        IsEnd = isEnd;
        AutoplaySongCid = autoplaySongCid;
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
