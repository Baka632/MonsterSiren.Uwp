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

    public const string AppBackgroundModeSettingsKey = "App_BackgroundMode_SettingsKey";
    public const string AppColorThemeSettingsKey = "App_ColorTheme_SettingsKey";

    public const string AppGlanceModeBurnProtectionSettingsKey = "App_GlanceMode_BurnProtection_SettingsKey";
    public const string AppGlanceModeUseLowerBrightnessSettingsKey = "App_GlanceMode_UseLowerBrightness_SettingsKey";
    #endregion

    #region Data Package Type
    public const string MusicAlbumInfoFormatId = "Music_AlbumInfo_DataPackage_FormatId";
    public const string MusicSongInfoAndAlbumPackDetailFormatId = "Music_SongInfoAndAlbumDetailPack_DataPackage_FormatId";
    #endregion

    #region Other Common Things
    public static readonly NavigationTransitionInfo DefaultTransitionInfo;
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
}
