using MonsterSiren.Uwp.Models.Favorites;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 根据 <see cref="AlbumInfo"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="albumInfos"><see cref="AlbumInfo"/> 序列。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会跳过异常项并将异常信息记录到 <see cref="ExceptionBox"/> 中。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(AlbumInfo[] albumInfos, ExceptionBox box)
    {
        List<Exception> innerExceptions = new(5);
        int songCount = 0;

        foreach (AlbumInfo albumInfo in albumInfos)
        {
            AlbumDetail detail;

            try
            {
                detail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            }
            catch (Exception ex)
            {
                innerExceptions.Add(ex);
                continue;
            }

            foreach (SongInfo item in detail.Songs)
            {
                songCount++;
                MediaPlaybackItem playbackItem = null;

                try
                {
                    playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.Cid, detail);
                }
                catch (Exception ex)
                {
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
                    ["PlayItem"] = albumInfos,
                }
            };

            box.InboxException = aggregate;
        }
    }

    /// <summary>
    /// 根据 <see cref="AlbumDetail"/> 实例获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(AlbumDetail albumDetail, ExceptionBox box)
    {
        foreach (SongInfo item in albumDetail.Songs)
        {
            MediaPlaybackItem playbackItem;

            try
            {
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.Cid, albumDetail);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return playbackItem;
        }
    }

    /// <summary>
    /// 根据 <see cref="SongInfo"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="songInfos"><see cref="SongInfo"/> 序列。</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/> 实例。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(SongInfo[] songInfos, AlbumDetail albumDetail, ExceptionBox box)
    {
        foreach (SongInfo item in songInfos)
        {
            MediaPlaybackItem playbackItem;

            try
            {
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.Cid, albumDetail);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return playbackItem;
        }
    }

    /// <summary>
    /// 根据 <see cref="PlaylistItem"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(PlaylistItem[] playlistItems, ExceptionBox box)
    {
        foreach (PlaylistItem item in playlistItems)
        {
            MediaPlaybackItem playbackItem;

            try
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.SongCid, albumDetail);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return playbackItem;
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
    /// 根据 <see cref="SongInfoAndAlbumDetailPack"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="packs"><see cref="SongInfoAndAlbumDetailPack"/> 序列。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(SongInfoAndAlbumDetailPack[] packs, ExceptionBox box)
    {
        foreach ((SongInfo songInfo, AlbumDetail albumDetail) in packs)
        {
            MediaPlaybackItem item;

            try
            {
                item = await MsrModelsHelper.GetMediaPlaybackItemAsync(songInfo.Cid, albumDetail);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return item;
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
    /// 根据 <see cref="SongFavoriteItem"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。
    /// </summary>
    /// <param name="songFavoriteItems"><see cref="SongFavoriteItem"/> 序列。</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/>。</param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列。</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<MediaPlaybackItem> GetMediaPlaybackItems(SongFavoriteItem[] songFavoriteItems, ExceptionBox box)
    {
        foreach (SongFavoriteItem item in songFavoriteItems)
        {
            MediaPlaybackItem playbackItem;

            try
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.SongCid, albumDetail);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return playbackItem;
        }
    }
}
