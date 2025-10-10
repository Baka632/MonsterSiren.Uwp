using System.Net.Http;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation.Metadata;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp;

/// <summary>
/// 提供特定于应用程序的行为，以补充默认的应用程序类。
/// </summary>
sealed partial class App : Application
{
    private bool isInitialized = false;

    /// <summary>
    /// 获取应用程序名。
    /// </summary>
    public static string AppDisplayName
    {
        get
        {
#if !RELEASE
            return "AppDisplayName_Debug".GetLocalized();
#else
            return "AppDisplayName".GetLocalized();
#endif
        }
    }

    /// <summary>
    /// 获取应用程序版本。
    /// </summary>
    public static string AppVersion { get; } = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
    /// <summary>
    /// 获取带“版本”文字的应用程序版本字符串。
    /// </summary>
    public static string AppVersionWithText => string.Format("AppVersion_WithPlaceholder".GetLocalized(), AppVersion);
    public static bool IsGreaterThan18362 => EnvironmentHelper.IsSystemBuildVersionEqualOrGreaterThan(18362);

    /// <summary>
    /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
    /// 已执行，逻辑上等同于 main() 或 WinMain()。
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        UnhandledException += App_UnhandledException;
        this.Suspending += OnSuspending;

        if (SettingsHelper.TryGet(CommonValues.AppColorThemeSettingsKey, out string colorThemeString) && Enum.TryParse(colorThemeString, out AppColorTheme colorTheme))
        {
            switch (colorTheme)
            {
                case AppColorTheme.Light:
                    Current.RequestedTheme = ApplicationTheme.Light;
                    break;
                case AppColorTheme.Dark:
                    Current.RequestedTheme = ApplicationTheme.Dark;
                    break;
                case AppColorTheme.Default:
                default:
                    break;
            }
        }
    }

    private async void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        Exception exception = e.Exception;

        if (exception is null)
        {
            return;
        }

        ToastContent toastContent = new()
        {
            Visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = exception.GetType().Name
                        },
                        new AdaptiveText()
                        {
                            Text = exception.Message
                        },
                        new AdaptiveText()
                        {
                            Text = exception.StackTrace
                        }
                    }
                }
            }
        };

        ToastNotification toastNotif = new(toastContent.GetXml());
        ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

        StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
        StorageFolder logFolder = await temporaryFolder.CreateFolderAsync("Log", CreationCollisionOption.OpenIfExists);
        StorageFile logFile = await logFolder.CreateFileAsync($"Log-{DateTimeOffset.Now:yyyy-MM-dd_HH.mm.ss.fff}.log", CreationCollisionOption.GenerateUniqueName);

        await FileIO.WriteTextAsync(logFile, $"""
            [Exception Detail]
            {exception.GetType()?.Name}: {exception.Message}
            ======
            Source: {exception.Source}
            HResult: {exception.HResult}
            TargetSite Info:
                Name: {exception.TargetSite?.Name}
                Module Name: {exception.TargetSite?.Module?.Name}
                DeclaringType: {exception.TargetSite?.DeclaringType?.Name}
            StackTrace:
            {exception.StackTrace}
            """);
    }

    /// <summary>
    /// 在应用程序由最终用户正常启动时进行调用。
    /// 将在启动应用程序以打开特定文件等情况下使用。
    /// </summary>
    /// <param name="e">有关启动请求和过程的详细信息。</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs e)
    {
        // 不要在窗口已包含内容时重复应用程序初始化，只需确保窗口处于活动状态
        if (Window.Current.Content is not Frame rootFrame)
        {
            // 创建要充当导航上下文的框架，并导航到第一页
            rootFrame = new Frame();

            rootFrame.NavigationFailed += OnNavigationFailed;

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: 从之前挂起的应用程序加载状态
            }

            // 将框架放在当前窗口中
            Window.Current.Content = rootFrame;
            await InitializeApp();
        }

        if (e.PrelaunchActivated == false)
        {
            if (rootFrame.Content == null)
            {
                // 当导航堆栈尚未还原时，导航到第一页，并通过将所需信息作为导航参数传入来配置参数
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // 确保当前窗口处于活动状态
            Window.Current.Activate();
        }
    }

    protected override async void OnActivated(IActivatedEventArgs args)
    {
        await InitializeAppWhenActivate();

        if (args.Kind == ActivationKind.VoiceCommand && args is VoiceCommandActivatedEventArgs voice)
        {
            SpeechRecognitionResult speechRecognitionResult = voice.Result;
            string voiceCommandName = speechRecognitionResult.RulePath[0];

            switch (voiceCommandName)
            {
                case "playLatestAlbum":
                    await GetAlbumsAndPlay();
                    break;
                default:
                    break;
            }
        }
        else if (args.Kind == ActivationKind.Protocol && args is ProtocolActivatedEventArgs protocol)
        {
            try
            {
                Uri uri = protocol.Uri;

                if (uri.Scheme == "windows.personalassistantlaunch")
                {
                    // Cortana
                    WwwFormUrlDecoder decoder = new(uri.Query);
                    string launchArgument = decoder.GetFirstValueByName("LaunchContext");

                    if (launchArgument == CommonValues.BakaEurekaAppLaunchArgument)
                    {
                        ShowBakaEurekaToast();
                    }
                    else
                    {
                        string[] launchArgumentParts = launchArgument.Split('=');
                        if (launchArgumentParts.Length > 1)
                        {
                            string type = launchArgumentParts[0];
                            string value = launchArgumentParts[1];

                            if (type == CommonValues.AlbumAppLaunchArgumentHeader)
                            {
                                await PlayAlbumByCid(value);
                            }
                        }
                    }
                }
                else if (uri.Segments.Length > 1)
                {
                    string argument = uri.Segments[1];

                    if (uri.Host.Equals("playSong", StringComparison.OrdinalIgnoreCase))
                    {
                        await PlaySongByCid(argument);
                    }
                    else if (uri.Host.Equals("playAlbum", StringComparison.OrdinalIgnoreCase))
                    {
                        await PlayAlbumByCid(argument);
                    }
                }
            }
            catch (UriFormatException ex)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
                System.Diagnostics.Debug.WriteLine(ex.Message);
#endif
                // :-)
            }
        }
    }

    private static async Task PlayAlbumByCid(string argument)
    {
        try
        {
            ExceptionBox box = new();
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(argument);
            IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(albumDetail, box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await CommonValues.DisplayInternetErrorDialog();
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidIncorrectInputMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    private static async Task PlaySongByCid(string argument)
    {
        try
        {
            MediaPlaybackItem item = await MsrModelsHelper.GetMediaPlaybackItemAsync(argument);
            MusicService.ReplaceMusic(item);
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await CommonValues.DisplayInternetErrorDialog();
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidIncorrectInputMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    private static async Task GetAlbumsAndPlay()
    {
        try
        {
            ExceptionBox box = new();
            AlbumInfo firstAlbum = (await CommonValues.GetOrFetchAlbums()).CollectionSource.AlbumInfos.FirstOrDefault();
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(firstAlbum.Cid);
            IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(albumDetail, box);

            await MusicService.ReplaceMusic(items);

            box.Unbox();
        }
        catch (HttpRequestException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await CommonValues.DisplayInternetErrorDialog();
        }
        catch (ArgumentOutOfRangeException)
        {
            MusicInfoService.Default.EnsurePlayRelatedPropertyIsCorrect();
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "SongOrAlbumCidIncorrectInputMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    private static void ShowBakaEurekaToast()
    {
        ToastContent toastContent = new()
        {
            Visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                     {
                        new AdaptiveText() { Text = "\u6765\u81EA\u0020\u0042\u0061\u006B\u0061\u0036\u0033\u0032\u0020\u7684\u5BA3\u544A" },
                        new AdaptiveText() { Text = "\u6211\u6C38\u8FDC\u559C\u6B22\u5C24\u91CC\u5361\uFF01" }
                     }
                }
            }
        };

        ToastNotification toastNotif = new(toastContent.GetXml());
        ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
    }

    protected override async void OnFileActivated(FileActivatedEventArgs args)
    {
        await InitializeAppWhenActivate();

        int playlistItemCount = 0;

        IReadOnlyList<IStorageItem> files = args.Files;
        List<Playlist> playlists = new(files.Count);
        foreach (IStorageItem item in files)
        {
            if (item.IsOfType(StorageItemTypes.File))
            {
                try
                {
                    StorageFile file = (StorageFile)item;
                    using Stream stream = await file.OpenStreamForReadAsync();
                    Playlist playlist = await JsonSerializer.DeserializeAsync<Playlist>(stream);

                    playlistItemCount += playlist.SongCount;

                    playlists.Add(playlist);
                }
                catch (JsonException)
                {
                    // Ignore it, just a bad file
                }
            }
        }

        await CommonValues.StartPlay(playlists);
    }

    private async Task InitializeAppWhenActivate()
    {
        if (Window.Current.Content is not Frame frame)
        {
            frame = new Frame();
            Window.Current.Content = frame;
            await InitializeApp();
        }

        if (frame.Content == null)
        {
            frame.Navigate(typeof(MainPage));
        }

        Window.Current.Activate();
    }

    private async Task InitializeApp()
    {
        if (isInitialized)
        {
            return;
        }

        CoreApplication.EnablePrelaunch(true);

        try
        {
            StorageFile vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync("MsrVoiceCommands.xml");
            await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
        }
        catch (Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"安装语音命令失败：\n{ex}");
#endif
        }

        UIThreadHelper.Initialize(Window.Current.Content.Dispatcher);
        await UIThreadHelper.RunOnUIThread(() =>
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                CommonValues.DefaultTransitionInfo = new SlideNavigationTransitionInfo()
                {
                    Effect = SlideNavigationTransitionEffect.FromRight
                };
            }
            else
            {
                CommonValues.DefaultTransitionInfo = new DrillInNavigationTransitionInfo();
            }
        });

        TitleBarHelper.SetTitleBarAppearance();
        LoadResourceDictionaries();

        // 初始化设置
        await DownloadService.Initialize();
        await PlaylistService.Initialize();

        if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
        {
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = Current.RequestedTheme switch
            {
                ApplicationTheme.Light => Colors.Black,
                ApplicationTheme.Dark => Colors.White,
                _ => throw new NotImplementedException(),
            };
        }

        RegisterBackgroundTask();

        isInitialized = true;
    }

    /// <summary>
    /// 导航到特定页失败时调用
    /// </summary>
    ///<param name="sender">导航失败的框架</param>
    ///<param name="e">有关导航失败的详细信息</param>
    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception($"Failed to load Page {e.SourcePageType.FullName}");
    }

    /// <summary>
    /// 在将要挂起应用程序执行时调用。  在不知道应用程序
    /// 无需知道应用程序会被终止还是会恢复，
    /// 并让内存内容保持不变。
    /// </summary>
    /// <param name="sender">挂起的请求的源。</param>
    /// <param name="e">有关挂起请求的详细信息。</param>
    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
        SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
        //TODO: 保存应用程序状态并停止任何后台活动
        deferral.Complete();
    }

    private void LoadResourceDictionaries()
    {
        if (EnvironmentHelper.IsWindowsMobile)
        {
            ResourceDictionary win10AppBackgroundStyle = new()
            {
                Source = new Uri("ms-appx:///ResourcesDictionaries/Win10AppBackground.xaml")
            };
            Resources.MergedDictionaries.Add(win10AppBackgroundStyle);
        }
    }
}
