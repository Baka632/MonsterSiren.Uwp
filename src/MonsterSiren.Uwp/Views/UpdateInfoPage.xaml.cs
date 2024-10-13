// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Globalization;
using Windows.Storage;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class UpdateInfoPage : Page
{
    internal readonly string UpdateInfoBasePath = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase)
            ? "ms-appx:///Assets/Release-Note/zh-cn/"
            : "ms-appx:///Assets/Release-Note/en-us/";

    public UpdateInfoPage()
    {
        this.InitializeComponent();
    }

    private async void OnMarkdownTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        Uri UpdateInfoBaseUri = new(UpdateInfoBasePath, UriKind.Absolute);
        Uri fileUri = new(UpdateInfoBaseUri, "note.md");

        StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
        string mdText = await FileIO.ReadTextAsync(file);
        UpdateInfoMarkdownTextBlock.Text = mdText;
    }
}
