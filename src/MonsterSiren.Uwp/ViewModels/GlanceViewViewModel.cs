using Windows.Media.Playback;
using Windows.Networking.Connectivity;
using Windows.System.Power;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class GlanceViewViewModel : ObservableObject
{
    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;
    public bool ShowPowerState { get => PowerStateGlyph != string.Empty; }

    [NotifyPropertyChangedFor(nameof(ShowPowerState))]
    [ObservableProperty]
    private string powerStateGlyph = "\uEBAA";
    [ObservableProperty]
    private double _contentOffset;
    [ObservableProperty]
    private bool showPlayState = MusicService.PlayerPlayBackState is MediaPlaybackState.Paused or MediaPlaybackState.Buffering or MediaPlaybackState.Opening;
    [ObservableProperty]
    private bool showMuteState = MusicService.IsPlayerMuted;
    [ObservableProperty]
    private bool showMeteredInternet = false;

    public GlanceViewViewModel()
    {
        ChangePowerStateGlyph();
        OnNetworkStatusChanged();
        NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
        MusicService.PlayerMuteStateChanged += OnMusicServicePlayerMuteStateChanged;
        MusicService.PlayerPlaybackStateChanged += OnMusicServicePlayerPlaybackStateChanged;
        PowerManager.BatteryStatusChanged += OnPowerStatusChanged;
        PowerManager.EnergySaverStatusChanged += OnPowerStatusChanged;
        PowerManager.RemainingChargePercentChanged += OnPowerStatusChanged;
    }

    private void OnMusicServicePlayerMuteStateChanged(bool state)
    {
        ShowMuteState = state;
    }

    private void OnMusicServicePlayerPlaybackStateChanged(MediaPlaybackState state)
    {
        ShowPlayState = state is MediaPlaybackState.Paused or MediaPlaybackState.Buffering or MediaPlaybackState.Opening;
    }

    ~GlanceViewViewModel()
    {
        NetworkInformation.NetworkStatusChanged -= OnNetworkStatusChanged;
        MusicService.PlayerMuteStateChanged -= OnMusicServicePlayerMuteStateChanged;
        MusicService.PlayerPlaybackStateChanged -= OnMusicServicePlayerPlaybackStateChanged;
        PowerManager.BatteryStatusChanged -= OnPowerStatusChanged;
        PowerManager.EnergySaverStatusChanged -= OnPowerStatusChanged;
        PowerManager.RemainingChargePercentChanged -= OnPowerStatusChanged;
    }

    private void OnPowerStatusChanged(object sender, object e)
    {
        ChangePowerStateGlyph();
    }

    private async void ChangePowerStateGlyph()
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            if (PowerManager.BatteryStatus == BatteryStatus.NotPresent)
            {
                PowerStateGlyph = string.Empty;
                return;
            }

            bool isEnableEnergySaver = PowerManager.EnergySaverStatus == EnergySaverStatus.On;

            if (isEnableEnergySaver)
            {
                PowerStateGlyph = PowerManager.RemainingChargePercent switch
                {
                    >= 80 and < 90 => "\uEBBF",
                    >= 70 and < 80 => "\uEBBE",
                    >= 60 and < 70 => "\uEBBD",
                    >= 50 and < 60 => "\uEBBC",
                    >= 40 and < 50 => "\uEBBB",
                    >= 30 and < 40 => "\uEBBA",
                    >= 20 and < 30 => "\uEBB9",
                    >= 10 and < 20 => "\uEBB8",
                    >= 5 and < 10 => "\uEBB7",
                    >= 0 and < 5 => "\uEBB6",
                    _ => "\uEBC0"
                };
            }
            else
            {
                bool isCharging = PowerManager.BatteryStatus != BatteryStatus.Discharging;

                if (isCharging)
                {
                    PowerStateGlyph = PowerManager.RemainingChargePercent switch
                    {
                        >= 80 and < 90 => "\uEBB4",
                        >= 70 and < 80 => "\uEBB3",
                        >= 60 and < 70 => "\uEBB2",
                        >= 50 and < 60 => "\uEBB1",
                        >= 40 and < 50 => "\uEBB0",
                        >= 30 and < 40 => "\uEBAF",
                        >= 20 and < 30 => "\uEBAE",
                        >= 10 and < 20 => "\uEBAD",
                        >= 5 and < 10 => "\uEBAC",
                        >= 0 and < 5 => "\uEBAB",
                        _ => "\uEBB5"
                    };
                }
                else
                {
                    PowerStateGlyph = PowerManager.RemainingChargePercent switch
                    {
                        >= 80 and < 90 => "\uEBA9",
                        >= 70 and < 80 => "\uEBA8",
                        >= 60 and < 70 => "\uEBA7",
                        >= 50 and < 60 => "\uEBA6",
                        >= 40 and < 50 => "\uEBA5",
                        >= 30 and < 40 => "\uEBA4",
                        >= 20 and < 30 => "\uEBA3",
                        >= 10 and < 20 => "\uEBA2",
                        >= 5 and < 10 => "\uEBA1",
                        >= 0 and < 5 => "\uEBA0",
                        _ => "\uEBAA"
                    };
                }
            }
        });
    }

    private async void OnNetworkStatusChanged(object sender = null)
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            ConnectionCost costInfo = NetworkInformation.GetInternetConnectionProfile()?.GetConnectionCost();
            ShowMeteredInternet = costInfo?.NetworkCostType is NetworkCostType.Fixed or NetworkCostType.Variable;
        });
    }
}