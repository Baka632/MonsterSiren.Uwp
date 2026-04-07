namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为生成 <see cref="AggregateException"/> 异常提供帮助的类。
/// </summary>
public sealed class AggregateExceptionHelper
{
    private readonly List<Exception> innerExceptions = new(5);

    /// <summary>
    /// 指示当前是否存在异常信息。
    /// </summary>
    public bool HasException => innerExceptions.Count > 0;

    /// <summary>
    /// 获取当前异常信息数量。
    /// </summary>
    public int ExceptionCount => innerExceptions.Count;

    /// <summary>
    /// 记录异常信息。
    /// </summary>
    /// <param name="exception">异常实例。</param>
    public void Record(Exception exception)
    {
        innerExceptions.Add(exception);
    }

    /// <summary>
    /// 获取由全部异常信息生成的 <see cref="AggregateException"/>。
    /// </summary>
    /// <typeparam name="TKey">异常数据字典的键信息。</typeparam>
    /// <typeparam name="TValue">异常数据字典的值信息。</typeparam>
    /// <param name="data">记录异常信息的键值对序列。</param>
    /// <returns>一个 <see cref="AggregateException"/> 实例。若没有异常消息被记录，则返回 <see langword="null"/>。</returns>
    public AggregateException TryGetException<TKey, TValue>(IEnumerable<(TKey Key, TValue Value)> data)
    {
        if (!HasException)
        {
            return null;
        }

        AggregateException aggregate = new("获取一个或多个项目的信息时出现错误，请查看内部异常以获取更多信息。", innerExceptions);
        if (data is not null)
        {
            foreach ((TKey Key, TValue Value) in data)
            {
                aggregate.Data[Key] = Value;
            }
        }

        return aggregate;
    }
}

/// <summary>
/// 为 <see cref="AggregateExceptionHelper"/> 提供扩展方法的类。
/// </summary>
public static class AggregateExceptionHelperExtensions
{
    extension(AggregateExceptionHelper)
    {
        /// <summary>
        /// 为 <see cref="AggregateExceptionHelper"/> 的常见使用方法生成异常信息键值对序列。
        /// </summary>
        /// <param name="allFailed">指示操作是否全部失败的值。</param>
        /// <param name="playItem">操作原始数据来源的实例。</param>
        /// <returns>异常信息键值对序列。</returns>
        public static IEnumerable<(string Key, object Value)> GetDataForCommonUsage(bool allFailed, object playItem)
        {
            return [
                    ("AllFailed", allFailed),
                    ("PlayItem", playItem)
            ];
        }
    }
}