namespace MonsterSiren.Uwp;

/// <summary>
/// 提供应用中常用的值的类
/// </summary>
internal static class CommonValues
{
    #region Message Token
    public const string NotifyWillUpdateMediaMessageToken = "Notify_WillUpdateMedia_MessageToken";
    public const string NotifyUpdateMediaFailMessageToken = "Notify_UpdateMediaFail_MessageToken";
    #endregion

    #region Cache Key
    public const string AlbumInfoCacheKey = "AlbumInfo_CacheKey";
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
    #endregion

    #region Data Package Type
    public const string MusicAlbumInfoFormatId = "Music_AlbumInfo_DataPackage_FormatId";
    public const string MusicSongInfoAndAlbumPackDetailFormatId = "Music_SongInfoAndAlbumDetailPack_DataPackage_FormatId";
    #endregion
}
