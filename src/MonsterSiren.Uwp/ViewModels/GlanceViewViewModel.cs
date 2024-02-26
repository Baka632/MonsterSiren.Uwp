namespace MonsterSiren.Uwp.ViewModels;

public sealed class GlanceViewViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer = new();

    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;
}