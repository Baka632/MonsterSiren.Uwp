//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

using System.Collections.ObjectModel;
using Windows.UI.Xaml.Markup;

namespace MonsterSiren.Uwp.Controls;

[ContentProperty(Name = nameof(Items))]
public sealed partial class SettingsExpander : UserControl
{
    public IconElement HeaderIcon
    {
        get => (IconElement)GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register("HeaderIcon", typeof(IconElement), typeof(SettingsExpander), new PropertyMetadata(null));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(string), typeof(SettingsExpander), new PropertyMetadata(string.Empty));

    public object CustomHeaderElement
    {
        get => GetValue(CustomHeaderElementProperty);
        set => SetValue(CustomHeaderElementProperty, value);
    }

    public static readonly DependencyProperty CustomHeaderElementProperty =
        DependencyProperty.Register("CustomHeaderElement", typeof(object), typeof(SettingsExpander), new PropertyMetadata(null));

    public ICollection<object> Items { get; }

    public SettingsExpander()
    {
        this.InitializeComponent();
        Items = new ObservableCollection<object>();
    }
}
