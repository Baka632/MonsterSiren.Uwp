using Windows.System;
using Windows.UI;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class NewsDetailViewModel : ObservableObject
{
    internal readonly LauncherOptions DefaultLauncherOptionsForExternal = new()
    {
        TreatAsUntrusted = true
    };

    [ObservableProperty]
    private NewsDetail currentNewsDetail;

    public async Task InitializeWebView(WebView targetWebView, Color textColor, Color accentColor)
    {
        try
        {
            // 添加主内容
            await targetWebView.InvokeScriptAsync("eval", new[]
            {
                $"document.getElementById('mainContent').insertAdjacentHTML('afterbegin', `{CurrentNewsDetail.Content}`);"
            });

            // 设置文本颜色
            await targetWebView.InvokeScriptAsync("eval", new[]
            {
                $"document.getElementById('mainContent').style.color = 'rgb({textColor.R}, {textColor.G}, {textColor.B})'",
            });

            // 设置文本大小
            string fontSizeString = EnvironmentHelper.IsWindowsMobile ? "2.3rem" : "14px";
            await targetWebView.InvokeScriptAsync("eval", new[]
            {
                $"document.styleSheets[0].insertRule('p {{ font-size: {fontSizeString}; }}', 0);"
            });

            // 设置被选中文本背景色
            await targetWebView.InvokeScriptAsync("eval", new[]
            {
                $"document.styleSheets[0].insertRule('p::selection {{ background: rgb({accentColor.R}, {accentColor.G}, {accentColor.B}) }}', 0);"
            });

            // 设置图像宽度
            string widthPercentage = EnvironmentHelper.IsWindowsMobile ? "90%" : "70%";
            await targetWebView.InvokeScriptAsync("eval", new[]
            {
                $"document.styleSheets[0].insertRule('img {{ max-width: {widthPercentage}; }}', 0);"
            });

            targetWebView.NavigationStarting += async (webView, args) =>
            {
                args.Cancel = true;
                await Launcher.LaunchUriAsync(args.Uri, DefaultLauncherOptionsForExternal);
            };
        }
        catch (Exception ex)
        {
            // 脚本出错了...
#if DEBUG
            System.Diagnostics.Debug.WriteLine("[ReadPage] Exception occured!");
            System.Diagnostics.Debug.WriteLine(ex.Message);
            System.Diagnostics.Debugger.Break();
#endif
        }
    }
}