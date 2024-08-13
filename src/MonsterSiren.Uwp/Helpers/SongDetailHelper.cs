﻿using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为 <see cref="SongDetail"/> 提供实用方法的类
/// </summary>
public static class SongDetailHelper
{
    /// <summary>
    /// 使用 <see cref="AlbumDetail"/> 和 <see cref="SongDetail"/> 来获得可供播放器播放的 <see cref="MediaPlaybackItem"/>
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

        if (!MemoryCacheHelper<TimeSpan>.Default.TryGetData(songDetail.Cid, out _))
        {
            source.OpenOperationCompleted += TryCacheSongDuration;
        }

        MediaItemDisplayProperties displayProps = playbackItem.GetDisplayProperties();
        displayProps.Type = MediaPlaybackType.Music;
        displayProps.MusicProperties.Artist = songDetail.Artists.Any() ? string.Join('/', songDetail.Artists) : "MSR".GetLocalized();
        displayProps.MusicProperties.Title = songDetail.Name;
        displayProps.MusicProperties.TrackNumber = (uint)songs.FindIndex(songInfo => songInfo.Cid == songDetail.Cid) + 1;
        displayProps.MusicProperties.AlbumTitle = albumDetail.Name;
        displayProps.MusicProperties.AlbumArtist = songDetail.Artists.FirstOrDefault() ?? "MSR".GetLocalized();
        displayProps.MusicProperties.AlbumTrackCount = (uint)songs.Count;

        Uri fileCoverUri = FileCacheHelper.GetAlbumCoverUriAsync(albumDetail).Result;
        displayProps.Thumbnail = fileCoverUri is not null
            ? RandomAccessStreamReference.CreateFromUri(fileCoverUri)
            : RandomAccessStreamReference.CreateFromUri(new(albumDetail.CoverUrl, UriKind.Absolute));

        playbackItem.ApplyDisplayProperties(displayProps);

        return playbackItem;

        async void TryCacheSongDuration(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs e)
        {
            if (sender.State == MediaSourceState.Opened && sender.Duration.HasValue)
            {
                TimeSpan currentSpan = sender.Duration.Value;
                TimeSpan? span = await FileCacheHelper.GetSongDurationAsync(songDetail.Cid);
                
                if (span != currentSpan)
                {
                    await FileCacheHelper.StoreSongDurationAsync(songDetail.Cid, currentSpan);
                }
            }

            sender.OpenOperationCompleted -= TryCacheSongDuration;
        }
    }

    /// <summary>
    /// 获取歌曲的时长
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/> 实例</param>
    /// <returns>一个 <see cref="System.TimeSpan"/> 实例</returns>
    public static async Task<TimeSpan?> GetSongDurationAsync(SongDetail songDetail)
    {
        TimeSpan? span = await FileCacheHelper.GetSongDurationAsync(songDetail.Cid);
        if (span.HasValue)
        {
            return span;
        }
        else
        {
            Uri musicUri = new(songDetail.SourceUrl, UriKind.Absolute);
            using MediaSource source = MediaSource.CreateFromUri(musicUri);
            await source.OpenAsync();

            TimeSpan? duration = source.Duration;

            if (duration.HasValue)
            {
                await FileCacheHelper.StoreSongDurationAsync(songDetail.Cid, duration.Value);
            }

            return duration;
        }
    }

    /// <summary>
    /// 通过 <see cref="SongInfo"/> 实例获得一个 <see cref="SongDetail"/> 实例
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例</param>
    /// <returns>一个 <see cref="SongDetail"/> 实例</returns>
    public static async Task<SongDetail> GetSongDetailAsync(SongInfo songInfo)
    {
        if (MemoryCacheHelper<SongDetail>.Default.TryGetData(songInfo.Cid, out SongDetail detail))
        {
            return detail;
        }
        else
        {
            SongDetail songDetail = await SongService.GetSongDetailedInfoAsync(songInfo.Cid);
            MemoryCacheHelper<SongDetail>.Default.Store(songInfo.Cid, songDetail);

            return songDetail;
        }
    }
}