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
    /// <param name="item">一个 <see cref="SongDetail"/>，其中存储了音乐的关键信息</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/>，其中存储了音乐专辑的封面信息</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/></returns>
    public static MediaPlaybackItem ToMediaPlaybackItem(this SongDetail item, AlbumDetail albumDetail)
    {
        Uri musicUri = new(item.SourceUrl, UriKind.Absolute);
        Uri coverUri = new(albumDetail.CoverUrl, UriKind.Absolute);

        MediaPlaybackItem playbackItem = new(MediaSource.CreateFromUri(musicUri));
        MediaItemDisplayProperties displayProps = playbackItem.GetDisplayProperties();
        displayProps.Type = MediaPlaybackType.Music;
        displayProps.MusicProperties.Artist = item.Artists.Any() ? string.Join('/', item.Artists) : "MSR".GetLocalized();
        displayProps.MusicProperties.Title = item.Name;
        displayProps.MusicProperties.AlbumTitle = albumDetail.Name;
        displayProps.MusicProperties.AlbumArtist = item.Artists.FirstOrDefault() ?? "MSR".GetLocalized();
        displayProps.MusicProperties.AlbumTrackCount = (uint)albumDetail.Songs.Count();
        displayProps.Thumbnail = RandomAccessStreamReference.CreateFromUri(coverUri);

        playbackItem.ApplyDisplayProperties(displayProps);

        return playbackItem;
    }
}
