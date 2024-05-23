namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class GlanceViewViewModel : ObservableObject
{
    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;

    [ObservableProperty]
    private double _contentOffset;
}