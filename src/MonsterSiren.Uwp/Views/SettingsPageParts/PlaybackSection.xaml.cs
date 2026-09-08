namespace MonsterSiren.Uwp.Views.SettingsPageParts;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaybackSection : UserControl
{
    public SettingsViewModel ViewModel
    {
        get => (SettingsViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(SettingsViewModel), typeof(PlaybackSection), new PropertyMetadata(null));

    public PlaybackSection()
    {
        this.InitializeComponent();
    }
}
