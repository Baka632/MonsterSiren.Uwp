using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为导航操作提供帮助方法的类
/// </summary>
public class NavigationHelper
{
    /// <summary>
    /// 当向后导航完成时引发
    /// </summary>
    public event Action GoBackComplete;
    /// <summary>
    /// 当向前导航完成时引发
    /// </summary>
    public event Action GoForwardComplete;
    /// <summary>
    /// 当向特定页导航完成时引发
    /// </summary>
    public event Action NavigationComplete;

    private readonly Frame currentFrame;

    /// <summary>
    /// 使用指定的参数构造 <see cref="NavigationHelper"/> 的新实例
    /// </summary>
    /// <param name="frame">要进行导航操作的 <see cref="Frame"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="frame"/> 为 <see langword="null"/></exception>
    public NavigationHelper(Frame frame)
    {
        currentFrame = frame ?? throw new ArgumentNullException(nameof(frame));
    }

    /// <summary>
    /// 进行向后导航
    /// </summary>
    public void GoBack()
    {
        if (currentFrame.CanGoBack)
        {
            currentFrame.GoBack();
            GoBackComplete?.Invoke();
        }
    }

    /// <summary>
    /// 进行向后导航
    /// </summary>
    /// <param name="e">系统导航请求事件的事件参数，将使用此参数将系统导航请求事件标记为已处理</param>
    public void GoBack(BackRequestedEventArgs e)
    {
        if (currentFrame.CanGoBack)
        {
            e.Handled = true;
            currentFrame.GoBack();
            GoBackComplete?.Invoke();
        }
    }

    /// <summary>
    /// 进行向前导航
    /// </summary>
    public void GoForward()
    {
        if (currentFrame.CanGoForward)
        {
            currentFrame.GoForward();
            GoForwardComplete?.Invoke();
        }
    }

    /// <summary>
    /// 导航到指定的页
    /// </summary>
    /// <param name="sourcePageType">表示目标页的 <see cref="Type"/></param>
    /// <param name="parameter">向目标页传递的参数</param>
    /// <param name="transitionInfo">要在导航时应用的切换效果</param>
    public void Navigate(Type sourcePageType, object parameter = null, NavigationTransitionInfo transitionInfo = null)
    {
        currentFrame.Navigate(sourcePageType, parameter, transitionInfo);
        NavigationComplete?.Invoke();
    }
}
