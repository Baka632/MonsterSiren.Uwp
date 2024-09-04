using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Input;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp;

/// <summary>
/// 提供应用中常用的值的类
/// </summary>
internal static class CommonValues
{
    #region Message Token
    public const string NotifyWillUpdateMediaMessageToken = "Notify_WillUpdateMedia_MessageToken";
    public const string NotifyUpdateMediaFailMessageToken = "Notify_UpdateMediaFail_MessageToken";
    public const string NotifyAppBackgroundChangedMessageToken = "Notify_AppBackgroundChanged_MessageToken";
    #endregion

    #region Cache Key
    public const string AlbumInfoCacheKey = "AlbumInfo_CacheKey";
    public const string NewsItemCollectionCacheKey = "NewsItemCollection_CacheKey";
    public const string RecommendedNewsInfosCacheKey = "RecommendedNewsInfos_CacheKey";
    #endregion

    #region Animation Key
    public const string AlbumInfoForwardConnectedAnimationKeyForMusicPage = "MusicPage_AlbumInfo_ForwardConnectedAnimation";
    public const string AlbumInfoBackConnectedAnimationKeyForMusicPage = "MusicPage_AlbumInfo_BackConnectedAnimation";

    public const string PlaylistDetailForwardConnectedAnimationKey = "PlaylistDetail_ForwardConnectedAnimation";
    public const string PlaylistDetailBackConnectedAnimationKey = "PlaylistDetail_BackConnectedAnimation";
    #endregion

    #region Settings Key
    public const string MusicVolumeSettingsKey = "Music_Volume_SettingsKey";
    public const string MusicMuteStateSettingsKey = "Music_MuteState_SettingsKey";
    public const string MusicShuffleStateSettingsKey = "Music_ShuffleState_SettingsKey";
    public const string MusicRepeatStateSettingsKey = "Music_RepeatState_SettingsKey";

    public const string MusicDownloadPathSettingsKey = "Download_Path_SettingsKey";
    public const string MusicDownloadLyricSettingsKey = "Download_DownloadLyric_SettingsKey";
    public const string MusicTranscodeDownloadedItemSettingsKey = "Download_TranscodeDownloadedItem_SettingsKey";
    public const string MusicTranscodeEncoderGuidSettingsKey = "Download_TranscodeEncoderGuid_SettingsKey";
    public const string MusicTranscodeQualitySettingsKey = "Download_TranscodeQuality_SettingsKey";
    public const string MusicTranscodeKeepWavFileSettingsKey = "Download_TranscodeKeepWavFile_SettingsKey";

    public const string PlaylistSavePathSettingsKey = "Playlist_SavePath_SettingsKey";

    public const string AppBackgroundModeSettingsKey = "App_BackgroundMode_SettingsKey";
    public const string AppColorThemeSettingsKey = "App_ColorTheme_SettingsKey";

    public const string AppGlanceModeBurnProtectionSettingsKey = "App_GlanceMode_BurnProtection_SettingsKey";
    public const string AppGlanceModeUseLowerBrightnessSettingsKey = "App_GlanceMode_UseLowerBrightness_SettingsKey";
    #endregion

    #region Data Package Type
    public const string MusicAlbumInfoFormatId = "Music_AlbumInfo_DataPackage_FormatId";
    public const string MusicSongInfoAndAlbumDetailPacksFormatId = "Music_SongInfoAndAlbumDetailPacks_DataPackage_FormatId";
    public const string MusicPlaylistFormatId = "Music_Playlist_DataPackage_FormatId";
    public const string MusicPlaylistItemsFormatId = "Music_PlaylistItems_DataPackage_FormatId";
    #endregion

    #region Other Common Things
    public static NavigationTransitionInfo DefaultTransitionInfo { get; private set; }
    public readonly static string SongCountFormat = "SongsCount".GetLocalized();
    public readonly static JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
    #endregion

    static CommonValues()
    {
        if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
        {
            DefaultTransitionInfo = new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            };
        }
        else
        {
            DefaultTransitionInfo = new DrillInNavigationTransitionInfo();
        }
    }

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
}
