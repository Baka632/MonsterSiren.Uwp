namespace MonsterSiren.Api;

/// <summary>
/// 提供全局的 <see cref="System.Net.Http.HttpClient"/>
/// </summary>
internal sealed class HttpClientProvider
{
    /// <summary>
    /// 共享的 <see cref="System.Net.Http.HttpClient"/> 实例，已配置好基 Uri
    /// </summary>
    public static HttpClient HttpClient { get; } = new()
    {
        BaseAddress = new Uri(CommonValues.ApiBaseUri)
    };
}
