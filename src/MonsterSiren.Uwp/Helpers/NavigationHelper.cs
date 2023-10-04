using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为导航操作提供帮助方法的类
/// </summary>
public static class NavigationHelper
{
    private static bool isInitialized;
    private static Frame currentFrame;

    static NavigationHelper()
    {
        SystemNavigationManager navigationManager = SystemNavigationManager.GetForCurrentView();
        navigationManager.BackRequested += BackRequested;
    }

    /// <summary>
    /// 使用指定的 <see cref="Frame"/> 初始化此类
    /// </summary>
    /// <param name="frame">一个 <see cref="Frame"/>，此类将会根据这个 <see cref="Frame"/> 来进行导航操作</param>
    public static void Initialize(Frame frame)
    {
        currentFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        isInitialized = true;
    }

    public static void GoBack(BackRequestedEventArgs e)
    {
        EnsureInitialized();

        if (currentFrame.CanGoBack)
        {
            e.Handled = true;
            currentFrame.GoBack();
        }
    }

    public static void GoForward(BackRequestedEventArgs e)
    {
        EnsureInitialized();

        if (currentFrame.CanGoForward)
        {
            e.Handled = true;
            currentFrame.GoForward();
        }
    }

    public static void Navigate(Type sourcePageType, object parameter = null, NavigationTransitionInfo transitionInfo = null)
    {
        EnsureInitialized();

        currentFrame.Navigate(sourcePageType, parameter, transitionInfo);
    }

    private static void BackRequested(object sender, BackRequestedEventArgs e)
    {
        GoBack(e);
    }

    private static void EnsureInitialized()
    {
        if (isInitialized is not true)
        {
            throw new InvalidOperationException($"必须先调用 {nameof(Initialize)} 方法，之后才能进行此操作");
        }
    }
}
