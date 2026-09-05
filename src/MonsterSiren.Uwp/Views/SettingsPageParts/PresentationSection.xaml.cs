namespace MonsterSiren.Uwp.Views.SettingsPageParts;

public sealed partial class PresentationSection : UserControl
{
    public SettingsViewModel ViewModel
    {
        get => (SettingsViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SettingsViewModel), typeof(PresentationSection), new PropertyMetadata(null));

    public PresentationSection()
    {
        this.InitializeComponent();
    }
}
