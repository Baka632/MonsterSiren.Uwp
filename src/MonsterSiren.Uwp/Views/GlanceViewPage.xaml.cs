// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class GlanceViewPage : Page
{
    private readonly DispatcherTimer _timer = new();
    private readonly Random _random = new();
    private readonly BrightnessOverride _brightnessOverride;

    public GlanceViewViewModel ViewModel { get; } = new GlanceViewViewModel();

    public GlanceViewPage()
    {
        this.InitializeComponent();

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeBurnProtectionSettingsKey, out bool isEnableBurnProtection) && isEnableBurnProtection)
        {
            _timer.Interval = TimeSpan.FromSeconds(40d);
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        SizeChanged += OnPageSizeChanged;

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeUseLowerBrightnessSettingsKey, out bool useLowerBrightness) && useLowerBrightness)
        {
            _brightnessOverride = BrightnessOverride.GetForCurrentView();
            if (_brightnessOverride.IsSupported)
            {
                _brightnessOverride.SetBrightnessLevel(0.1, DisplayBrightnessOverrideOptions.UseDimmedPolicyWhenBatteryIsLow);
                _brightnessOverride.StartOverride();
            }
        }
    }

    private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.ContentOffset = ContentCanvas.ActualHeight - ContentStackPanel.ActualHeight;
    }

    private void OnTimerTick(object sender, object e)
    {
        AdjustContentPosition();
    }

    private void AdjustContentPosition()
    {
        double height = ContentCanvas.ActualHeight - ContentStackPanel.ActualHeight;

        if (ViewModel.ContentOffset == height)
        {
            ViewModel.ContentOffset -= height / 5d;
        }
        else if (ViewModel.ContentOffset > height - (height * 0.85) && ViewModel.ContentOffset < height)
        {
            double randomValue = _random.NextDouble();

            if (randomValue > 0.65)
            {
                ViewModel.ContentOffset -= randomValue * (height / 5d);
            }
            else
            {
                double delta = randomValue * (height / 5d);

                if (ViewModel.ContentOffset + delta > height)
                {
                    ViewModel.ContentOffset = height;
                }
                else
                {
                    ViewModel.ContentOffset += delta;
                }
            }
        }
        else
        {
            ViewModel.ContentOffset = height;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ApplicationView view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode != true)
        {
            view.TryEnterFullScreenMode();
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        ApplicationView view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
        {
            view.ExitFullScreenMode();
        }

        if (_brightnessOverride is not null && _brightnessOverride.IsSupported)
        {
            _brightnessOverride.StopOverride();
        }
    }

    private void OnContentLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ContentOffset = ContentCanvas.ActualHeight - ContentStackPanel.ActualHeight;
    }

    private double MeasureTextSize(TextBlock textBlock)
    {
        using CanvasTextFormat textFormat = new()
        {
            FontSize = (float)FontSize,
            FontFamily = FontFamily.Source,
            Direction = CanvasTextDirection.LeftToRightThenTopToBottom,
            WordWrapping = CanvasWordWrapping.NoWrap
        };
        CanvasDevice device = CanvasDevice.GetSharedDevice();

        double width = (double.IsNaN(textBlock.ActualWidth) || textBlock.ActualWidth < 0) ? 0 : textBlock.ActualWidth;
        using CanvasTextLayout layout = new(device, textBlock.Text, textFormat, (float)width, 0);
        return layout.LayoutBounds.Width;
    }
}
