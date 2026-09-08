namespace MonsterSiren.Api.Models;

[Serializable]
internal sealed class InsufficientDataException : Exception
{
    /// <summary>
    /// 完成解析所需的最小字节数。
    /// </summary>
    public int RequiredBytes { get; }

    public InsufficientDataException(int requiredBytes) : this(requiredBytes, "为了完成解析，需要更多数据。") { }
    public InsufficientDataException(int requiredBytes, string message) : base(message) => RequiredBytes = requiredBytes;
    public InsufficientDataException(int requiredBytes, string message, Exception inner) : base(message, inner) => RequiredBytes = requiredBytes;
}
