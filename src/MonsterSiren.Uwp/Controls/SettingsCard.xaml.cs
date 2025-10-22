//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

using Windows.UI.Xaml.Markup;

namespace MonsterSiren.Uwp.Controls;

[ContentProperty(Name = nameof(SettingsContent))]
public sealed partial class SettingsCard : UserControl
{
    public object SettingsContent
    {
        get => GetValue(SettingsContentProperty);
        set => SetValue(SettingsContentProperty, value);
    }

    public static readonly DependencyProperty SettingsContentProperty =
        DependencyProperty.Register("SettingsContent", typeof(object), typeof(SettingsCard), new PropertyMetadata(null));

    public IconElement Icon
    {
        get => (IconElement)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon", typeof(IconElement), typeof(SettingsCard), new PropertyMetadata(null));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(SettingsCard), new PropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register("Description", typeof(string), typeof(SettingsCard), new PropertyMetadata(string.Empty));

    public Style HeaderStyle
    {
        get => (Style)GetValue(HeaderStyleProperty);
        set => SetValue(HeaderStyleProperty, value);
    }

    public static readonly DependencyProperty HeaderStyleProperty =
        DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(SettingsCard), new PropertyMetadata(null));

    public Style DescriptionStyle
    {
        get => (Style)GetValue(DescriptionStyleProperty);
        set => SetValue(DescriptionStyleProperty, value);
    }

    public static readonly DependencyProperty DescriptionStyleProperty =
        DependencyProperty.Register("DescriptionStyle", typeof(Style), typeof(SettingsCard), new PropertyMetadata(null));

    public SettingsCard()
    {
        this.InitializeComponent();
    }
}
