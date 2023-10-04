namespace MonsterSiren.Api;

/// <summary>
/// 存储库中常用常量的类
/// </summary>
public static class CommonValues
{
    /// <summary>
    /// 塞壬唱片 API 的基 Uri
    /// </summary>
    public static readonly string ApiBaseUri = "https://monster-siren.hypergryph.com/api/";

    /// <summary>
    /// 默认 <see cref="JsonSerializer"/> 设置
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
