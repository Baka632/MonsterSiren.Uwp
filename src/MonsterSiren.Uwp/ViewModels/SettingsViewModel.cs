using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Windows.Media.Core;
using Windows.Media.MediaProperties;

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
    private bool isFolderRedirected = DownloadService.DownloadPathRedirected;
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
        IsFolderRedirected = false;
    }

    private static bool HasCommonEncoders(CodecInfo info)
    {
        return info.Subtypes.Any(IsCommonEncoder);
    }

    private static bool IsCommonEncoder(string encoderGuid)
    {
        return encoderGuid == CodecSubtypes.AudioFormatFlac || encoderGuid == CodecSubtypes.AudioFormatMP3;
    }
}