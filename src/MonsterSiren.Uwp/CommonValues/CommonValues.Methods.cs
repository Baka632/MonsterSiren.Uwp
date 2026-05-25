#region 请保留，发布模式需要
using Microsoft.Services.Store.Engagement;
#endregion
using System.Text;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Toolkit.Uwp.UI.Controls;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    private static readonly SemaphoreSlim _GetOrFetchAlbumsSemaphore = new(1);
    private static readonly SemaphoreSlim _LoadAndCacheAlbumSemaphore = new(10);

    /// <summary>
    /// 显示一个对话框。
    /// </summary>
    /// <param name="title">对话框的标题。</param>
    /// <param name="message">对话框的消息。</param>
    /// <param name="primaryButtonText">主按钮文本。</param>
    /// <param name="closeButtonText">关闭按钮文本。</param>
    /// <param name="secondaryButtonText">第二按钮文本。</param>
    /// <param name="defaultButton">默认按钮。</param>
    /// <returns>记录结果的 <see cref="ContentDialogResult"/>。</returns>
    public static async Task<ContentDialogResult> DisplayContentDialog(
        string title, string message, string primaryButtonText = "", string closeButtonText = "",
        string secondaryButtonText = "", ContentDialogButton defaultButton = ContentDialogButton.None)
    {
        ContentDialogResult result = await UIThreadHelper.RunOnUIThread(async () =>
        {
            ContentDialog contentDialog = new()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                SecondaryButtonText = secondaryButtonText,
                DefaultButton = defaultButton
            };

            // 防止出现多个对话框弹出而导致的异常
            while (VisualTreeHelper.GetOpenPopups(Window.Current).Count > 0)
            {
                await Task.Delay(500);
            }

            return await contentDialog.ShowAsync();
        });

        return result;
    }

    /// <summary>
    /// 将字符串中不能作为文件名的部分字符替换为相近的合法字符。
    /// </summary>
    /// <param name="fileName">文件名字符串。</param>
    /// <remarks>
    /// <para>
    /// 本方法替换了以下字符：
    /// </para>
    /// <para>
    /// " ? : &lt; &gt; | * / \
    /// </para>
    /// <para>
    /// 其他在 <see cref="Path.GetInvalidFileNameChars"/> 方法中出现的字符将被删去。
    /// </para>
    /// </remarks>
    /// <returns>新的字符串。</returns>
    public static string ReplaceInvalidFileNameChars(string fileName)
    {
        StringBuilder stringBuilder = new(fileName);
        stringBuilder.Replace('"', '\'');
        stringBuilder.Replace('?', '？');
        stringBuilder.Replace(':', '：');
        stringBuilder.Replace('<', '[');
        stringBuilder.Replace('>', ']');
        stringBuilder.Replace('|', 'I');
        stringBuilder.Replace('*', '★');
        stringBuilder.Replace('/', '↗');
        stringBuilder.Replace('\\', '↘');

        foreach (string invalidCharStr in InvalidFileNameCharsStringArray)
        {
            stringBuilder.Replace(invalidCharStr, string.Empty);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 将文件夹名称中结尾的全部“.”替换为指定的非以“.”结尾的字符串，或直接删除结尾全部的“.”。
    /// </summary>
    /// <param name="folderName">文件夹名称。</param>
    /// <param name="replaceString">替换字符串。当此参数为 <see langword="null"/> 时，则直接删除“.”字符。</param>
    /// <returns>修改后的字符串，其前导及后导空白字符将被删除。</returns>
    /// <exception cref="ArgumentException"><paramref name="folderName"/> 或 <paramref name="replaceString"/> 的值无效。</exception>
    public static string RemoveOrReplaceDotEndingInFolderName(string folderName, string replaceString = null)
    {
        folderName = folderName?.Trim();
        replaceString = replaceString?.Trim();

        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new ArgumentException($"“{nameof(folderName)}”不能为 null 或空白。", nameof(folderName));
        }
        else if (folderName.IndexOfAny(InvalidFileNameChars) != -1 || folderName.All(chr => chr == '.'))
        {
            throw new ArgumentException("文件夹名称无效。", nameof(folderName));
        }
        else if (!folderName.EndsWith('.'))
        {
            return folderName;
        }
        else if (replaceString != null &&
            (replaceString.IndexOfAny(InvalidFileNameChars) != -1 || replaceString.EndsWith('.')))
        {
            throw new ArgumentException("替换字符串无效。", nameof(replaceString));
        }

        string newFolderName = folderName.TrimEnd('.');

        if (string.IsNullOrWhiteSpace(replaceString))
        {
            return newFolderName;
        }
        else
        {
            return $"{newFolderName}{replaceString}";
        }
    }

    /// <summary>
    /// 创建“添加到”的 <see cref="MenuFlyoutSubItem"/>。
    /// </summary>
    /// <param name="addToNowPlayingCommand">“添加到正在播放”命令。</param>
    /// <param name="addToNowPlayingCommandParameter">“添加到正在播放”命令的参数。</param>
    /// <param name="playlistCommand">“添加到播放列表”命令。</param>
    /// <param name="optionalModel">可选的模型类，用于防止播放列表添加自身。</param>
    /// <returns>一个 <see cref="MenuFlyoutSubItem"/> 实例。</returns>
    public static MenuFlyoutSubItem CreateAddToFlyoutSubItem(ICommand addToNowPlayingCommand, object addToNowPlayingCommandParameter, ICommand playlistCommand, Func<Playlist, CommandParameter> playlistCommandParameterFactory, Playlist optionalModel = null)
    {
        MenuFlyoutSubItem mainSubItem = new()
        {
            Icon = new SymbolIcon(Symbol.Add),
            Text = "AddToPlaylistOrNowPlayingLiteral".GetLocalized()
        };
        MenuFlyoutItem addToNowPlayingItem = CreateAddToNowPlayingItem(addToNowPlayingCommand, addToNowPlayingCommandParameter);
        MenuFlyoutSubItem playlistSubItem = CreateAddToPlaylistSubItem(playlistCommand, playlistCommandParameterFactory, optionalModel);

        mainSubItem.Items.Add(addToNowPlayingItem);
        mainSubItem.Items.Add(playlistSubItem);

        return mainSubItem;
    }

    /// <summary>
    /// 创建一个“添加到正在播放”的 <see cref="MenuFlyoutItem"/>。
    /// </summary>
    /// <param name="addToNowPlayingCommand">“添加到正在播放”命令。</param>
    /// <param name="addToNowPlayingCommandParameter">“添加到正在播放”命令的参数。</param>
    /// <returns>一个 <see cref="MenuFlyoutItem"/> 实例。</returns>
    public static MenuFlyoutItem CreateAddToNowPlayingItem(ICommand addToNowPlayingCommand, object addToNowPlayingCommandParameter)
    {
        return new()
        {
            Text = "NowPlayingLiteral".GetLocalized(),
            Icon = new SymbolIcon(Symbol.MusicInfo),
            Command = addToNowPlayingCommand,
            CommandParameter = addToNowPlayingCommandParameter
        };
    }

    public static MenuFlyoutSubItem CreateAddToPlaylistSubItem(ICommand playlistCommand, Func<Playlist, CommandParameter> playlistCommandParameterFactory, Playlist optionalModel = null, MenuFlyoutSubItem playlistSubItem = null)
    {
        playlistSubItem ??= new()
        {
            Icon = new SymbolIcon(Symbol.List),
            Text = "AddToPlaylistTextLiteral".GetLocalized(),
        };
        InitializeAddToPlaylistSubItem(playlistCommand, playlistCommandParameterFactory, optionalModel, playlistSubItem);

        return playlistSubItem;
    }

    public static void InitializeAddToPlaylistSubItem(ICommand playlistCommand, Func<Playlist, CommandParameter> playlistCommandParameterFactory, Playlist optionalModel, MenuFlyoutSubItem playlistSubItem)
    {
        playlistSubItem.Items.Clear();

        if (PlaylistService.TotalPlaylists.Count > 0)
        {
            playlistSubItem.IsEnabled = true;

            foreach (Playlist playlist in PlaylistService.TotalPlaylists)
            {
                MenuFlyoutItem item = CreateMenuFlyoutItemByPlaylist(playlist, playlistCommand, playlistCommandParameterFactory, optionalModel);
                playlistSubItem.Items.Add(item);
            }
        }
        else
        {
            playlistSubItem.IsEnabled = false;
        }
    }

    private static MenuFlyoutItem CreateMenuFlyoutItemByPlaylist(Playlist playlist, ICommand playlistCommand, Func<Playlist, CommandParameter> playlistCommandParameterFactory, Playlist optionalModel)
    {
        MenuFlyoutItem flyoutItem = new()
        {
            DataContext = playlist,
            Text = playlist.Title,
            Icon = new FontIcon()
            {
                Glyph = "\uEC4F"
            },
            Command = playlistCommand,
            CommandParameter = playlistCommandParameterFactory(playlist),
            IsEnabled = playlist != optionalModel,
        };

        return flyoutItem;
    }

    /// <summary>
    /// 显示网络错误的提示对话框。
    /// </summary>
    public static async Task DisplayInternetErrorDialog()
    {
        await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
    }

    private static async Task DisplaySongOrAlbumCidCorruptDialog()
    {
        await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
    }

    private static async Task DisplaySomeSongOrAlbumCidCorruptDialog()
    {
        await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SomeSongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
    }

    private static async Task DisplayPlaylistEmptyDialog()
    {
        await DisplayContentDialog("NoSongPlayed_Title".GetLocalized(), "NoSongPlayed_PlaylistEmpty".GetLocalized(), "OK".GetLocalized());
    }

    /// <summary>
    /// 显示针对 <see cref="AggregateException"/> 的错误信息。
    /// </summary>
    /// <param name="aggregate">指定的 <see cref="AggregateException"/>。</param>
    private static async Task DisplayAggregateExceptionErrorDialog(AggregateException aggregate)
    {
        StringBuilder builder = new(aggregate.InnerExceptions.Count * 10);

        bool allFailed = false;
        if (aggregate.Data.Contains("AllFailed"))
        {
            allFailed = aggregate.Data["AllFailed"] is bool val && val;
        }

        object errorPlayItem = null;
        if (aggregate.Data.Contains("PlayItem"))
        {
            errorPlayItem = aggregate.Data["PlayItem"];
        }

        aggregate = aggregate.Flatten();

        if (aggregate.InnerExceptions.Any(ex => ex is HttpRequestException))
        {
            builder.AppendLine("InternetErrorMessage".GetLocalized());
        }

        if (aggregate.InnerExceptions.Any(ex => ex is ArgumentOutOfRangeException))
        {
            string message;
            if (allFailed)
            {
                message = errorPlayItem is Playlist playlist
                    ? string.Format("PlaylistCorruptMessage".GetLocalized(), playlist.Title)
                    : "SongOrAlbumCidCorruptMessage".GetLocalized();
            }
            else
            {
                message = errorPlayItem is Playlist playlist
                    ? string.Format("SomePlaylistItemCorruptMessage".GetLocalized(), playlist.Title)
                    : "SomeSongOrAlbumCidCorruptMessage".GetLocalized();
            }

            builder.AppendLine(message);
        }

        foreach (Exception ex in aggregate.InnerExceptions.Where(ex => ex is not HttpRequestException and not ArgumentOutOfRangeException))
        {
            builder.AppendLine(ex.Message);
        }

        string dialogTitle = allFailed ? "ErrorOccurred".GetLocalized() : "WarningOccurred".GetLocalized();
        string dialogMessage = builder.ToString().Trim();
        await DisplayContentDialog(dialogTitle, dialogMessage, closeButtonText: "Close".GetLocalized());
    }

    /// <summary>
    /// 从服务器中获取全部专辑的信息，并填充艺术家信息及缓存封面信息。
    /// </summary>
    /// <remarks>
    /// 本方法与 <see cref="GetOrFetchAlbums"/> 不同的是，本方法将只从服务器获取最新数据，而不进行缓存。并且本方法返回的是 <see cref="IEnumerable{T}"/> 序列。
    /// </remarks>
    /// <returns>包含全部专辑信息的 <see cref="AlbumInfo"/> 列表。</returns>
    public async static Task<IEnumerable<AlbumInfo>> GetAlbumsFromServer()
    {
        List<AlbumInfo> albums = await Task.Run(async () =>
        {
            List<AlbumInfo> albumList = [.. (await AlbumService.GetAllAlbumsAsync())];
            await MsrModelsHelper.TryFillArtistAndCachedCoverForAlbum(albumList);

            return albumList;
        });

        return albums;
    }

    /// <summary>
    /// 获取类型为 <see cref="CustomIncrementalLoadingCollection{TSource, IType}"/> 的 <see cref="AlbumInfo"/> 集合。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此方法与 <see cref="GetAlbumsFromServer"/> 方法不同的是，本方法会进行缓存，并将 <see cref="IEnumerable{T}"/> 转换为 <see cref="CustomIncrementalLoadingCollection{TSource, IType}"/>。
    /// </para>
    /// <para>
    /// 请注意，在使用 <see cref="IEnumerable{T}"/> 相关的扩展方法时，请务必使用 <see cref="CustomIncrementalLoadingCollection{TSource, IType}.CollectionSource"/> 成员中的集合来获取正确结果。否则，由于增量加载的缘故，<see cref="IEnumerable{T}"/> 相关的扩展方法可能会出现预期外的结果。
    /// </para>
    /// </remarks>
    /// <returns>一个类型为 <see cref="CustomIncrementalLoadingCollection{TSource, IType}"/> 的 <see cref="AlbumInfo"/> 集合</returns>
    public async static Task<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>> GetOrFetchAlbums()
    {
        await _GetOrFetchAlbumsSemaphore.WaitAsync();

        try
        {
            if (MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.TryGetData(AlbumInfoCacheKey, out CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> infos))
            {
                return infos;
            }
            else
            {
                IEnumerable<AlbumInfo> albums = await GetAlbumsFromServer();

                CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> incrementalCollection = CreateAlbumInfoIncrementalLoadingCollection(albums);

                if (incrementalCollection.CollectionSource.Count != 0)
                {
                    MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.Store(AlbumInfoCacheKey, incrementalCollection);
                }

                return incrementalCollection;
            }
        }
        finally
        {
            _GetOrFetchAlbumsSemaphore.Release();
        }
    }

    /// <summary>
    /// 为 <see cref="AlbumInfo"/> 列表创建实现增量加载的 <see cref="CustomIncrementalLoadingCollection{TSource, IType}"/> 集合。
    /// </summary>
    /// <remarks>
    /// 请注意，在使用 <see cref="IEnumerable{T}"/> 相关的扩展方法时，请务必使用 <see cref="CustomIncrementalLoadingCollection{TSource, IType}.CollectionSource"/> 成员来获取正确结果。否则，由于增量加载的缘故，<see cref="IEnumerable{T}"/> 相关的扩展方法可能会出现预期外的结果。
    /// </remarks>
    /// <param name="albums">包含专辑信息的 <see cref="AlbumInfo"/> 列表。</param>
    /// <returns>新的 <see cref="CustomIncrementalLoadingCollection{TSource, IType}"/> 实例。</returns>
    public static CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> CreateAlbumInfoIncrementalLoadingCollection(IEnumerable<AlbumInfo> albums)
    {
        int loadCount = EnvironmentHelper.IsWindowsMobile ? 5 : 10;
        return new(new AlbumInfoSource(albums), loadCount);
    }

    /// <summary>
    /// 为指定的 <see cref="ImageEx"/> 加载并缓存专辑封面。
    /// </summary>
    /// <param name="image">指定的 <see cref="ImageEx"/> 实例。</param>
    public static async Task LoadAndCacheMusicCover(ImageEx image, AlbumInfo info)
    {
        bool needModifySource = false;

        if (image.Source is not BitmapImage bitmapImage)
        {
            needModifySource = true;
            bitmapImage = new BitmapImage()
            {
                DecodePixelHeight = 250,
                DecodePixelType = DecodePixelType.Logical,
                DecodePixelWidth = 250
            };
        }

        bool isSuccess = await LoadAndCacheMusicCoverCore(bitmapImage, info.CoverUrl, info.Cid, () => (AlbumInfo)image.DataContext == info);

        lock (image)
        {
            if (needModifySource && isSuccess)
            {
                image.Source = bitmapImage;
            }
        }
    }

    /// <summary>
    /// 为指定的 <see cref="ImageEx"/> 加载并缓存专辑封面。
    /// </summary>
    /// <param name="image">指定的 <see cref="ImageEx"/> 实例。</param>
    public static async Task LoadAndCacheMusicCover(ImageEx image, AlbumFavoriteItem item)
    {
        bool needModifySource = false;

        if (image.Source is not BitmapImage bitmapImage)
        {
            needModifySource = true;
            bitmapImage = new BitmapImage()
            {
                DecodePixelHeight = 250,
                DecodePixelType = DecodePixelType.Logical,
                DecodePixelWidth = 250
            };
        }

        AlbumDetail detail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
        bool isSuccess = await LoadAndCacheMusicCoverCore(bitmapImage, detail.CoverUrl, detail.Cid, () => (AlbumFavoriteItem)image.DataContext == item);

        lock (image)
        {
            if (needModifySource && isSuccess)
            {
                image.Source = bitmapImage;
            }
        }
    }

    private static async Task<bool> LoadAndCacheMusicCoverCore(BitmapImage bitmapImage, string coverUrl, string albumCid, Func<bool> detectCanUpdateSource)
    {
        if (bitmapImage is null)
        {
            throw new ArgumentNullException(nameof(bitmapImage));
        }

        if (detectCanUpdateSource is null)
        {
            throw new ArgumentNullException(nameof(detectCanUpdateSource));
        }

        try
        {
            Uri fileCoverUri = await GetMusicCoverUriCore(coverUrl, albumCid);

            if (detectCanUpdateSource())
            {
                bitmapImage.UriSource = fileCoverUri;
                return true;
            }
        }
        catch (Exception ex)
        {
#if RELEASE
                try
                {
                    StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
                    logger.Log("缓存封面图像失败");
                }
                catch
                {
                    // Enough!
                }
#else
            Debug.WriteLine(ex);
            Debugger.Break();
#endif
        }

        return false;
    }

    private static async Task<Uri> GetMusicCoverUriCore(string coverUrl, string albumCid)
    {
        Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumCid);
        if (fileCoverUri is null)
        {
            await _LoadAndCacheAlbumSemaphore.WaitAsync();
            try
            {
                fileCoverUri = await Task.Run(async () => await FileCacheHelper.StoreAlbumCoverByUriAndCid(coverUrl, albumCid));
            }
            finally
            {
                _LoadAndCacheAlbumSemaphore.Release();
            }
        }
        return fileCoverUri;
    }

    /// <summary>
    /// 从指定的对象中获取 <see cref="ISongCidProvider"/>。
    /// </summary>
    /// <param name="source">指定的对象。</param>
    /// <returns>一个 <see cref="ISongCidProvider"/> 实例。</returns>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> 的类型是 PlaylistItem，这种情况下请提前将 PlaylistItem 转换为 ISongCidProvider，之后再传入此方法。</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> 无法转换为 <see cref="ISongCidProvider"/>。</exception>
    public static ISongCidProvider GetSongCidProvider(object source)
    {
        return source switch
        {
            AlbumInfo albumInfo => albumInfo.ToAdapter(),
            IEnumerable<AlbumInfo> albumInfos => albumInfos.ToAdapter(),
            AlbumDetail detail => detail.ToAdapter(),
            SongInfo songInfo => songInfo.ToAdapter(),
            IEnumerable<SongInfo> songInfos => songInfos.ToAdapter(),
            AlbumFavoriteItem albumFavoriteItem => albumFavoriteItem.ToAdapter(),
            IEnumerable<AlbumFavoriteItem> albumFavoriteItems => albumFavoriteItems.ToAdapter(),
            AlbumFavoriteList albumFavoriteList => albumFavoriteList.ToAdapter(),
            Playlist playlist => playlist.ToAdapter(),
            PlaylistItem _ => throw new InvalidOperationException("对于 PlaylistItem，请提前转换为 ISongCidProvider，然后再传入此方法。"),
            IEnumerable<PlaylistItem> playlistItems => playlistItems.ToAdapter(),
            SongFavoriteItem songFavoriteItem => songFavoriteItem.ToAdapter(),
            IEnumerable<SongFavoriteItem> songFavoriteItems => songFavoriteItems.ToAdapter(),
            SongFavoriteList songFavoriteList => songFavoriteList.ToAdapter(),
            ISongCidProvider songCidProvider => songCidProvider,
            Func<ISongCidProvider> songCidProviderFactory => songCidProviderFactory.Invoke(),
            _ => throw new ArgumentException("指定的对象无法转换为 ISongCidProvider。", nameof(source)),
        };
    }

    /// <summary>
    /// 从指定的对象中获取 <see cref="IFavoriteAddable"/>。
    /// </summary>
    /// <param name="source">指定的对象。</param>
    /// <returns>一个 <see cref="IFavoriteAddable"/> 实例。</returns>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> 的类型是 PlaylistItem，这种情况下请提前将 PlaylistItem 转换为 ISongCidProvider，之后再传入此方法。</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> 无法转换为 <see cref="IFavoriteAddable"/>。</exception>
    public static IFavoriteAddable GetFavoriteAddable(object source)
    {
        return source switch
        {
            AlbumInfo albumInfo => albumInfo.ToAdapter(),
            IEnumerable<AlbumInfo> albumInfos => albumInfos.ToAdapter(),
            AlbumDetail detail => detail.ToAdapter(),
            SongInfo songInfo => songInfo.ToAdapter(),
            IEnumerable<SongInfo> songInfos => songInfos.ToAdapter(),
            AlbumFavoriteItem albumFavoriteItem => albumFavoriteItem.ToAdapter(),
            IEnumerable<AlbumFavoriteItem> albumFavoriteItems => albumFavoriteItems.ToAdapter(),
            PlaylistItem _ => throw new InvalidOperationException("对于 PlaylistItem，请提前转换为 ISongCidProvider，然后再传入此方法。"),
            IEnumerable<PlaylistItem> playlistItems => playlistItems.ToAdapter(),
            SongFavoriteItem songFavoriteItem => songFavoriteItem.ToAdapter(),
            IEnumerable<SongFavoriteItem> songFavoriteItems => songFavoriteItems.ToAdapter(),
            IFavoriteAddable favoriteAddable => favoriteAddable,
            Func<ISongCidProvider> songCidProviderFactory when songCidProviderFactory.Invoke() is IFavoriteAddable addable => addable,
            _ => throw new ArgumentException("指定的对象无法转换为 IFavoriteAddable。", nameof(source)),
        };
    }

    /// <summary>
    /// 从指定的对象中获取 <see cref="INameProvider"/>。
    /// </summary>
    /// <param name="source">指定的对象。</param>
    /// <returns>一个 <see cref="INameProvider"/> 实例。</returns>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> 的类型是 PlaylistItem，这种情况下请提前将 PlaylistItem 转换为 INameProvider，之后再传入此方法。</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> 无法转换为 <see cref="INameProvider"/>。</exception>
    public static INameProvider GetNameProvider(object source)
    {
        return source switch
        {
            AlbumInfo albumInfo => albumInfo.ToAdapter(),
            AlbumDetail detail => detail.ToAdapter(),
            SongInfo songInfo => songInfo.ToAdapter(),
            AlbumFavoriteItem albumFavoriteItem => albumFavoriteItem.ToAdapter(),
            Playlist playlist => playlist.ToAdapter(),
            PlaylistItem _ => throw new InvalidOperationException("对于 PlaylistItem，请提前转换为 ISongCidProvider，然后再传入此方法。"),
            SongFavoriteItem songFavoriteItem => songFavoriteItem.ToAdapter(),
            INameProvider nameProvider => nameProvider,
            Func<ISongCidProvider> songCidProviderFactory when songCidProviderFactory.Invoke() is INameProvider name => name,
            _ => throw new ArgumentException("指定的对象无法转换为 ISongCidProvider。", nameof(source)),
        };
    }
}