using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为导航操作提供帮助方法的类
/// </summary>
public class NavigationHelper
{
    private readonly Frame currentFrame;

    public NavigationHelper(Frame frame)
    {
        currentFrame = frame ?? throw new ArgumentNullException(nameof(frame));
    }

    public void GoBack()
    {
        if (currentFrame.CanGoBack)
        {
            currentFrame.GoBack();
        }
    }

    public void GoBack(BackRequestedEventArgs e)
    {
        if (currentFrame.CanGoBack)
        {
            e.Handled = true;
            currentFrame.GoBack();
        }
    }

    public void GoForward()
    {
        if (currentFrame.CanGoForward)
        {
            currentFrame.GoForward();
        }
    }
    
    public void GoForward(BackRequestedEventArgs e)
    {
        if (currentFrame.CanGoForward)
        {
            e.Handled = true;
            currentFrame.GoForward();
        }
    }

    public void Navigate(Type sourcePageType, object parameter = null, NavigationTransitionInfo transitionInfo = null)
    {
        currentFrame.Navigate(sourcePageType, parameter, transitionInfo);
    }
}
