using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.System;

namespace MonsterSiren.Uwp.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public readonly IReadOnlyList<CodecInfo> AvailableCommonEncoders;
    public readonly IReadOnlyList<AudioEncodingQuality> AudioEncodingQualities;
    public readonly IReadOnlyList<AppBackgroundMode> AppBackgroundModes;
    public readonly IReadOnlyList<AppColorTheme> AppColorThemes;

    [ObservableProperty]
    private string downloadPath = DownloadService.DownloadPath;
    [ObservableProperty]
    private string playlistSavePath = PlaylistService.PlaylistSavePath;
    [ObservableProperty]
    private bool isDownloadFolderRedirected = DownloadService.DownloadPathRedirected;
    [ObservableProperty]
    private bool isPlaylistFolderRedirected = PlaylistService.PlaylistPathRedirected;
    [ObservableProperty]
    private bool downloadLyric = DownloadService.DownloadLyric;
    [ObservableProperty]
    private bool transcodeDownloadedMusic = DownloadService.TranscodeDownloadedItem;
    [ObservableProperty]
    private int selectedCodecInfoIndex = -1;
    [ObservableProperty]
    private int selectedTranscodeQualityIndex = -1;
    [ObservableProperty]
    private bool preserveWavAfterTranscode = DownloadService.KeepWavFileAfterTranscode;
    [ObservableProperty]
    private int selectedAppBackgroundModeIndex;
    [ObservableProperty]
    private int selectedAppColorThemeIndex;
    [ObservableProperty]
    private bool enableGlanceBurnProtection = true;
    [ObservableProperty]
    private bool glanceModeUseLowerBrightness = true;

    public SettingsViewModel()
    {
        #region Transcoding
        if (CodecQueryHelper.TryGetCommonEncoders(out IEnumerable<CodecInfo> infos))
        {
            List<CodecInfo> codecInfos = infos.ToList();

            AvailableCommonEncoders = codecInfos;
            selectedCodecInfoIndex = codecInfos.FindIndex(info => info.Subtypes.SequenceEqual(DownloadService.TranscodeEncoderInfo?.Subtypes ?? Enumerable.Empty<string>()));
        }

        List<AudioEncodingQuality> qualities = [AudioEncodingQuality.High, AudioEncodingQuality.Medium, AudioEncodingQuality.Low];
        AudioEncodingQualities = qualities;
        selectedTranscodeQualityIndex = qualities.IndexOf(DownloadService.TranscodeQuality);
        #endregion

        #region Background
        List<AppBackgroundMode> bgModes = new(3);

        bool isSupportMica = MicaHelper.IsSupported();
        bool isSupportAcrylic = AcrylicHelper.IsSupported();

        if (isSupportMica)
        {
            bgModes.Add(AppBackgroundMode.Mica);
        }

        if (isSupportAcrylic)
        {
            bgModes.Add(AppBackgroundMode.Acrylic);
        }

        // 不管什么情况，系统一定支持纯色背景显示
        bgModes.Add(AppBackgroundMode.PureColor);

        AppBackgroundModes = bgModes;

        if (SettingsHelper.TryGet(CommonValues.AppBackgroundModeSettingsKey, out string bgModeString) && Enum.TryParse(bgModeString, out AppBackgroundMode backgroundMode))
        {
            // ;-)
        }
        else
        {
            if (isSupportMica)
            {
                backgroundMode = AppBackgroundMode.Mica;
            }
            else if (isSupportAcrylic)
            {
                backgroundMode = AppBackgroundMode.Acrylic;
            }
            else
            {
                backgroundMode = AppBackgroundMode.PureColor;
            }
        }

        selectedAppBackgroundModeIndex = bgModes.IndexOf(backgroundMode);
        #endregion

        #region Color Theme
        List<AppColorTheme> appColorThemes = [AppColorTheme.Default, AppColorTheme.Light, AppColorTheme.Dark];
        AppColorThemes = appColorThemes;

        if (SettingsHelper.TryGet(CommonValues.AppColorThemeSettingsKey, out string colorThemeString) && Enum.TryParse(colorThemeString, out AppColorTheme colorTheme))
        {
            // ;-)
        }
        else
        {
            colorTheme = AppColorTheme.Default;
        }

        selectedAppColorThemeIndex = appColorThemes.IndexOf(colorTheme);
        #endregion

        #region Glance
        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeBurnProtectionSettingsKey, out bool enableBurnProtection))
        {
            enableGlanceBurnProtection = enableBurnProtection;
        }
        else
        {
            enableGlanceBurnProtection = true;
            SettingsHelper.Set(CommonValues.AppGlanceModeBurnProtectionSettingsKey, true);
        }

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeUseLowerBrightnessSettingsKey, out bool useLowerBrightness))
        {
            glanceModeUseLowerBrightness = useLowerBrightness;
        }
        else
        {
            glanceModeUseLowerBrightness = true;
            SettingsHelper.Set(CommonValues.AppGlanceModeUseLowerBrightnessSettingsKey, true);
        }
        #endregion
    }

    partial void OnGlanceModeUseLowerBrightnessChanged(bool value)
    {
        SettingsHelper.Set(CommonValues.AppGlanceModeUseLowerBrightnessSettingsKey, value);
    }
    
    partial void OnEnableGlanceBurnProtectionChanged(bool value)
    {
        SettingsHelper.Set(CommonValues.AppGlanceModeBurnProtectionSettingsKey, value);
    }

    partial void OnDownloadLyricChanged(bool value)
    {
        DownloadService.DownloadLyric = value;
    }

    partial void OnTranscodeDownloadedMusicChanged(bool value)
    {
        DownloadService.TranscodeDownloadedItem = value;
    }

    partial void OnSelectedCodecInfoIndexChanged(int value)
    {
        if (value >= 0)
        {
            DownloadService.TranscodeEncoderInfo = AvailableCommonEncoders[value];
        }
    }

    partial void OnSelectedTranscodeQualityIndexChanged(int value)
    {
        if (value >= 0)
        {
            DownloadService.TranscodeQuality = AudioEncodingQualities[value];
        }
    }

    partial void OnPreserveWavAfterTranscodeChanged(bool value)
    {
        DownloadService.KeepWavFileAfterTranscode = value;
    }

    partial void OnSelectedAppBackgroundModeIndexChanged(int value)
    {
        if (value >= 0)
        {
            AppBackgroundMode bgMode = AppBackgroundModes[value];
            string bgModeString = bgMode.ToString();
            SettingsHelper.Set(CommonValues.AppBackgroundModeSettingsKey, bgModeString);

            WeakReferenceMessenger.Default.Send(bgModeString, CommonValues.NotifyAppBackgroundChangedMessageToken);
        }
    }

    partial void OnSelectedAppColorThemeIndexChanged(int value)
    {
        if (value >= 0)
        {
            AppColorTheme theme = AppColorThemes[value];
            string themeString = theme.ToString();
            SettingsHelper.Set(CommonValues.AppColorThemeSettingsKey, themeString);
        }
    }

    [RelayCommand]
    private async Task PickPlaylistFolderAsync()
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        StorageFolder playlistFolder = await folderPicker.PickSingleFolderAsync();

        if (playlistFolder is null)
        {
            // 用户取消了文件夹选择
            return;
        }

        StorageApplicationPermissions.FutureAccessList.AddOrReplace(CommonValues.PlaylistSavePathSettingsKey, playlistFolder);
        PlaylistService.PlaylistSavePath = PlaylistSavePath = playlistFolder.Path;
        IsPlaylistFolderRedirected = false;
    }

    [RelayCommand]
    private async Task PickDownloadFolderAsync()
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        StorageFolder downloadFolder = await folderPicker.PickSingleFolderAsync();

        if (downloadFolder is null)
        {
            // 用户取消了文件夹选择
            return;
        }

        StorageApplicationPermissions.FutureAccessList.AddOrReplace(CommonValues.MusicDownloadPathSettingsKey, downloadFolder);
        DownloadService.DownloadPath = DownloadPath = downloadFolder.Path;
        IsDownloadFolderRedirected = false;
    }

    [RelayCommand]
    private static async Task OpenLogFolder()
    {
        StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
        StorageFolder logFolder = await temporaryFolder.CreateFolderAsync("Log", CreationCollisionOption.OpenIfExists);
        await Launcher.LaunchFolderAsync(logFolder);
    }

    [RelayCommand]
    private static async Task OpenCodecsInfoDialog()
    {
        CodecQuery codecQuery = new();
        IReadOnlyList<CodecInfo> encoders = await codecQuery.FindAllAsync(CodecKind.Audio, CodecCategory.Encoder, string.Empty);
        IReadOnlyList<CodecInfo> decoders = await codecQuery.FindAllAsync(CodecKind.Audio, CodecCategory.Decoder, string.Empty);

        List<CodecInfo> codecs = [.. encoders, .. decoders];

        CodecInfoDialog dialog = new(codecs);
        _ = await dialog.ShowAsync();
    }
}