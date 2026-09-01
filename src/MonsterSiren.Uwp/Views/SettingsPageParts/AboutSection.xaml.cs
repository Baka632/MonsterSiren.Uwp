namespace MonsterSiren.Uwp.Views.SettingsPageParts;

public sealed partial class AboutSection : UserControl
{
    public SettingsViewModel ViewModel
    {
        get => (SettingsViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SettingsViewModel), typeof(AboutSection), new PropertyMetadata(null));

    public AboutSection()
    {
        this.InitializeComponent();
    }
}
