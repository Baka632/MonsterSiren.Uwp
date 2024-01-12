using System.Diagnostics;

namespace MonsterSiren.Api.Models;

/// <summary>
/// 为服务器响应提供通用模型
/// </summary>
/// <typeparam name="T">响应中数据的类型</typeparam>
public struct ResponsePackage<T>
{
    /// <summary>
    /// 响应代码
    /// </summary>
    public int Code { get; set; } = -1;

    /// <summary>
    /// 响应消息
    /// </summary>
    [JsonPropertyName("msg")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应中的实际数据
    /// </summary>
    public T? Data { get; set; } = default;

    /// <summary>
    /// 使用指定的参数构造 <see cref="ResponsePackage{T}"/> 的新实例
    /// </summary>
    public ResponsePackage(int code, string message, T? data)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    /// <summary>
    /// 确定操作是否成功
    /// </summary>
    /// <returns>若操作成功，则返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    [DebuggerStepThrough]
    public readonly bool IsSuccess()
    {
        return Code == 0;
    }
}
