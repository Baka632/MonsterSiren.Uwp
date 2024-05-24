using Windows.System.Power;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class GlanceViewViewModel : ObservableObject
{
    [ObservableProperty]
    private string powerStateGlyph = "\uEBAA";

    public MusicInfoService MusicInfo { get; } = MusicInfoService.Default;

    [ObservableProperty]
    private double _contentOffset;

    public GlanceViewViewModel()
    {
        ChangePowerStateGlyph();
        PowerManager.BatteryStatusChanged += OnPowerStatusChanged;
        PowerManager.EnergySaverStatusChanged += OnPowerStatusChanged;
        PowerManager.RemainingChargePercentChanged += OnPowerStatusChanged;
    }

    ~GlanceViewViewModel()
    {
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
}