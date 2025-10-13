using System.Text.Json;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="SettingsPage"/> 提供视图模型。
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<AudioFormat> audioFormats;
    [ObservableProperty]
    private IReadOnlyList<AudioEncodingQuality> audioEncodingQualities;
    [ObservableProperty]
    private IReadOnlyList<AppBackgroundMode> appBackgroundModes;
    [ObservableProperty]
    private IReadOnlyList<AppColorTheme> appColorThemes;
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
    private bool replaceInvalidCharInDownloadedFileName = DownloadService.ReplaceInvalidCharInFileName;
    [ObservableProperty]
    private int selectedAudioFormatIndex = -1;
    [ObservableProperty]
    private int selectedTranscodeQualityIndex = -1;
    [ObservableProperty]
    private bool preserveRawMusicAfterTranscode = DownloadService.KeepRawMusicFileAfterTranscode;
    [ObservableProperty]
    private int selectedAppBackgroundModeIndex = -1;
    [ObservableProperty]
    private int selectedAppColorThemeIndex = -1;
    [ObservableProperty]
    private bool enableGlanceBurnProtection = true;
    [ObservableProperty]
    private bool glanceModeUseLowerBrightness = true;
    [ObservableProperty]
    private bool glanceModeRemainDisplayOn = true;
    [ObservableProperty]
    private IReadOnlyList<AppLanguage> appLanguages;
    [ObservableProperty]
    private int selectedAppLanguageIndex = -1;
    [ObservableProperty]
    private string restartOrCloseButtonText = CommonValues.IsContract5Present
        ? "RestartApp".GetLocalized()
        : "CloseApp".GetLocalized();
    [ObservableProperty]
    private bool showColorThemeChangedInfoBar;
    [ObservableProperty]
    private bool showLanguageChangedInfoBar;

    public void Initialize()
    {
        #region Transcoding
        List<AudioFormat> formats = [AudioFormat.Mp3, AudioFormat.Flac];
        AudioFormats = formats;
        SelectedAudioFormatIndex = formats.IndexOf(DownloadService.TranscodeFormat);

        List<AudioEncodingQuality> qualities = [AudioEncodingQuality.High, AudioEncodingQuality.Medium, AudioEncodingQuality.Low];
        AudioEncodingQualities = qualities;
        SelectedTranscodeQualityIndex = qualities.IndexOf(DownloadService.TranscodeQuality);
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

        SelectedAppBackgroundModeIndex = bgModes.IndexOf(backgroundMode);
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

        SelectedAppColorThemeIndex = appColorThemes.IndexOf(colorTheme);
        ShowColorThemeChangedInfoBar = false; // 为了防止初始化时就显示重启应用通知（毕竟这里不是用户修改的）
        #endregion

        #region Glance
        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeBurnProtectionSettingsKey, out bool enableBurnProtection))
        {
            EnableGlanceBurnProtection = enableBurnProtection;
        }
        else
        {
            EnableGlanceBurnProtection = true;
            SettingsHelper.Set(CommonValues.AppGlanceModeBurnProtectionSettingsKey, true);
        }

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeUseLowerBrightnessSettingsKey, out bool useLowerBrightness))
        {
            GlanceModeUseLowerBrightness = useLowerBrightness;
        }
        else
        {
            GlanceModeUseLowerBrightness = true;
            SettingsHelper.Set(CommonValues.AppGlanceModeUseLowerBrightnessSettingsKey, true);
        }

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeRemainDisplayOnSettingsKey, out bool remainDisplayOn))
        {
            GlanceModeRemainDisplayOn = remainDisplayOn;
        }
        else
        {
            GlanceModeRemainDisplayOn = true;
            SettingsHelper.Set(CommonValues.AppGlanceModeRemainDisplayOnSettingsKey, true);
        }
        #endregion

        #region Languages
        List<AppLanguage> appLanguages = [AppLanguage.SystemLanguage, .. LanguageHelper.SupportLanguages];
        AppLanguages = appLanguages;

        AppLanguage currentLanguage = LanguageHelper.GetCurrentAppLanguage();
        SelectedAppLanguageIndex = appLanguages.IndexOf(currentLanguage);
        ShowLanguageChangedInfoBar = false; // 为了防止初始化时就显示重启应用通知（毕竟这里不是用户修改的）
        #endregion
    }

    partial void OnGlanceModeRemainDisplayOnChanged(bool value)
    {
        SettingsHelper.Set(CommonValues.AppGlanceModeRemainDisplayOnSettingsKey, value);
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

    partial void OnReplaceInvalidCharInDownloadedFileNameChanged(bool value)
    {
        DownloadService.ReplaceInvalidCharInFileName = value;
    }

    partial void OnSelectedAudioFormatIndexChanged(int value)
    {
        DownloadService.TranscodeFormat = AudioFormats[value];
    }

    partial void OnSelectedTranscodeQualityIndexChanged(int value)
    {
        if (value >= 0)
        {
            DownloadService.TranscodeQuality = AudioEncodingQualities[value];
        }
    }

    partial void OnPreserveRawMusicAfterTranscodeChanged(bool value)
    {
        DownloadService.KeepRawMusicFileAfterTranscode = value;
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

    partial void OnSelectedAppLanguageIndexChanged(int value)
    {
        if (value >= 0)
        {
            ShowLanguageChangedInfoBar = true;
            AppLanguage lang = AppLanguages[value];
            LanguageHelper.SetAppLanguage(lang);
        }
    }

    partial void OnSelectedAppColorThemeIndexChanged(int value)
    {
        if (value >= 0)
        {
            ShowColorThemeChangedInfoBar = true;
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

        if (playlistFolder is null || playlistFolder.Path.Equals(PlaylistSavePath, StringComparison.OrdinalIgnoreCase))
        {
            // 用户取消了文件夹选择
            return;
        }

        if (!string.IsNullOrEmpty(PlaylistSavePath))
        {
            try
            {
                StorageFolder formerFolder = await StorageFolder.GetFolderFromPathAsync(PlaylistSavePath);
                IEnumerable<StorageFile> formerFiles = (await formerFolder.GetFilesAsync()).Where(static file => file.FileType.Equals(PlaylistService.PlaylistFileExtension, StringComparison.OrdinalIgnoreCase));

                if (formerFiles.Any())
                {
                    ContentDialogResult result = await CommonValues.DisplayContentDialog("OriginalFolderContainsPlaylist_Title".GetLocalized(),
                        "OriginalFolderContainsPlaylist_Message".GetLocalized(),
                        "OriginalFolderContainsPlaylist_Move".GetLocalized(),
                        secondaryButtonText: "OriginalFolderContainsPlaylist_DoNotMove".GetLocalized(),
                        closeButtonText: "Cancel".GetLocalized(),
                        defaultButton: ContentDialogButton.Primary);

                    if (result == ContentDialogResult.None)
                    {
                        return;
                    }
                    else if (result == ContentDialogResult.Primary)
                    {
                        foreach (StorageFile fileToMove in formerFiles)
                        {
                            if (await playlistFolder.FileExistsAsync(fileToMove.Name))
                            {
                                StorageFile duplicateFile = await playlistFolder.GetFileAsync(fileToMove.Name);
                                Playlist mayDuplicatePlaylist;
                                Playlist currentPlaylist;

                                try
                                {
                                    using Stream duplicateFileStream = await duplicateFile.OpenStreamForReadAsync();
                                    mayDuplicatePlaylist = await JsonSerializer.DeserializeAsync<Playlist>(duplicateFileStream, CommonValues.DefaultJsonSerializerOptions);
                                }
                                catch (JsonException)
                                {
                                    await fileToMove.MoveAndReplaceAsync(duplicateFile);
                                    continue;
                                }

                                try
                                {
                                    using Stream currentFileStream = await fileToMove.OpenStreamForReadAsync();
                                    currentPlaylist = await JsonSerializer.DeserializeAsync<Playlist>(currentFileStream, CommonValues.DefaultJsonSerializerOptions);
                                }
                                catch (JsonException)
                                {
                                    continue;
                                }

                                NameCollisionOption option = currentPlaylist.PlaylistId == mayDuplicatePlaylist.PlaylistId
                                    ? NameCollisionOption.ReplaceExisting
                                    : NameCollisionOption.GenerateUniqueName;

                                await fileToMove.MoveAsync(playlistFolder, PlaylistService.GetPlaylistFileName(currentPlaylist), option);

                                if (!fileToMove.DisplayName.Equals(currentPlaylist.PlaylistSaveName, StringComparison.OrdinalIgnoreCase))
                                {
                                    currentPlaylist.PlaylistSaveName = fileToMove.DisplayName;
                                    using Stream currentFileStream = await fileToMove.OpenStreamForWriteAsync();
                                    currentFileStream.SetLength(0);

                                    await JsonSerializer.SerializeAsync(currentFileStream, currentPlaylist, CommonValues.DefaultJsonSerializerOptions);
                                }
                            }
                            else
                            {
                                await fileToMove.MoveAsync(playlistFolder);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException)
            {
                // :-)
            }
        }

        StorageApplicationPermissions.FutureAccessList.AddOrReplace(CommonValues.PlaylistSavePathSettingsKey, playlistFolder);
        PlaylistService.PlaylistSavePath = PlaylistSavePath = playlistFolder.Path;
        IsPlaylistFolderRedirected = false;
        await PlaylistService.Initialize();
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
        try
        {
            CodecQuery codecQuery = new();
            IReadOnlyList<CodecInfo> encoders = await codecQuery.FindAllAsync(CodecKind.Audio, CodecCategory.Encoder, string.Empty);
            IReadOnlyList<CodecInfo> decoders = await codecQuery.FindAllAsync(CodecKind.Audio, CodecCategory.Decoder, string.Empty);

            List<CodecInfo> codecs = [.. encoders, .. decoders];

            CodecInfoDialog dialog = new(codecs);
            _ = await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            _ = await CommonValues.DisplayContentDialog(
                "CodecInfoDialog_UnableDisplayTitle".GetLocalized(),
                string.Format("CodecInfoDialog_UnableDisplayMessage".GetLocalized(), ex.Message),
                closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private static async Task OpenCopyrightNoticeDialog()
    {
        CopyrightDialog copyrightDialog = new();
        await copyrightDialog.ShowAsync();
    }

    [RelayCommand]
    private static void OpenUpdateInfoPage()
    {
        ContentFrameNavigationHelper.Navigate(typeof(UpdateInfoPage), null, CommonValues.DefaultTransitionInfo);
    }

    [RelayCommand]
    private static async Task RestartOrCloseApp()
    {
        if (CommonValues.IsContract5Present)
        {
            await CoreApplication.RequestRestartAsync(string.Empty);
        }
        else
        {
            Application.Current.Exit();
        }
    }
}