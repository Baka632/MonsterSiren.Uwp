using System.Net.Http;
using System.Threading;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为 MonsterSiren.Api 库中模型提供实用方法的类
/// </summary>
public static partial class MsrModelsHelper
{
    [Obsolete("Replace to Async version")]
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
                    SemaphoreSlim semaphore = LockerHelper<string>.GetOrCreateLocker(songDetail.Cid);

                    await semaphore.WaitAsync();
                    try
                    {
                        await FileCacheHelper.StoreSongDurationAsync(songDetail.Cid, currentSpan);
                    }
                    finally
                    {
                        semaphore.Release();
                        LockerHelper<string>.ReturnLocker(songDetail.Cid);
                    }
                }
            }

            sender.OpenOperationCompleted -= TryCacheSongDuration;
        }
    }

    /// <summary>
    /// 使用 <see cref="AlbumDetail"/> 和 <see cref="SongDetail"/> 来获得可供播放器播放的 <see cref="MediaPlaybackItem"/>
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/>，其中存储了音乐的关键信息</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/>，其中存储了音乐专辑的封面信息</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/></returns>
    public static async Task<MediaPlaybackItem> GetMediaPlaybackItemAsync(SongDetail songDetail, AlbumDetail albumDetail)
    {
        if (string.IsNullOrWhiteSpace(songDetail.SourceUrl))
        {
            throw new ArgumentNullException(nameof(songDetail), $"歌曲{(string.IsNullOrWhiteSpace(songDetail.Name) ? string.Empty : $"《{songDetail.Name}》")}没有音频链接信息，无法播放。");
        }

        Uri musicUri = new(songDetail.SourceUrl, UriKind.Absolute);

        List<SongInfo> songs = [.. albumDetail.Songs];
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

        Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumDetail);
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
                    SemaphoreSlim semaphore = LockerHelper<string>.GetOrCreateLocker(songDetail.Cid);

                    await semaphore.WaitAsync();
                    try
                    {
                        await FileCacheHelper.StoreSongDurationAsync(songDetail.Cid, currentSpan);
                    }
                    finally
                    {
                        semaphore.Release();
                        LockerHelper<string>.ReturnLocker(songDetail.Cid);
                    }
                }
            }

            sender.OpenOperationCompleted -= TryCacheSongDuration;
        }
    }

    /// <summary>
    /// 通过歌曲的 CID 来获得可供播放器播放的 <see cref="MediaPlaybackItem"/>
    /// </summary>
    /// <param name="songCid">歌曲 CID</param>
    /// <param name="refresh">指示是否要跳过缓存来获得最新版本的 <see cref="SongDetail"/> 和 <see cref="AlbumDetail"/> 的值</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/></returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数无效</exception>
    /// <exception cref="System.ArgumentNullException">参数为空或空白</exception>
    public static async Task<MediaPlaybackItem> GetMediaPlaybackItemAsync(string songCid, bool refresh = false)
    {
        SongDetail songDetail = await GetSongDetailAsync(songCid, refresh);
        AlbumDetail albumDetail = await GetAlbumDetailAsync(songDetail.AlbumCid, refresh);
        return await GetMediaPlaybackItemAsync(songDetail, albumDetail);
    }

    /// <summary>
    /// 通过歌曲的 CID 和一个 <see cref="AlbumDetail"/> 实例来获得可供播放器播放的 <see cref="MediaPlaybackItem"/>
    /// </summary>
    /// <param name="songCid">歌曲 CID</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    /// <param name="refresh">指示是否要跳过缓存来获得最新版本的 <see cref="SongDetail"/> 的值</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/></returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数无效</exception>
    /// <exception cref="System.ArgumentNullException">参数为空或空白</exception>
    public static async Task<MediaPlaybackItem> GetMediaPlaybackItemAsync(string songCid, AlbumDetail albumDetail, bool refresh = false)
    {
        SongDetail songDetail = await GetSongDetailAsync(songCid, refresh);
        return await GetMediaPlaybackItemAsync(songDetail, albumDetail);
    }

    /// <summary>
    /// 获取歌曲的时长
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/> 实例</param>
    /// <returns>一个 <see cref="System.TimeSpan"/> 实例</returns>
    public static async Task<TimeSpan?> GetSongDurationAsync(SongDetail songDetail)
    {
        SemaphoreSlim semaphore = LockerHelper<string>.GetOrCreateLocker(songDetail.Cid);

        await semaphore.WaitAsync();
        try
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
        finally
        {
            semaphore.Release();
            LockerHelper<string>.ReturnLocker(songDetail.Cid);
        }
    }

    /// <summary>
    /// 通过歌曲 CID 获得一个 <see cref="SongDetail"/> 实例
    /// </summary>
    /// <param name="cid">歌曲 CID</param>
    /// <param name="refresh">指示是否要跳过缓存来获得最新版本的 <see cref="SongDetail"/> 的值</param>
    /// <returns>一个 <see cref="SongDetail"/> 实例</returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数无效</exception>
    /// <exception cref="System.ArgumentNullException">参数为空或空白</exception>
    public static async Task<SongDetail> GetSongDetailAsync(string cid, bool refresh = false)
    {
        if (!refresh && MemoryCacheHelper<SongDetail>.Default.TryGetData(cid, out SongDetail detail))
        {
            return detail;
        }
        else
        {
            SongDetail songDetail = await SongService.GetSongDetailedInfoAsync(cid);
            MemoryCacheHelper<SongDetail>.Default.Store(cid, songDetail);

            return songDetail;
        }
    }

    /// <summary>
    /// 尝试为 <see cref="SongInfo"/> 填充艺术家信息
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例</param>
    /// <returns>一个二元组，第一项是指示是否修改了 <see cref="SongInfo"/> 的布尔值，第二项是 <see cref="SongInfo"/> 实例。若第一项为 <see langword="false"/> ，则表示没有必要对 <see cref="SongInfo"/> 进行修改</returns>
    public static ValueTuple<bool, SongInfo> TryFillArtistForSong(SongInfo songInfo)
    {
        if (songInfo.Artists is null || songInfo.Artists.Any() != true)
        {
            songInfo = songInfo with { Artists = ["MSR".GetLocalized()] };

            return (true, songInfo);
        }
        else
        {
            return (false, songInfo);
        }
    }

    /// <summary>
    /// 尝试为 <see cref="SongInfo"/> 序列填充艺术家信息
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 序列的实例</param>
    /// <returns>指示是否修改了 <see cref="SongInfo"/> 的布尔值</returns>
    public static bool TryFillArtistForSongs(IList<SongInfo> songs)
    {
        bool isModify = false;

        for (int i = 0; i < songs.Count; i++)
        {
            (bool modifySuccess, SongInfo songInfo) = TryFillArtistForSong(songs[i]);

            if (modifySuccess)
            {
                songs[i] = songInfo;
                isModify = true;
            }
        }

        return isModify;
    }

    /// <summary>
    /// 尝试为 <see cref="SongDetail"/> 填充艺术家信息
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/> 实例</param>
    /// <returns>一个二元组，第一项是指示是否修改了 <see cref="SongDetail"/> 的布尔值，第二项是 <see cref="SongDetail"/> 实例。若第一项为 <see langword="false"/> ，则表示没有必要对 <see cref="SongDetail"/> 进行修改</returns>
    public static ValueTuple<bool, SongDetail> TryFillArtistForSong(SongDetail songDetail)
    {
        if (songDetail.Artists is null || songDetail.Artists.Any() != true)
        {
            songDetail = songDetail with { Artists = ["MSR".GetLocalized()] };

            return (true, songDetail);
        }
        else
        {
            return (false, songDetail);
        }
    }
}