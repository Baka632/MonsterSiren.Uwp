// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using Windows.Graphics.Display;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class GlanceViewPage : Page
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random;
    private BrightnessOverride _brightnessOverride;
    private DisplayRequest _displayRequest;

    public GlanceViewViewModel ViewModel { get; } = new GlanceViewViewModel();

    public GlanceViewPage()
    {
        this.InitializeComponent();

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeBurnProtectionSettingsKey, out bool isEnableBurnProtection) && isEnableBurnProtection)
        {
            _random = new();
            _timer = new()
            {
                Interval = TimeSpan.FromSeconds(40d)
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        SizeChanged += OnPageSizeChanged;
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
                double delta = (randomValue + 0.1) * (height / 5d);

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

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Window.Current.Dispatcher.AcceleratorKeyActivated += OnDispatcherAcceleratorKeyActivated;

        ApplicationView view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode != true)
        {
            view.TryEnterFullScreenMode();
        }

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeUseLowerBrightnessSettingsKey, out bool useLowerBrightness) && useLowerBrightness)
        {
            await UIThreadHelper.RunOnUIThread(() =>
            {
                _brightnessOverride = BrightnessOverride.GetForCurrentView();
            });
            TryStartBrightnessOverride();

            Application.Current.EnteredBackground += OnAppEnteredBackground;
            Application.Current.LeavingBackground += OnAppLeavingBackground;
        }

        if (SettingsHelper.TryGet(CommonValues.AppGlanceModeRemainDisplayOnSettingsKey, out bool remainDisplayOn) && remainDisplayOn)
        {
            _displayRequest = new DisplayRequest();
            _displayRequest.RequestActive();
        }

        if (!SettingsHelper.TryGet(CommonValues.GlanceModeIsUsedOnceIndicator, out bool glanceModeIsUsedOnce))
        {
            await CommonValues.DisplayContentDialog("GlanceMode_Welcome_Title".GetLocalized(),
                                                    "GlanceMode_Welcome_Message".GetLocalized(), closeButtonText: "OK".GetLocalized());

            SettingsHelper.Set(CommonValues.GlanceModeIsUsedOnceIndicator, true);
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        Window.Current.Dispatcher.AcceleratorKeyActivated -= OnDispatcherAcceleratorKeyActivated;
        Application.Current.EnteredBackground -= OnAppEnteredBackground;
        Application.Current.LeavingBackground -= OnAppLeavingBackground;

        ApplicationView view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
        {
            view.ExitFullScreenMode();
        }

        TryStopBrightnessOverride();

        _displayRequest?.RequestRelease();
    }

    private void OnAppEnteredBackground(object sender, EnteredBackgroundEventArgs e)
    {
        TryStopBrightnessOverride();
    }

    private void OnAppLeavingBackground(object sender, LeavingBackgroundEventArgs e)
    {
        TryStartBrightnessOverride();
    }

    private void OnDispatcherAcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
    {
        if (args.VirtualKey == VirtualKey.Escape)
        {
            ExitGlanceMode();
            args.Handled = true;
        }
    }

    private void TryStartBrightnessOverride()
    {
        if (_brightnessOverride is not null && _brightnessOverride.IsSupported)
        {
            _brightnessOverride.SetBrightnessLevel(0, DisplayBrightnessOverrideOptions.UseDimmedPolicyWhenBatteryIsLow);
            _brightnessOverride.StartOverride();
        }
    }

    private void TryStopBrightnessOverride()
    {
        if (_brightnessOverride is not null && _brightnessOverride.IsSupported)
        {
            _brightnessOverride.StopOverride();
        }
    }

    private void OnContentLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ContentOffset = ContentCanvas.ActualHeight - ContentStackPanel.ActualHeight;
    }

    private void OnPageDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ExitGlanceMode();
    }

    private static void ExitGlanceMode()
    {
        MainPageNavigationHelper.GoBack();
    }
}
