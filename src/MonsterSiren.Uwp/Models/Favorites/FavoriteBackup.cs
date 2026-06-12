namespace MonsterSiren.Uwp.Models.Favorites;

/// <summary>
///用于为各类型的收藏提供备份的类。
/// </summary>
/// <param name="SongFavorite">歌曲收藏。</param>
/// <param name="AlbumFavorite">专辑收藏。</param>
public record FavoriteBackup(SongFavoriteList SongFavorite, AlbumFavoriteList AlbumFavorite);
