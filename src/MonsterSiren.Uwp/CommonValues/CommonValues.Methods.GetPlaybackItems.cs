using MonsterSiren.Uwp.Models.Abstracts;
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
        AllFailedHelper allFailedHelper = new();

        await foreach (string songCid in playable.GetSongCidsAsync(innerBox))
        {
            allFailedHelper.Start();

            MediaPlaybackItem playbackItem;

            try
            {
                playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(songCid);
                allFailedHelper.Succeed();
            }
            catch (Exception ex)
            {
                aggregateHelper.Record(ex);

                if (ex is ArgumentOutOfRangeException && playable is IContentCorruptible contentCorruptible)
                {
                    contentCorruptible.MarkItemAsCorrupted(songCid);
                }

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
            bool allFailed = allFailedHelper.IsAllFailed();
            box.InboxException = aggregateHelper.TryGetException(AggregateExceptionHelper.GetDataForCommonUsage(allFailed, playable));
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
        AggregateExceptionHelper aggregateHelper = new();
        AllFailedHelper allFailedHelper = new();

        foreach (Playlist playlist in playlists)
        {
            for (int i = 0; i < playlist.Items.Count; i++)
            {
                allFailedHelper.Start();

                PlaylistItem item = playlist.Items[i];
                MediaPlaybackItem playbackItem = null;

                try
                {
                    AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                    playbackItem = await MsrModelsHelper.GetMediaPlaybackItemAsync(item.SongCid, albumDetail);
                    allFailedHelper.Succeed();
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentOutOfRangeException)
                    {
                        await UIThreadHelper.RunOnUIThread(() => playlist.Items[i] = item with { IsCorruptedItem = true });
                    }

                    aggregateHelper.Record(ex);
                }

                if (playbackItem is not null)
                {
                    yield return playbackItem;
                }
            }
        }

        if (aggregateHelper.HasException)
        {
            bool allFailed = allFailedHelper.IsAllFailed();
            box.InboxException = aggregateHelper.TryGetException(AggregateExceptionHelper.GetDataForCommonUsage(allFailed, playlists));
        }
    }
}
