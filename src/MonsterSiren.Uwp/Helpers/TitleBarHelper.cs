using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为标题栏相关操作提供帮助的类
/// </summary>
public static class TitleBarHelper
{
    /// <summary>
    /// 当系统默认的后退按钮可视性发生改变时引发
    /// </summary>
    public static event Action<BackButtonVisibilityChangedEventArgs> BackButtonVisibilityChangedEvent;
    /// <summary>
    /// 当标题栏的可视性发生改变时引发
    /// </summary>
    public static event Action<CoreApplicationViewTitleBar> TitleBarVisibilityChangedEvent;
    /// <summary>
    /// 系统导航管理器
    /// </summary>
    public static SystemNavigationManager NavigationManager { get; } = SystemNavigationManager.GetForCurrentView();

    /// <summary>
    /// 使用指定的 <see cref="Frame"/> 初始化此类
    /// </summary>
    /// <param name="frame">一个 <see cref="Frame"/> </param>
    public static void Initialize(Frame frame)
    {
        if (DeviceFamilyHelper.IsWindowsMobile() != true && frame is not null)
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.IsVisibleChanged += OnTitleBarVisibilityChanged;

            ApplicationViewTitleBar presentationTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            presentationTitleBar.ButtonBackgroundColor = Colors.Transparent;
            Color ForegroundColor = Application.Current.RequestedTheme switch
            {
                ApplicationTheme.Light => Colors.Black,
                ApplicationTheme.Dark => Colors.White,
                _ => Colors.White,
            };
            presentationTitleBar.ButtonForegroundColor = ForegroundColor;

            frame.Navigated += OnCurrentFrameNavigated;
        }
    }

    private static void OnTitleBarVisibilityChanged(CoreApplicationViewTitleBar sender, object args)
    {
        TitleBarVisibilityChangedEvent?.Invoke(sender);
    }

    private static void OnCurrentFrameNavigated(object sender, NavigationEventArgs e)
    {
        NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibilityToBoolean((sender as Frame).CanGoBack);
        BackButtonVisibilityChangedEvent?.Invoke(new BackButtonVisibilityChangedEventArgs(NavigationManager.AppViewBackButtonVisibility));
    }

    private static AppViewBackButtonVisibility AppViewBackButtonVisibilityToBoolean(bool canGoBack)
    {
        return canGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
    }
}

/// <summary>
/// 为“后退按钮的可视性发生改变”事件提供数据
/// </summary>
public sealed class BackButtonVisibilityChangedEventArgs : EventArgs
{
    public AppViewBackButtonVisibility BackButtonVisibility { get; }

    public BackButtonVisibilityChangedEventArgs(AppViewBackButtonVisibility visibility)
    {
        BackButtonVisibility = visibility;
    }
}
