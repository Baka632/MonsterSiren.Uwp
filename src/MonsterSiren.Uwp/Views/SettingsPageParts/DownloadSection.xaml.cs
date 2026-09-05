namespace MonsterSiren.Uwp.Views.SettingsPageParts;

public sealed partial class DownloadSection : UserControl
{
    public SettingsViewModel ViewModel
    {
        get => (SettingsViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SettingsViewModel), typeof(DownloadSection), new PropertyMetadata(null));

    public DownloadSection()
    {
        this.InitializeComponent();
    }
}
