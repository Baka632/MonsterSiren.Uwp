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
    /// 创建“添加到”的 <see cref="MenuFlyoutSubItem"/>。
    /// </summary>
    /// <param name="addToNowPlayingCommand">“添加到正在播放”命令。</param>
    /// <param name="addToNowPlayingCommandParameter">“添加到正在播放”命令的参数。</param>
    /// <param name="playlistCommand">“添加到播放列表”命令。</param>
    /// <param name="optionalModel">可选的模型类，用于防止播放列表添加自身。</param>
    /// <returns>一个 <see cref="MenuFlyoutSubItem"/> 实例。</returns>
    public static MenuFlyoutSubItem CreateAddToFlyoutSubItem(ICommand addToNowPlayingCommand, object addToNowPlayingCommandParameter, ICommand playlistCommand, Playlist optionalModel = null)
    {
        MenuFlyoutSubItem mainSubItem = new()
        {
            Icon = new SymbolIcon(Symbol.Add),
            Text = "AddToPlaylistOrNowPlayingLiteral".GetLocalized()
        };
        MenuFlyoutItem addToNowPlayingItem = CreateAddToNowPlayingItem(addToNowPlayingCommand, addToNowPlayingCommandParameter);
        MenuFlyoutSubItem playlistSubItem = CreateAddToPlaylistSubItem(playlistCommand, optionalModel);

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

    /// <summary>
    /// 创建一个“添加到播放列表”的 <see cref="MenuFlyoutSubItem"/>。
    /// </summary>
    /// <param name="playlistCommand">“添加到播放列表”命令。</param>
    /// <param name="optionalModel">可选的模型类，用于防止播放列表添加自身。</param>
    /// <returns>一个 <see cref="MenuFlyoutSubItem"/> 实例。</returns>
    public static MenuFlyoutSubItem CreateAddToPlaylistSubItem(ICommand playlistCommand, Playlist optionalModel = null)
    {
        MenuFlyoutSubItem playlistSubItem = new()
        {
            Icon = new SymbolIcon(Symbol.List),
            Text = "AddToPlaylistTextLiteral".GetLocalized(),
        };

        if (PlaylistService.TotalPlaylists.Count > 0)
        {
            playlistSubItem.IsEnabled = true;

            foreach (Playlist playlist in PlaylistService.TotalPlaylists)
            {
                MenuFlyoutItem item = CreateMenuFlyoutItemByPlaylist(playlist, playlistCommand, optionalModel);
                playlistSubItem.Items.Add(item);
            }
        }
        else
        {
            playlistSubItem.IsEnabled = false;
        }

        return playlistSubItem;
    }

    private static MenuFlyoutItem CreateMenuFlyoutItemByPlaylist(Playlist playlist, ICommand playlistCommand, Playlist optionalModel)
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
            CommandParameter = playlist,
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
    /// 显示针对 <see cref="AggregateException"/> 的错误信息
    /// </summary>
    /// <param name="aggregate">指定的 <see cref="AggregateException"/></param>
    private static async Task DisplayAggregateExceptionErrorDialog(AggregateException aggregate)
    {
        StringBuilder builder = new(aggregate.InnerExceptions.Count * 10);

        if (aggregate.Data["AllFailed"] is not bool allFailed)
        {
            allFailed = false;
        }
        object errorPlayItem = aggregate.Data["PlayItem"];

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

                if (incrementalCollection.CollectionSource.AlbumInfos.Any())
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
    /// 请注意，在使用 <see cref="IEnumerable{T}"/> 相关的扩展方法时，请务必使用 <see cref="CustomIncrementalLoadingCollection{TSource, IType}.CollectionSource"/> 成员中的集合来获取正确结果。否则，由于增量加载的缘故，<see cref="IEnumerable{T}"/> 相关的扩展方法可能会出现预期外的结果。
    /// </remarks>
    /// <param name="albums">包含专辑信息的 <see cref="AlbumInfo"/> 列表。</param>
    /// <returns>新的 <see cref="CustomIncrementalLoadingCollection{TSource, IType}"/> 实例。</returns>
    public static CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> CreateAlbumInfoIncrementalLoadingCollection(IEnumerable<AlbumInfo> albums)
    {
        int loadCount = EnvironmentHelper.IsWindowsMobile ? 5 : 10;
        return new(new AlbumInfoSource(albums), loadCount);
    }

    /// <summary>
    /// 为指定的 <see cref="Image"/> 加载并缓存专辑封面。
    /// </summary>
    /// <param name="image">指定的 <see cref="Image"/> 实例。</param>
    public static async Task LoadAndCacheMusicCover(Image image, AlbumInfo info)
    {
        if (image.Source is not BitmapImage bitmapImage)
        {
            bitmapImage = new BitmapImage();
            image.Source = bitmapImage;
        }
        await LoadAndCacheMusicCoverCore(bitmapImage, info, () => ReferenceEquals(image.Source, bitmapImage) && (AlbumInfo)image.DataContext == info);
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

        bool isSuccess = await LoadAndCacheMusicCoverCore(bitmapImage, info, () => (AlbumInfo)image.DataContext == info);

        lock (image)
        {
            if (needModifySource && isSuccess)
            {
                image.Source = bitmapImage;
            }
        }
    }

    private static async Task<bool> LoadAndCacheMusicCoverCore(BitmapImage bitmapImage, AlbumInfo info, Func<bool> detectCanUpdateSource)
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
            Uri fileCoverUri = await GetMusicCoverUriCore(info.Cid, info);

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

    private static async Task<Uri> GetMusicCoverUriCore(string albumCid, AlbumInfo info)
    {
        Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumCid);
        if (fileCoverUri is null)
        {
            await _LoadAndCacheAlbumSemaphore.WaitAsync();
            try
            {
                fileCoverUri = await Task.Run(async () => await FileCacheHelper.StoreAlbumCoverAsync(info));
            }
            finally
            {
                _LoadAndCacheAlbumSemaphore.Release();
            }
        }
        return fileCoverUri;
    }
}