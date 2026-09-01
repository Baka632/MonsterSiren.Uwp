using System.Text;
using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 启动下载 <see cref="ISongCidProvider"/> 中表示歌曲的操作。
    /// </summary>
    /// <param name="songCidProvider">可提供歌曲 CID 对象的实例。</param>
    /// <returns>指示下载操作是否成功开始的值。</returns>
    public static async Task<bool> StartDownload(ISongCidProvider songCidProvider)
    {
        if (songCidProvider is null)
        {
            throw new ArgumentNullException(nameof(songCidProvider));
        }

        if (songCidProvider is IContentContainer container)
        {
            if (container.IsEmpty)
            {
                return false;
            }
            else if (container.Count >= TooManyItemThresholdCount)
            {
                ContentDialogResult result = await DisplayContentDialog("WarningOccurred".GetLocalized(),
                                                        "DownloadTooManyItemMessage".GetLocalized(),
                                                        "Continue".GetLocalized(), "Cancel".GetLocalized());

                if (result != ContentDialogResult.Primary)
                {
                    return false;
                }
            }
        }

        AggregateExceptionHelper aggregateHelper = new();
        ExceptionBox box = new();
        IAsyncEnumerable<string> cids = songCidProvider.GetSongCidsAsync(box);

        AllFailedHelper allFailedHelper = new();

        await foreach (string songCid in cids)
        {
            try
            {
                allFailedHelper.Start();

                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songCid);
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(songDetail.AlbumCid);

                DownloadService.EnqueueSongDownload(albumDetail, songDetail);

                allFailedHelper.Succeed();
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException && songCidProvider is IContentCorruptible contentCorruptible)
                {
                    contentCorruptible.MarkItemAsCorrupted(songCid);
                }

                aggregateHelper.Record(ex);
            }
        }

        if (box.InboxException is not null)
        {
            aggregateHelper.Record(box.InboxException);
        }

        if (aggregateHelper.HasException)
        {
            bool allFailed = allFailedHelper.IsAllFailed();
            AggregateException aggregate = aggregateHelper.TryGetException(AggregateExceptionHelper.GetDataForCommonUsage(allFailed, songCidProvider));

            if (aggregate.Flatten().InnerExceptions.Any(ex => ex is ArgumentOutOfRangeException))
            {
                if (songCidProvider is ICorruptible corruptible)
                {
                    corruptible.MarkAsCorrupted();
                }
            }

            await DisplayAggregateExceptionErrorDialog(aggregate);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 启动下载收藏夹中歌曲的操作。
    /// </summary>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownloadSongFavorites()
        => await StartDownload(FavoriteService.SongFavoriteList.ToAdapter());

    /// <summary>
    /// 启动下载专辑收藏夹中所有歌曲的操作。
    /// </summary>
    /// <returns>指示下载是否完全成功的值。</returns>
    public static async Task<bool> StartDownloadAlbumFavorites()
        => await StartDownload(FavoriteService.AlbumFavoriteList.ToAdapter());

    /// <summary>
    /// 从专辑详细信息及歌曲详细信息中获取适合用于下载的音乐元数据。
    /// </summary>
    /// <param name="albumDetail">专辑详细信息。</param>
    /// <param name="songDetail">歌曲详细信息。</param>
    /// <returns>一个包含音乐元数据的元组。</returns>
    public static (string AlbumTitle, string SongTitle, string Artist, string Artists) GetMusicMetadataForDownload(AlbumDetail albumDetail, SongDetail songDetail)
    {
        string defaultMsrName = "MSR".GetLocalized();

        string albumTitle = albumDetail.Name?.Trim();
        string songTitle = songDetail.Name?.Trim();
        string artist = songDetail.Artists.FirstOrDefault()?.Trim() ?? defaultMsrName;
        IEnumerable<string> artistSequence = songDetail.Artists.Where(str => !string.IsNullOrWhiteSpace(str)).Select(str => str.Trim());
        string artists = artistSequence.Any() ? string.Join(',', artistSequence) : defaultMsrName;

        return (albumTitle, songTitle, artist, artists);
    }

    /// <summary>
    /// 获取歌曲下载时使用的歌曲文件名和专辑文件夹名。
    /// </summary>
    /// <param name="albumTitle">专辑名称。</param>
    /// <param name="songTitle">歌曲名称。</param>
    /// <param name="artist">艺术家。</param>
    /// <param name="artists">艺术家列表。</param>
    /// <param name="albumDetail">专辑详情。</param>
    /// <param name="musicFileNameCache">歌曲文件名缓存。</param>
    /// <param name="musicAlbumFolderNameCache">专辑文件夹名缓存。</param>
    /// <returns>一个二元组，包含歌曲文件名和专辑文件夹名。</returns>
    /// <exception cref="NotImplementedException">当保存文件名或文件夹名称包含未支持的模板时抛出。</exception>
    public static async Task<(string MusicName, string AlbumFolderName)> AcquireMusicNameAndAlbumFolderName(
            (string AlbumTitle, string SongTitle, string Artist, string Artists) metadataTuple, AlbumDetail albumDetail)
    {
        StringBuilder musicFileNameBuilder = new(DownloadService.MusicFileTemplateString);
        foreach (string template in MusicFilenamePartTemplates)
        {
            string content = template switch
            {
                "{AlbumTitle}" => metadataTuple.AlbumTitle,
                "{SongTitle}" => metadataTuple.SongTitle,
                "{Artist}" => metadataTuple.Artist,
                "{Artists}" => metadataTuple.Artists,
                _ => throw new NotImplementedException("未添加对指定文件名模板的支持。")
            };
            musicFileNameBuilder.Replace(template, content);
        }

        StringBuilder musicAlbumFolderNameBuilder = new(DownloadService.MusicAlbumFolderNameTemplateString);
        foreach (string template in MusicAlbumFolderNamePartTemplates)
        {
            string content = template switch
            {
                "{AlbumTitle}" => metadataTuple.AlbumTitle,
                "{SongIndexOneStart}" => (await GetAlbumIndexAsync(albumDetail)).ToString(),
                "{Artist}" => metadataTuple.Artist,
                "{Artists}" => metadataTuple.Artists,
                _ => throw new NotImplementedException("未添加对指定文件夹名称模板的支持。")
            };
            musicAlbumFolderNameBuilder.Replace(template, content);

            static async Task<int> GetAlbumIndexAsync(AlbumDetail albumDetail)
            {
                CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> albums = await GetOrFetchAlbums();

                int albumCount = albums.CollectionSource.Count;
                for (int i = 0; i < albumCount; i++)
                {
                    AlbumInfo info = albums.CollectionSource.ElementAt(i);
                    if (info.Cid == albumDetail.Cid)
                    {
                        return albumCount - i;
                    }
                }

                return -1;
            }
        }

        string musicFileName = musicFileNameBuilder.ToString();
        string musicAlbumFolderName = musicAlbumFolderNameBuilder.ToString();

        if (DownloadService.ReplaceInvalidCharInFileName)
        {
            musicFileName = ReplaceInvalidFileNameChars(musicFileName);
            musicAlbumFolderName = ReplaceInvalidFileNameChars(musicAlbumFolderName);
        }
        else
        {
            foreach (string invalidCharStr in InvalidFileNameCharsStringArray)
            {
                musicFileName = musicFileName.Replace(invalidCharStr, string.Empty);
                musicAlbumFolderName = musicAlbumFolderName.Replace(invalidCharStr, string.Empty);
            }
        }
        musicAlbumFolderName = RemoveOrReplaceDotEndingInFolderName(musicAlbumFolderName);

        return (musicFileName, musicAlbumFolderName);
    }
}
