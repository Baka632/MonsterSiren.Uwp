using Windows.Media.Casting;
using Windows.UI.Popups;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 为投送媒体提供服务的类。
/// </summary>
public static class MediaCastService
{
    private static readonly CastingDevicePicker castingDevicePicker;
    private static CastingConnection currentConnection;
    private static bool _isMediaCasting;

    public static event Action<bool> MediaCastingStateChanged;

    /// <summary>
    /// 指示当前平台是否支持媒体投送的值。
    /// </summary>
    public static readonly bool IsSupported = !CommonValues.IsXbox;

    /// <summary>
    /// 指示当前是否正在投送音频的值。
    /// </summary>
    public static bool IsMediaCasting
    {
        get => _isMediaCasting;
        set
        {
            if (_isMediaCasting != value)
            {
                _isMediaCasting = value;
                InvokeMediaCastingStateChanged();
            }
        }
    }

    static MediaCastService()
    {
        if (IsSupported)
        {
            castingDevicePicker = new();
            castingDevicePicker.Filter.SupportsAudio = true;
            castingDevicePicker.CastingDeviceSelected += OnCastingDevicePickerCastingDeviceSelected;
        }
        else
        {
            IsMediaCasting = false; 
        }
    }

    /// <summary>
    /// 显示设备选择器。
    /// </summary>
    /// <param name="rect">选择器的显示区域。</param>
    /// <param name="placement">选择器的位置。</param>
    public static void ShowCastingDevicePicker(Rect rect, Placement placement = Placement.Default)
    {
        if (!IsSupported)
        {
            throw new NotSupportedException("Xbox 上不支持媒体投送功能。");
        }

        castingDevicePicker.Show(rect, placement);
    }

    public static async void StopCasting()
    {
        if (currentConnection is not null)
        {
            await CleanupForCurrentConnection(currentConnection);
            IsMediaCasting = false;
        }
    }

    private async static void InvokeMediaCastingStateChanged()
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            MediaCastingStateChanged?.Invoke(IsMediaCasting);
        });
    }

    private async static void OnCastingDevicePickerCastingDeviceSelected(CastingDevicePicker sender, CastingDeviceSelectedEventArgs args)
    {
        await UIThreadHelper.RunOnUIThread(async () =>
        {
            if (currentConnection is not null)
            {
                await CleanupForCurrentConnection(currentConnection);
            }

            CastingConnection connection = args.SelectedCastingDevice.CreateCastingConnection();
            connection.StateChanged += OnCastingsConnectionStateChanged;
            connection.ErrorOccurred += OnCastingConnectionErrorOccurred;
            currentConnection = connection;

            CastingSource source = MusicService.GetCastingSource();
            CastingConnectionErrorStatus result = await connection.RequestStartCastingAsync(source);

            if (result == CastingConnectionErrorStatus.Succeeded)
            {
                IsMediaCasting = true;
            }
            else
            {
                await CleanupForCurrentConnection(currentConnection);
                IsMediaCasting = false;
            }
        });
    }

    private async static void OnCastingConnectionErrorOccurred(CastingConnection sender, CastingConnectionErrorOccurredEventArgs args)
    {
        await CleanupForCurrentConnection(sender);
    }

    private static async void OnCastingsConnectionStateChanged(CastingConnection sender, object args)
    {
        switch (sender.State)
        {
            case CastingConnectionState.Disconnected:
                if (IsMediaCasting)
                {
                    await CleanupForCurrentConnection(sender);
                    IsMediaCasting = false;
                }
                break;
            case CastingConnectionState.Connected:
                IsMediaCasting = true;
                break;
            case CastingConnectionState.Rendering:
            case CastingConnectionState.Disconnecting:
            case CastingConnectionState.Connecting:
            default:
                // :-)
                break;
        }
    }

    private static async Task CleanupForCurrentConnection(CastingConnection connection)
    {
        if (connection is null)
        {
            return;
        }

        await CleanupConnection(connection);
        if (connection == currentConnection)
        {
            currentConnection = null;
        }
    }

    private static async Task CleanupConnection(CastingConnection connection)
    {
        if (connection is null)
        {
            return;
        }

        connection.StateChanged -= OnCastingsConnectionStateChanged;
        connection.ErrorOccurred -= OnCastingConnectionErrorOccurred;
        await connection.DisconnectAsync();
        connection.Dispose();
    }
}
