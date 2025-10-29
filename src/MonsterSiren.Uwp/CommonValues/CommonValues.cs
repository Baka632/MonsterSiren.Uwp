using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp;

/// <summary>
/// 提供应用中常用内容的类。
/// </summary>
internal static partial class CommonValues
{
    #region Message Token
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
    #endregion

    #region Settings Key
    public const string MusicVolumeSettingsKey = "Music_Volume_SettingsKey";
    public const string MusicMuteStateSettingsKey = "Music_MuteState_SettingsKey";
    public const string MusicShuffleStateSettingsKey = "Music_ShuffleState_SettingsKey";
    public const string MusicRepeatStateSettingsKey = "Music_RepeatState_SettingsKey";

    public const string MusicDownloadPathSettingsKey = "Download_Path_SettingsKey";
    public const string MusicDownloadLyricSettingsKey = "Download_DownloadLyric_SettingsKey";
    public const string MusicTranscodeDownloadedItemSettingsKey = "Download_TranscodeDownloadedItem_SettingsKey";
    [Obsolete("Don't use")]
    public const string MusicTranscodeEncoderGuidSettingsKey = "Download_TranscodeEncoderGuid_SettingsKey";
    public const string MusicTranscodeFormatSettingsKey = "Download_TranscodeFormat_SettingsKey";
    public const string MusicTranscodeQualitySettingsKey = "Download_TranscodeQuality_SettingsKey";
    public const string MusicTranscodeKeepWavFileSettingsKey = "Download_TranscodeKeepWavFile_SettingsKey";
    public const string MusicReplaceInvalidCharInDownloadedFileNameSettingsKey = "Download_ReplaceInvalidCharInFileName_SettingsKey";
    public const string MusicAllowUnnecessaryTranscodeSettingsKey = "Download_AllowUnnecessaryTranscode_SettingsKey";
    public const string MusicFileTemplateStringSettingsKey = "Download_MusicFileTemplateStringSettingsKey_SettingsKey";

    public const string PlaylistSavePathSettingsKey = "Playlist_SavePath_SettingsKey";

    public const string AppBackgroundModeSettingsKey = "App_BackgroundMode_SettingsKey";
    public const string AppColorThemeSettingsKey = "App_ColorTheme_SettingsKey";

    public const string AppGlanceModeBurnProtectionSettingsKey = "App_GlanceMode_BurnProtection_SettingsKey";
    public const string AppGlanceModeUseLowerBrightnessSettingsKey = "App_GlanceMode_UseLowerBrightness_SettingsKey";
    public const string AppGlanceModeRemainDisplayOnSettingsKey = "App_GlanceMode_RemainDisplayOn_SettingsKey";

    public const string GlanceModeIsUsedOnceIndicator = "GlanceMode_IsUsedOnce_Indicator";

    public const string AppVersionSettingsKey = "AppVersion_SettingsKey";
    #endregion

    #region Data Package Type
    public const string MusicAlbumInfoFormatId = "Music_AlbumInfo_DataPackage_FormatId";
    public const string MusicSongInfoAndAlbumDetailPacksFormatId = "Music_SongInfoAndAlbumDetailPacks_DataPackage_FormatId";
    public const string MusicPlaylistFormatId = "Music_Playlist_DataPackage_FormatId";
    public const string MusicPlaylistItemsFormatId = "Music_PlaylistItems_DataPackage_FormatId";
    public const string MusicSongFavoriteItemsFormatId = "Music_SongFavoriteItems_DataPackage_FormatId";
    #endregion

    #region Other Common Things
    private static bool isDefaultTransitionInfoSet = false;
    private static NavigationTransitionInfo _defaultTransitionInfo;

    public static NavigationTransitionInfo DefaultTransitionInfo
    {
        get => _defaultTransitionInfo;
        set
        {
            if (isDefaultTransitionInfoSet)
            {
                throw new InvalidOperationException($"{nameof(DefaultTransitionInfo)} 已被设置，不能再修改。");
            }

            _defaultTransitionInfo = value;
            isDefaultTransitionInfoSet = true;
        }
    }

    /// <summary>
    /// <para>
    /// 保存不能作为文件名的字符的字符串数组。
    /// </para>
    /// <para>
    /// 此数组来自于调用 <see cref="Path.GetInvalidFileNameChars"/> 获得的字符数组，而此字段将这个字符数组转换为了字符串数组。
    /// </para>
    /// <para>
    /// 此字段是为了简化在使用诸如 <see cref="string.Replace(string, string)"/> 等类似方法来删除字符时的操作。
    /// </para>
    /// </summary>
    public static string[] InvalidFileNameCharsStringArray = [.. Path.GetInvalidFileNameChars().Select(chr => chr.ToString())];

    public readonly static string SongCountFormat = "SongsCount".GetLocalized();
    public readonly static JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    /// <summary>
    /// 用于提示应用显示“Baka-Eureka”彩蛋的参数。
    /// </summary>
    /// <remarks>XML Document Comment for TN (tianlan).</remarks>
    internal const string BakaEurekaAppLaunchArgument = "BakaEureka";
    internal const string CortanaAppService = "Baka632.SoraRecords.CortanaService";

    public const string AlbumAppLaunchArgumentHeader = "album";
    public static readonly bool IsContract5Present = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5);
    public static readonly bool IsXbox = AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox";

    public static LockerHelper<string> SongDurationLocker { get; } = new();

    public const string DefaultMusicFilenameTemplate = "{Artist} - {SongTitle}";
    public static readonly string[] MusicFilenamePartTemplates = [
            "{AlbumTitle}",
            "{SongTitle}",
            "{Artist}",
            "{Artists}",
        ];
    #endregion
}
