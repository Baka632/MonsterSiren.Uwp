using System.Net.Http;
using System.Threading;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为 MonsterSiren.Api 库中模型提供实用方法的类。
/// </summary>
public static partial class MsrModelsHelper
{
    /// <summary>
    /// 尝试从 <see cref="MediaPlaybackItem"/> 中获取 <see cref="SongDetail"/>。
    /// </summary>
    /// <param name="item">从 <see cref="MsrModelsHelper"/> 获取到的 <see cref="MediaPlaybackItem"/>。</param>
    /// <remarks>
    /// 传入 <see cref="MediaPlaybackItem"/> 的来源必须是 <see cref="MsrModelsHelper"/> 中的相关方法，因为这些方法会向 <see cref="MediaPlaybackItem"/> 写入必要的额外数据。
    /// </remarks>
    /// <returns>
    /// <para>
    /// 一个二元组。
    /// </para>
    /// <para>
    /// 第一个值指示操作是否成功，第二个值是操作成功时获取到的 <see cref="SongDetail"/> 实例。
    /// </para>
    /// </returns>
    public static async Task<ValueTuple<bool, SongDetail>> TryGetSongDetailFromMediaPlaybackItem(MediaPlaybackItem item)
    {
        if (TryGetSongCidFromMediaPlaybackItem(item, out string cid))
        {
            try
            {
                SongDetail songDetail = await GetSongDetailAsync(cid);
                return (true, songDetail);
            }
            catch
            {
            }
        }

        return (false, default);
    }

    /// <summary>
    /// 使用 <see cref="AlbumDetail"/> 和 <see cref="SongDetail"/> 来获得可供播放器播放的 <see cref="MediaPlaybackItem"/>。
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/>，其中存储了音乐的关键信息。</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/>，其中存储了音乐专辑的封面信息。</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/>。</returns>
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

        // 为了将 MediaPlaybackItem 与 SongDetail 联系起来，于是借用了 Genres，qwq
        SetExtraSongAndAlbumCidForMusicProperties(displayProps.MusicProperties, albumDetail, songDetail);

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
                    SemaphoreSlim semaphore = CommonValues.SongDurationLocker.GetOrCreateLocker(songDetail.Cid);

                    await semaphore.WaitAsync();
                    try
                    {
                        await FileCacheHelper.StoreSongDurationAsync(songDetail.Cid, currentSpan);
                    }
                    finally
                    {
                        semaphore.Release();
                        CommonValues.SongDurationLocker.ReturnLocker(songDetail.Cid);
                    }
                }
            }

            sender.OpenOperationCompleted -= TryCacheSongDuration;
        }
    }

    /// <summary>
    /// 通过歌曲的 CID 来获得可供播放器播放的 <see cref="MediaPlaybackItem"/>。
    /// </summary>
    /// <param name="songCid">歌曲 CID。</param>
    /// <param name="refresh">指示是否要跳过缓存来获得最新版本的 <see cref="SongDetail"/> 和 <see cref="AlbumDetail"/> 的值。</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/>。</returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数无效。</exception>
    /// <exception cref="System.ArgumentNullException">参数为空或空白。</exception>
    public static async Task<MediaPlaybackItem> GetMediaPlaybackItemAsync(string songCid, bool refresh = false)
    {
        SongDetail songDetail = await GetSongDetailAsync(songCid, refresh);
        AlbumDetail albumDetail = await GetAlbumDetailAsync(songDetail.AlbumCid, refresh);
        return await GetMediaPlaybackItemAsync(songDetail, albumDetail);
    }

    /// <summary>
    /// 通过歌曲的 CID 和一个 <see cref="AlbumDetail"/> 实例来获得可供播放器播放的 <see cref="MediaPlaybackItem"/>。
    /// </summary>
    /// <param name="songCid">歌曲 CID。</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例。</param>
    /// <param name="refresh">指示是否要跳过缓存来获得最新版本的 <see cref="SongDetail"/> 的值。</param>
    /// <returns>已设置好媒体信息且可供播放器播放的 <see cref="MediaPlaybackItem"/>。</returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数无效。</exception>
    /// <exception cref="System.ArgumentNullException">参数为空或空白。</exception>
    public static async Task<MediaPlaybackItem> GetMediaPlaybackItemAsync(string songCid, AlbumDetail albumDetail, bool refresh = false)
    {
        SongDetail songDetail = await GetSongDetailAsync(songCid, refresh);
        return await GetMediaPlaybackItemAsync(songDetail, albumDetail);
    }

    /// <summary>
    /// 获取歌曲的时长。
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/> 实例。</param>
    /// <returns>一个 <see cref="System.TimeSpan"/> 实例。</returns>
    public static async Task<TimeSpan?> GetSongDurationAsync(SongDetail songDetail)
    {
        SemaphoreSlim semaphore = CommonValues.SongDurationLocker.GetOrCreateLocker(songDetail.Cid);

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
            CommonValues.SongDurationLocker.ReturnLocker(songDetail.Cid);
        }
    }

    /// <summary>
    /// 通过歌曲 CID 获得一个 <see cref="SongDetail"/> 实例。
    /// </summary>
    /// <param name="cid">歌曲 CID。</param>
    /// <param name="refresh">指示是否要跳过缓存来获得最新版本的 <see cref="SongDetail"/> 的值。</param>
    /// <returns>一个 <see cref="SongDetail"/> 实例。</returns>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败。</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数无效。</exception>
    /// <exception cref="System.ArgumentNullException">参数为空或空白。</exception>
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
    /// 尝试为 <see cref="SongInfo"/> 填充艺术家信息。
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例。</param>
    /// <returns>
    /// <para>
    /// 一个二元组。
    /// </para>
    /// <para>
    /// 第一项是指示是否修改了 <see cref="SongInfo"/> 的布尔值，第二项是 <see cref="SongInfo"/> 实例。
    /// </para>
    /// <para>
    /// 若第一项为 <see langword="false"/> ，则表示没有必要对 <see cref="SongInfo"/> 进行修改。
    /// </para>
    /// </returns>
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
    /// 尝试为 <see cref="SongInfo"/> 序列填充艺术家信息。
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 序列的实例。</param>
    /// <returns>指示是否修改了 <see cref="SongInfo"/> 的布尔值。</returns>
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
    /// 尝试为 <see cref="SongDetail"/> 填充艺术家信息。
    /// </summary>
    /// <param name="songDetail">一个 <see cref="SongDetail"/> 实例。</param>
    /// <returns>
    /// <para>
    /// 一个二元组。
    /// </para>
    /// <para>
    /// 第一项是指示是否修改了 <see cref="SongDetail"/> 的布尔值，第二项是 <see cref="SongDetail"/> 实例。
    /// </para>
    /// <para>
    /// 若第一项为 <see langword="false"/>，则表示没有必要对 <see cref="SongDetail"/> 进行修改。
    /// </para>
    /// </returns>
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

    private static void SetExtraSongAndAlbumCidForMusicProperties(MusicDisplayProperties properties, AlbumDetail albumDetail, SongDetail songDetail)
    {
        if (properties is null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        // 为了将 MediaPlaybackItem 与 SongDetail 联系起来，于是借用了 Genres，qwq
        // 0 => Song CID
        // 1 => Album CID
        properties.Genres.Add(songDetail.Cid);
        properties.Genres.Add(albumDetail.Cid);
    }

    private static bool TryGetSongCidFromMediaPlaybackItem(MediaPlaybackItem item, out string songCid)
    {
        if (item is not null)
        {
            MusicDisplayProperties props = item.GetDisplayProperties().MusicProperties;
            songCid = props.Genres.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(songCid))
            {
                return true;
            }
        }

        songCid = null;
        return false;
    }

    private static bool TryGetAlbumCidFromMediaPlaybackItem(MediaPlaybackItem item, out string albumCid)
    {
        if (item is not null)
        {
            MusicDisplayProperties props = item.GetDisplayProperties().MusicProperties;
            albumCid = props.Genres.Skip(1).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(albumCid))
            {
                return true;
            }
        }

        albumCid = null;
        return false;
    }
}