using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 根据 <see cref="ISongCidProvider"/> 获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="playable"><see cref="ISongCidProvider"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会跳过异常项并将异常信息记录到 <see cref="ExceptionBox"/> 中。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(ISongCidProvider playable, ExceptionBox box)
    {
        if (playable is null)
        {
            throw new ArgumentNullException(nameof(playable));
        }

        if (box is null)
        {
            throw new ArgumentNullException(nameof(box));
        }

        ExceptionBox innerBox = new();
        AggregateExceptionHelper aggregateHelper = new();
        int songCount = 0;

        await foreach (string songCid in playable.GetSongCidsAsync(innerBox))
        {
            songCount++;

            MediaPlaybackItem playbackItem;

            try
            {
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(songCid);
            }
            catch (Exception ex)
            {
                aggregateHelper.Record(ex);
                continue;
            }

            yield return playbackItem;
        }

        if (innerBox.InboxException is not null)
        {
            aggregateHelper.Record(innerBox.InboxException);
        }

        if (aggregateHelper.HasException)
        {
            bool allFailed = songCount == aggregateHelper.ExceptionCount;
            box.InboxException = aggregateHelper.TryGetException(AggregateExceptionHelper.GetDataForCommonUsage(allFailed, playable));
        }
    }

    /// <summary>
    /// 根据 <see cref="Playlist"/> 获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="playlist"><see cref="Playlist"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当播放列表中存在无效项时，此方法会跳过无效项并将异常信息记录到 <see cref="ExceptionBox"/> 中。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(Playlist playlist, ExceptionBox box)
    {
        List<Exception> innerExceptions = new(5);

        for (int i = 0; i < playlist.Items.Count; i++)
        {
            PlaylistItem item = playlist.Items[i];
            MediaPlaybackItem playbackItem = null;

            try
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.SongCid, albumDetail);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    await UIThreadHelper.RunOnUIThread(() => playlist.Items[i] = item with { IsCorruptedItem = true });
                }

                innerExceptions.Add(ex);
            }

            if (playbackItem is not null)
            {
                yield return playbackItem;
            }
        }

        if (innerExceptions.Count > 0)
        {
            bool allFailed = playlist.Items.Count == innerExceptions.Count;
            AggregateException aggregate = new("获取一个或多个项目的信息时出现错误，请查看内部异常以获取更多信息。", innerExceptions)
            {
                Data =
                {
                    ["AllFailed"] = allFailed,
                    ["PlayItem"] = playlist,
                }
            };

            box.InboxException = aggregate;
        }
    }

    /// <summary>
    /// 根据 <see cref="Playlist"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="playlists"><see cref="Playlist"/> 序列。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当播放列表中存在无效项时，此方法会跳过无效项并将异常信息记录到 <see cref="ExceptionBox"/> 中。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(Playlist[] playlists, ExceptionBox box)
    {
        List<Exception> innerExceptions = new(5);
        int songCount = 0;

        foreach (Playlist playlist in playlists)
        {
            for (int i = 0; i < playlist.Items.Count; i++)
            {
                PlaylistItem item = playlist.Items[i];
                songCount++;
                MediaPlaybackItem playbackItem = null;

                try
                {
                    AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                    playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.SongCid, albumDetail);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentOutOfRangeException)
                    {
                        await UIThreadHelper.RunOnUIThread(() => playlist.Items[i] = item with { IsCorruptedItem = true });
                    }

                    innerExceptions.Add(ex);
                }

                if (playbackItem is not null)
                {
                    yield return playbackItem;
                }
            }
        }

        if (innerExceptions.Count > 0)
        {
            bool allFailed = songCount == innerExceptions.Count;
            AggregateException aggregate = new("获取一个或多个项目的信息时出现错误，请查看内部异常以获取更多信息。", innerExceptions)
            {
                Data =
                {
                    ["AllFailed"] = allFailed,
                    ["PlayItem"] = playlists,
                }
            };

            box.InboxException = aggregate;
        }
    }

    /// <summary>
    /// 根据 <see cref="SongFavoriteList"/> 获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="songFavorites"><see cref="SongFavoriteList"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当收藏夹中存在无效项时，此方法会跳过无效项并将异常信息记录到 <see cref="ExceptionBox"/> 中。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(SongFavoriteList songFavorites, ExceptionBox box)
    {
        List<Exception> innerExceptions = new(5);

        for (int i = 0; i < songFavorites.Items.Count; i++)
        {
            SongFavoriteItem item = songFavorites.Items[i];
            MediaPlaybackItem playbackItem = null;

            try
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.SongCid, albumDetail);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    await UIThreadHelper.RunOnUIThread(() => songFavorites.Items[i] = item with { IsCorruptedItem = true });
                }

                innerExceptions.Add(ex);
            }

            if (playbackItem is not null)
            {
                yield return playbackItem;
            }
        }

        if (innerExceptions.Count > 0)
        {
            bool allFailed = songFavorites.Items.Count == innerExceptions.Count;
            AggregateException aggregate = new("获取一个或多个项目的信息时出现错误，请查看内部异常以获取更多信息。", innerExceptions)
            {
                Data =
                {
                    ["AllFailed"] = allFailed,
                    ["PlayItem"] = songFavorites,
                }
            };

            box.InboxException = aggregate;
        }
    }

    /// <summary>
    /// 根据 <see cref="AlbumFavoriteList"/> 获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="albumFavorites"><see cref="AlbumFavoriteList"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当收藏夹中存在无效专辑时，此方法会跳过无效专辑及其歌曲，并将异常信息记录到 <see cref="ExceptionBox"/> 中。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(AlbumFavoriteList albumFavorites, ExceptionBox box)
    {
        List<Exception> innerExceptions = new(5);
        int totalSongCount = 0;
        int failedSongCount = 0;

        for (int i = 0; i < albumFavorites.Items.Count; i++)
        {
            AlbumFavoriteItem albumItem = albumFavorites.Items[i];
            AlbumDetail albumDetail;

            try
            {
                albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumItem.AlbumCid);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    await UIThreadHelper.RunOnUIThread(() => albumFavorites.Items[i] = albumItem with { IsCorruptedItem = true });
                }
                innerExceptions.Add(ex);
                continue;
            }

            if (albumDetail.Songs == null)
            {
                continue;
            }

            foreach (SongInfo songInfo in albumDetail.Songs)
            {
                totalSongCount++;
                MediaPlaybackItem playbackItem = null;

                try
                {
                    playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(songInfo.Cid, albumDetail);
                }
                catch (Exception ex)
                {
                    failedSongCount++;
                    innerExceptions.Add(ex);
                }

                if (playbackItem is not null)
                {
                    yield return playbackItem;
                }
            }
        }

        if (innerExceptions.Count > 0)
        {
            bool allFailed = totalSongCount > 0 && failedSongCount == totalSongCount;
            AggregateException aggregate = new("获取一个或多个项目的信息时出现错误，请查看内部异常以获取更多信息。", innerExceptions)
            {
                Data =
                {
                    ["AllFailed"] = allFailed,
                    ["PlayItem"] = albumFavorites,
                }
            };

            box.InboxException = aggregate;
        }
    }
}
