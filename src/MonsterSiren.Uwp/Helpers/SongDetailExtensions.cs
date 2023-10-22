using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为 <see cref="SongDetail"/> 提供扩展方法
/// </summary>
internal static class SongDetailExtensions
{
    /// <summary>
    /// 使用一个 <see cref="AlbumDetail"/> 将 <see cref="SongDetail"/> 转换为可供播放器播放的 <see cref="MediaPlaybackItem"/>
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/>，其中存储了音乐的关键信息</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/>，其中存储了音乐专辑的封面信息</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/></returns>
    public static MediaPlaybackItem ToMediaPlaybackItem(this SongDetail songDetail, AlbumDetail albumDetail)
    {
        Uri musicUri = new(songDetail.SourceUrl, UriKind.Absolute);

        List<SongInfo> songs = albumDetail.Songs.ToList();
        MediaSource source = MediaSource.CreateFromUri(musicUri);
        MediaPlaybackItem playbackItem = new(source);
        MediaItemDisplayProperties displayProps = playbackItem.GetDisplayProperties();
        displayProps.Type = MediaPlaybackType.Music;
        displayProps.MusicProperties.Artist = songDetail.Artists.Any() ? string.Join('/', songDetail.Artists) : "MSR".GetLocalized();
        displayProps.MusicProperties.Title = songDetail.Name;
        displayProps.MusicProperties.TrackNumber = (uint)songs.FindIndex(info => info.Name == songDetail.Name && info.AlbumCid == songDetail.AlbumCid);
        displayProps.MusicProperties.AlbumTitle = albumDetail.Name;
        displayProps.MusicProperties.AlbumArtist = songDetail.Artists.FirstOrDefault() ?? "MSR".GetLocalized();
        displayProps.MusicProperties.AlbumTrackCount = (uint)songs.Count;

        Uri fileCoverUri = FileCacheHelper.Default.GetAlbumCoverUriAsync(albumDetail).Result;
        displayProps.Thumbnail = fileCoverUri != null
            ? RandomAccessStreamReference.CreateFromUri(fileCoverUri)
            : RandomAccessStreamReference.CreateFromUri(new(albumDetail.CoverUrl, UriKind.Absolute));

        playbackItem.ApplyDisplayProperties(displayProps);

        return playbackItem;
    }
}
