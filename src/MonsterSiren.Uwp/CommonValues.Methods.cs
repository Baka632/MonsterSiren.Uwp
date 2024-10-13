using System.Text;
using System.Net.Http;
using System.Windows.Input;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 显示一个对话框
    /// </summary>
    /// <param name="title">对话框的标题</param>
    /// <param name="message">对话框的消息</param>
    /// <param name="primaryButtonText">主按钮文本</param>
    /// <param name="closeButtonText">关闭按钮文本</param>
    /// <param name="secondaryButtonText">第二按钮文本</param>
    /// <param name="defaultButton">默认按钮</param>
    /// <returns>记录结果的 <see cref="ContentDialogResult"/></returns>
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
    /// 将字符串中不能作为文件名的部分字符替换为相近的合法字符
    /// </summary>
    /// <param name="fileName">文件名字符串</param>
    /// <returns>新的字符串</returns>
    public static string ReplaceInvaildFileNameChars(string fileName)
    {
        StringBuilder stringBuilder = new(fileName);
        stringBuilder.Replace('"', '\'');
        stringBuilder.Replace('<', '[');
        stringBuilder.Replace('>', ']');
        stringBuilder.Replace('|', 'I');
        stringBuilder.Replace(':', '：');
        stringBuilder.Replace('*', '★');
        stringBuilder.Replace('?', '？');
        stringBuilder.Replace('/', '↗');
        stringBuilder.Replace('\\', '↘');
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 创建“添加到”的 <see cref="MenuFlyoutSubItem"/>
    /// </summary>
    /// <param name="addToNowPlayingCommand">“添加到正在播放”命令</param>
    /// <param name="addToNowPlayingCommandParameter">命令参数</param>
    /// <param name="playlistCommand">添加到播放列表命令</param>
    /// <param name="optionalModel">可选的模型类，用于防止播放列表添加自身</param>
    /// <returns>一个 <see cref="MenuFlyoutSubItem"/> 实例</returns>
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
    /// 创建一个“添加到正在播放”的 <see cref="MenuFlyoutItem"/>
    /// </summary>
    /// <param name="addToNowPlayingCommand">“添加到正在播放”命令</param>
    /// <param name="addToNowPlayingCommandParameter">命令参数</param>
    /// <returns>一个 <see cref="MenuFlyoutItem"/> 实例</returns>
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
    /// 创建一个“添加到播放列表”的 <see cref="MenuFlyoutSubItem"/>
    /// </summary>
    /// <param name="playlistCommand">“添加到播放列表”命令</param>
    /// <param name="optionalModel">可选的模型类，用于防止播放列表添加自身</param>
    /// <returns>一个 <see cref="MenuFlyoutSubItem"/> 实例</returns>
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
    /// 播放 <see cref="AlbumInfo"/> 中的歌曲
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(AlbumInfo albumInfo)
    {
        try
        {
            ExceptionBox box = new();
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="AlbumInfo"/> 序列
    /// </summary>
    /// <param name="albumInfos">一个 <see cref="AlbumInfo"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumInfos.ToArray(), box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="AlbumDetail"/> 中的歌曲
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> StartPlay(AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="SongInfo"/> 所表示的歌曲
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 的实例</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(songDetail, albumDetail);

            MusicService.ReplaceMusic(item);

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 播放一个 <see cref="SongInfo"/> 序列，其中的歌曲应当属于 <paramref name="albumDetail"/> 所表示的专辑
    /// </summary>
    /// <param name="songInfos">一个歌曲序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> StartPlay(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (songInfos.Any() != true)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(songInfos.ToArray(), albumDetail, box);
            await MusicService.ReplaceMusic(items);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="PlaylistItem"/> 所表示的播放列表项
    /// </summary>
    /// <param name="playlistItem"><see cref="PlaylistItem"/> 所表示的播放列表项</param>
    /// <param name="playlist">播放列表项所属的播放列表，此参数用于在 <paramref name="playlistItem"/> 无效时对播放列表进行更新。</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(PlaylistItem playlistItem, Playlist playlist)
    {
        try
        {
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(playlistItem.SongCid);
            MusicService.ReplaceMusic(item);

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();

            int targetIndex = playlist.Items.IndexOf(playlistItem);
            if (targetIndex != -1)
            {
                playlist.Items[targetIndex] = playlistItem with { IsCorruptedItem = true };
            }

            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 播放一个 <see cref="PlaylistItem"/> 序列
    /// </summary>
    /// <param name="playlistItems">一个 <see cref="PlaylistItem"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(IEnumerable<PlaylistItem> playlistItems)
    {
        if (!playlistItems.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(playlistItems.ToArray(), box);
            await MusicService.ReplaceMusic(items);
            box.Unbox();

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="Playlist"/> 所表示的播放列表
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(Playlist playlist)
    {
        if (playlist.SongCount == 0)
        {
            await DisplayContentDialog("NoSongPlayed_Title".GetLocalized(),
                                                    "NoSongPlayed_PlaylistEmpty".GetLocalized(),
                                                    "OK".GetLocalized());
        }
        else
        {
            try
            {
                await PlaylistService.PlayForPlaylistAsync(playlist);

                return true;
            }
            catch (AggregateException ex)
            {
                MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
                await DisplayAggregateExceptionError(ex);
            }
        }

        return false;
    }

    /// <summary>
    /// 播放 <see cref="Playlist"/> 序列
    /// </summary>
    /// <param name="playlists">一个 <see cref="Playlist"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartPlay(IEnumerable<Playlist> playlists)
    {
        if (!playlists.Any())
        {
            return false;
        }

        bool noSongInPlaylists = true;
        foreach (Playlist playlist in playlists)
        {
            if (playlist.SongCount > 0)
            {
                noSongInPlaylists = false;
                break;
            }
        }

        if (noSongInPlaylists)
        {
            await DisplayContentDialog("NoSongPlayed_Title".GetLocalized(),
                                                    "NoSongPlayed_PlaylistEmpty".GetLocalized(),
                                                    "OK".GetLocalized());
            return false;
        }

        try
        {
            await PlaylistService.PlayForPlaylistsAsync(playlists);

            return true;
        }
        catch (AggregateException ex)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayAggregateExceptionError(ex);
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="AlbumInfo"/> 中的歌曲加入到正在播放列表中
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToNowPlaying(AlbumInfo albumInfo)
    {
        try
        {
            ExceptionBox box = new();
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);

            await MusicService.AddMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="AlbumInfo"/> 序列加入到正在播放列表中
    /// </summary>
    /// <param name="albumInfos">一个 <see cref="AlbumInfo"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumInfos.ToArray(), box);

            await MusicService.AddMusic(items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="AlbumDetail"/> 中的歌曲加入到正在播放列表中
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> AddToNowPlaying(AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(albumDetail, box);
            await MusicService.AddMusic(items);
            box.Unbox();

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="SongInfo"/> 加入到正在播放列表中
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> AddToNowPlaying(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(songDetail, albumDetail);

            MusicService.AddMusic(item);

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="SongInfo"/> 序列添加到正在播放列表中，其中的歌曲应当属于 <paramref name="albumDetail"/> 所表示的专辑
    /// </summary>
    /// <param name="songInfos">一个歌曲序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (!songInfos.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(songInfos.ToArray(), albumDetail, box);
            await MusicService.AddMusic(items);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="PlaylistItem"/> 添加到正在播放列表中
    /// </summary>
    /// <param name="playlistItem">一个 <see cref="PlaylistItem"/> 实例</param>
    /// <param name="playlist">播放列表项所属的播放列表，此参数用于在 <paramref name="playlistItem"/> 无效时对播放列表进行更新。</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToNowPlaying(PlaylistItem playlistItem, Playlist playlist)
    {
        try
        {
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(playlistItem.SongCid);
            MusicService.AddMusic(item);

            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();

            int targetIndex = playlist.Items.IndexOf(playlistItem);
            if (targetIndex != -1)
            {
                playlist.Items[targetIndex] = playlistItem with { IsCorruptedItem = true };
            }

            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="Playlist"/> 添加到正在播放列表中
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToNowPlaying(Playlist playlist)
    {
        if (playlist.Items.Count <= 0)
        {
            await DisplayContentDialog("NoSongPlayed_Title".GetLocalized(),
                                                    "NoSongPlayed_PlaylistEmpty".GetLocalized(),
                                                    "OK".GetLocalized());
            return false;
        }

        try
        {
            await PlaylistService.AddPlaylistToNowPlayingAsync(playlist);

            return true;
        }
        catch (AggregateException ex)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayAggregateExceptionError(ex);
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="Playlist"/> 序列添加到正在播放列表中
    /// </summary>
    /// <param name="playlists">一个 <see cref="Playlist"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<Playlist> playlists)
    {
        if (!playlists.Any())
        {
            return false;
        }

        bool noSongInPlaylists = true;
        foreach (Playlist playlist in playlists)
        {
            if (playlist.SongCount > 0)
            {
                noSongInPlaylists = false;
                break;
            }
        }

        if (noSongInPlaylists)
        {
            await DisplayContentDialog("NoSongPlayed_Title".GetLocalized(),
                                                    "NoSongPlayed_PlaylistEmpty".GetLocalized(),
                                                    "OK".GetLocalized());
            return false;
        }

        try
        {
            await PlaylistService.AddPlaylistsToNowPlayingAsync(playlists);

            return true;
        }
        catch (AggregateException ex)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayAggregateExceptionError(ex);
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="SongInfoAndAlbumDetailPack"/> 序列添加到正在播放列表中
    /// </summary>
    /// <param name="packs">一个 <see cref="SongInfoAndAlbumDetailPack"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<SongInfoAndAlbumDetailPack> packs)
    {
        if (!packs.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(packs.ToArray(), box);
            await MusicService.AddMusic(items);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="PlaylistItem"/> 序列添加到正在播放列表中
    /// </summary>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToNowPlaying(IEnumerable<PlaylistItem> playlistItems)
    {
        PlaylistItem[] playlistItemsArray = playlistItems.ToArray();

        if (playlistItemsArray.Length <= 0)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<MediaPlaybackItem> items = GetMediaPlaybackItems(playlistItemsArray, box);
            await MusicService.AddMusic(items);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();

            if (playlistItemsArray.Length == 1)
            {
                await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
            }
            else
            {
                await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SomeSongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
            }
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="AlbumInfo"/> 中的歌曲添加到播放列表中
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <param name="albumInfo"><see cref="AlbumInfo"/> 实例</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, AlbumInfo albumInfo)
    {
        try
        {
            ExceptionBox box = new();

            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(albumDetail, box);

            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            ExceptionBox box = new();

            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(albumInfos, box);

            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="AlbumDetail"/> 中的歌曲添加到播放列表中
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(albumDetail, box);
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将一个 <see cref="SongInfo"/> 添加到播放列表中
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 实例</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, albumDetail);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="SongInfo"/> 序列添加到播放列表中
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <param name="songInfos"><see cref="SongInfo"/> 序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的布尔值</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (!songInfos.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(songInfos.ToArray(), albumDetail, box);
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);

            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="PlaylistItem"/> 序列添加到播放列表中
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<PlaylistItem> playlistItems)
    {
        if (!playlistItems.Any())
        {
            return false;
        }

        try
        {
            await PlaylistService.AddItemsForPlaylistAsync(playlist, playlistItems.ToArray());
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 将 <see cref="SongInfoAndAlbumDetailPack"/> 序列添加到播放列表中
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <param name="packs"><see cref="SongInfoAndAlbumDetailPack"/> 序列</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> AddToPlaylist(Playlist playlist, IEnumerable<SongInfoAndAlbumDetailPack> packs)
    {
        if (!packs.Any())
        {
            return false;
        }

        try
        {
            ExceptionBox box = new();
            IAsyncEnumerable<(SongDetail, AlbumDetail)> items = GetSongDetailAlbumDetailPairs(packs, box);
            await PlaylistService.AddItemsForPlaylistAsync(playlist, items);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="AlbumInfo"/> 中歌曲的操作
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartDownload(AlbumInfo albumInfo)
    {
        try
        {
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);

            foreach (SongInfo songInfo in albumDetail.Songs)
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="AlbumInfo"/> 序列中歌曲的操作
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumInfo"/> 序列</param>
    /// <returns>指示下载操作是否成功开始的值</returns>
    public static async Task<bool> StartDownload(IEnumerable<AlbumInfo> albumInfos)
    {
        try
        {
            foreach (AlbumInfo albumInfo in albumInfos)
            {
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                    _ = DownloadService.DownloadSong(albumDetail, songDetail);
                }
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="AlbumDetail"/> 中歌曲的操作
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 的实例</param>
    /// <returns>指示下载操作是否成功开始的值</returns>
    public static async Task<bool> StartDownload(AlbumDetail albumDetail)
    {
        if (albumDetail.Songs is null)
        {
            return false;
        }

        try
        {
            foreach (SongInfo item in albumDetail.Songs)
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="SongInfo"/> 所表示歌曲的操作
    /// </summary>
    /// <param name="songInfo">一个 <see cref="SongInfo"/> 的实例</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartDownload(SongInfo songInfo, AlbumDetail albumDetail)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            _ = DownloadService.DownloadSong(albumDetail, songDetail);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="SongInfo"/> 序列的操作
    /// </summary>
    /// <param name="songInfos"><see cref="SongInfo"/> 序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/></param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartDownload(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail)
    {
        if (!songInfos.Any())
        {
            return false;
        }

        try
        {
            foreach (SongInfo songInfo in songInfos.ToArray())
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="PlaylistItem"/> 所表示播放列表项的操作
    /// </summary>
    /// <param name="playlistItem">一个 <see cref="PlaylistItem"/> 实例</param>
    /// <returns>指示操作是否成功的值</returns>
    public static async Task<bool> StartDownload(PlaylistItem playlistItem)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(playlistItem.SongCid);
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(playlistItem.AlbumCid);
            _ = DownloadService.DownloadSong(albumDetail, songDetail);

            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
        catch (ArgumentOutOfRangeException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }

        return false;
    }

    /// <summary>
    /// 启动下载 <see cref="Playlist"/> 中歌曲的操作
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <returns>指示下载是否完全成功的值</returns>
    public static async Task<bool> StartDownload(Playlist playlist)
    {
        if (playlist.Items.Count <= 0)
        {
            return false;
        }

        bool isAllSuccess = await StartDownload(playlist.Items);
        return isAllSuccess;
    }

    /// <summary>
    /// 启动下载 <see cref="PlaylistItem"/> 序列的操作
    /// </summary>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列</param>
    /// <returns>指示下载是否完全成功的值</returns>
    public static async Task<bool> StartDownload(IEnumerable<PlaylistItem> playlistItems)
    {
        if (!playlistItems.Any())
        {
            return false;
        }

        bool isAllSuccess = true;

        foreach (PlaylistItem playlistItem in playlistItems.ToArray())
        {
            try
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(playlistItem.SongCid);
                AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(playlistItem.AlbumCid);
                _ = DownloadService.DownloadSong(albumDetail, songDetail);
            }
            catch (HttpRequestException)
            {
                await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
                isAllSuccess = false;
            }
            catch (ArgumentOutOfRangeException)
            {
                await DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidCorruptMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
                isAllSuccess = false;
            }
        }

        return isAllSuccess;
    }

    /// <summary>
    /// 根据 <see cref="AlbumInfo"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列
    /// </summary>
    /// <param name="albumInfos"><see cref="AlbumInfo"/> 序列</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列</returns>
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
    /// 根据 <see cref="AlbumDetail"/> 实例获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列</returns>
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
    /// <param name="songInfos"><see cref="SongInfo"/> 序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/> 实例</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列</returns>
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
    /// 根据 <see cref="PlaylistItem"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列
    /// </summary>
    /// <param name="playlistItems"><see cref="PlaylistItem"/> 序列</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列</returns>
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
    /// 根据 <see cref="Playlist"/> 获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列
    /// </summary>
    /// <param name="playlist"><see cref="Playlist"/> 实例</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列</returns>
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
    /// 根据 <see cref="Playlist"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列
    /// </summary>
    /// <param name="playlists"><see cref="Playlist"/> 序列</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列</returns>
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
    /// 根据 <see cref="SongInfoAndAlbumDetailPack"/> 序列获得可异步枚举的 <see cref="MediaPlaybackItem"/> 序列
    /// </summary>
    /// <param name="packs"><see cref="SongInfoAndAlbumDetailPack"/> 序列</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="MediaPlaybackItem"/> 序列</returns>
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
    /// 根据 <see cref="AlbumInfo"/> 序列获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列
    /// </summary>
    /// <param name="albumInfos">一个 <see cref="AlbumInfo"/> 序列</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(IEnumerable<AlbumInfo> albumInfos, ExceptionBox box)
    {
        foreach (AlbumInfo albumInfo in albumInfos)
        {
            AlbumDetail albumDetail;
            try
            {
                albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            foreach (SongInfo songInfo in albumDetail.Songs)
            {
                SongDetail songDetail;

                try
                {
                    songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                }
                catch (Exception ex)
                {
                    box.InboxException = ex;
                    yield break;
                }

                yield return (songDetail, albumDetail);
            }
        }
    }

    /// <summary>
    /// 根据 <see cref="AlbumDetail"/> 实例获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组 序列</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(AlbumDetail albumDetail, ExceptionBox box)
    {
        foreach (SongInfo item in albumDetail.Songs)
        {
            SongDetail songDetail;

            try
            {
                songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return (songDetail, albumDetail);
        }
    }

    /// <summary>
    /// 根据 <see cref="SongInfo"/> 序列获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列
    /// </summary>
    /// <param name="songInfos"><see cref="SongInfo"/> 序列</param>
    /// <param name="albumDetail">表示歌曲所属专辑信息的 <see cref="AlbumDetail"/> 实例</param>
    /// <param name="box">存储异常的 <see cref="ExceptionBox"/></param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列</returns>
    /// <remarks>
    /// 当出现异常时，此方法会将异常信息记录到 <see cref="ExceptionBox"/> 中，并中止序列枚举。
    /// </remarks>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(IEnumerable<SongInfo> songInfos, AlbumDetail albumDetail, ExceptionBox box)
    {
        foreach (SongInfo item in songInfos)
        {
            SongDetail songDetail;

            try
            {
                songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return (songDetail, albumDetail);
        }
    }

    /// <summary>
    /// 根据 <see cref="SongInfoAndAlbumDetailPack"/> 序列获得可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列
    /// </summary>
    /// <param name="packs"><see cref="SongInfoAndAlbumDetailPack"/> 序列</param>
    /// <returns>一个可异步枚举的 <see cref="SongDetail"/> 与 <see cref="AlbumDetail"/> 二元组序列</returns>
    public static async IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> GetSongDetailAlbumDetailPairs(IEnumerable<SongInfoAndAlbumDetailPack> packs, ExceptionBox box)
    {
        foreach ((SongInfo songInfo, AlbumDetail albumDetail) in packs)
        {
            SongDetail songDetail;

            try
            {
                songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
            }
            catch (Exception ex)
            {
                box.InboxException = ex;
                yield break;
            }

            yield return (songDetail, albumDetail);
        }
    }

    /// <summary>
    /// 新建播放列表
    /// </summary>
    public static async Task CreatePlaylist()
    {
        PlaylistInfoDialog dialog = new()
        {
            Title = "PlaylistCreationTitle".GetLocalized(),
            PrimaryButtonText = "PlaylistCreationPrimaryButtonText".GetLocalized()
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.CreateNewPlaylistAsync(dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }

    /// <summary>
    /// 修改指定的播放列表
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    public static async Task ModifyPlaylist(Playlist playlist)
    {
        PlaylistInfoDialog dialog = new()
        {
            Title = "PlaylistModifyTitle".GetLocalized(),
            PrimaryButtonText = "PlaylistModifyPrimaryButtonText".GetLocalized(),
            PlaylistTitle = playlist.Title,
            PlaylistDescription = playlist.Description,
            TargetPlaylist = playlist,
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.ModifyPlaylistAsync(playlist, dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }

    /// <summary>
    /// 移除指定的播放列表
    /// </summary>
    /// <param name="playlist">目标播放列表</param>
    /// <param name="suppressWarning">指示是否要取消删除警告的值</param>
    public static async Task RemovePlaylist(Playlist playlist, bool suppressWarning = false)
    {
        ContentDialogResult result = !suppressWarning
            ? await DisplayContentDialog("EnsureDelete".GetLocalized(), "", "OK".GetLocalized(),
                                                "Cancel".GetLocalized())
            : ContentDialogResult.None;

        if (suppressWarning || result == ContentDialogResult.Primary)
        {
            await PlaylistService.RemovePlaylistAsync(playlist);
        }
    }

    /// <summary>
    /// 显示针对 <see cref="AggregateException"/> 的错误信息
    /// </summary>
    /// <param name="aggregate">指定的 <see cref="AggregateException"/></param>
    public static async Task DisplayAggregateExceptionError(AggregateException aggregate)
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
}