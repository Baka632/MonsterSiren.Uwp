namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 为专辑封面加载提供参数的类。
/// </summary>
/// <param name="AlbumCid">专辑 CID。</param>
/// <param name="CoverUri">专辑封面 Uri。</param>
public readonly record struct AlbumCoverLoadArgs(string AlbumCid, string CoverUri);
