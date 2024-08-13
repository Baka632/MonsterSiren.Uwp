﻿using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
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

    public const string PlaylistSavePathSettingsKey = "Playlist_SavePath_SettingsKey";

    public const string AppBackgroundModeSettingsKey = "App_BackgroundMode_SettingsKey";
    public const string AppColorThemeSettingsKey = "App_ColorTheme_SettingsKey";

    public const string AppGlanceModeBurnProtectionSettingsKey = "App_GlanceMode_BurnProtection_SettingsKey";
    public const string AppGlanceModeUseLowerBrightnessSettingsKey = "App_GlanceMode_UseLowerBrightness_SettingsKey";
    #endregion

    #region Data Package Type
    public const string MusicAlbumInfoFormatId = "Music_AlbumInfo_DataPackage_FormatId";
    public const string MusicSongInfoAndAlbumDetailPackFormatId = "Music_SongInfoAndAlbumDetailPack_DataPackage_FormatId";
    public const string MusicPlaylistFormatId = "Music_Playlist_DataPackage_FormatId";
    public const string MusicPlaylistItemFormatId = "Music_PlaylistItem_DataPackage_FormatId";
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
}
