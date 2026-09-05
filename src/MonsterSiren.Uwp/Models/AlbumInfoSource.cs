using System.Collections;
using System.Threading;
using Microsoft.Toolkit.Collections;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 为 <see cref="AlbumInfo"/> 提供可增量加载的源。
/// </summary>
/// <param name="infos"><see cref="AlbumInfo"/> 序列。</param>
public class AlbumInfoSource(IEnumerable<AlbumInfo> infos) : IIncrementalSource<AlbumInfo>, IEnumerable<AlbumInfo>
{
    private readonly List<AlbumInfo> _infos = [.. infos];

    /// <summary>
    /// 此集合中 <see cref="AlbumInfo"/> 的数量。
    /// </summary>
    public int Count => _infos.Count;

    /// <summary>
    /// 获取 <see cref="AlbumInfo"/> 中此集合中的索引。
    /// </summary>
    /// <param name="info">一个 <see cref="AlbumInfo"/> 实例。</param>
    /// <returns>索引号，若未找到则为 -1。</returns>
    public int IndexOf(AlbumInfo info) => _infos.IndexOf(info);

    /// <summary>
    /// 获取在指定位置的 <see cref="AlbumInfo"/> 实例。
    /// </summary>
    /// <param name="index">索引号。</param>
    /// <returns>一个 <see cref="AlbumInfo"/> 实例。</returns>
    /// <exception cref="IndexOutOfRangeException">当索引越界时抛出。</exception>
    public AlbumInfo ElementAt(int index) => _infos[index];

    /// <inheritdoc />
    public Task<IEnumerable<AlbumInfo>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        IEnumerable<AlbumInfo> result = _infos.Skip(pageIndex * pageSize).Take(pageSize);
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public IEnumerator<AlbumInfo> GetEnumerator() => _infos.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _infos.GetEnumerator();
}