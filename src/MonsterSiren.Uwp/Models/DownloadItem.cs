using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Networking.BackgroundTransfer;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个下载项。
/// </summary>
public sealed record DownloadItem : INotifyPropertyChanged
{
    private double _progress;
    private DownloadItemState _state = DownloadItemState.Paused;
    private Exception _errorException;

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 获取表示当前的下载操作的 <see cref="DownloadOperation"/>。
    /// </summary>
    public DownloadOperation Operation { get; init; }
    /// <summary>
    /// 当前下载项的显示名称。
    /// </summary>
    public string DisplayName { get; init; }
    /// <summary>
    /// 获取指示下载操作是否应当取消的 <see cref="CancellationTokenSource"/>。
    /// </summary>
    public CancellationTokenSource CancelToken { get; init; }

    /// <summary>
    /// 获取下载或转码操作的进度。
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                OnPropertiesChanged();
                EnsureStatus();
            }
        }
    }

    /// <summary>
    /// 获取下载操作的状态。
    /// </summary>
    public DownloadItemState State
    {
        get => _state;
        internal set
        {
            if (_state != value)
            {
                _state = value;
                OnPropertiesChanged();
            }
        }
    }

    /// <summary>
    /// 获取导致下载操作出现错误的异常。
    /// </summary>
    public Exception ErrorException
    {
        get => _errorException;
        internal set
        {
            if (_errorException != value)
            {
                _errorException = value;
                OnPropertiesChanged();
            }
        }
    }

    /// <summary>
    /// 使用指定的参数构造 <see cref="DownloadItem"/> 的新实例。
    /// </summary>
    /// <param name="op">表示下载操作的 <see cref="DownloadOperation"/>。</param>
    /// <param name="displayName">下载项的显示名称。</param>
    public DownloadItem(DownloadOperation op, string displayName)
    {
        Operation = op;
        DisplayName = displayName;
        CancelToken = new CancellationTokenSource();

        BackgroundDownloadProgress progress = op.Progress;
        Progress = progress.TotalBytesToReceive == 0 ? 0d : (double)progress.BytesReceived / progress.TotalBytesToReceive;
        EnsureStatus();
    }

    /// <summary>
    /// 构造一个占位下载项，其不进行实际的下载操作。
    /// </summary>
    /// <param name="displayName">下载项的显示名称。</param>
    /// <param name="state">下载项的状态。</param>
    public DownloadItem(string displayName, DownloadItemState state)
    {
        Operation = null;
        CancelToken = null;
        DisplayName = displayName;
        State = state;
        Progress = 1d;
    }

    /// <summary>
    /// 恢复下载。
    /// </summary>
    public void ResumeDownload()
    {
        if (State is DownloadItemState.Skipped or DownloadItemState.Downloading)
        {
            return;
        }

        Operation.Resume();
        State = DownloadItemState.Downloading;
    }

    /// <summary>
    /// 暂停下载。
    /// </summary>
    public void PauseDownload()
    {
        if (State is DownloadItemState.Skipped or DownloadItemState.Paused)
        {
            return;
        }

        Operation.Pause();
        State = DownloadItemState.Paused;
    }

    /// <summary>
    /// 取消下载。
    /// </summary>
    public void CancelDownload()
    {
        if (State is DownloadItemState.Skipped or DownloadItemState.Cancelling)
        {
            return;
        }

        CancelToken.Cancel();
        State = DownloadItemState.Cancelling;
    }

    /// <summary>
    /// 通知运行时属性已经发生更改。
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称，其填充是自动完成的。</param>
    public async void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

    private void EnsureStatus()
    {
        if (Operation is not null
            && State is DownloadItemState.Canceled or DownloadItemState.Done or DownloadItemState.Error or DownloadItemState.Paused or DownloadItemState.Downloading)
        {
            BackgroundTransferStatus status = Operation.Progress.Status;
            DownloadItemState dlStateFromOpState = status switch
            {
                // 如果下载项是恢复过来的，那么PausedNoNetwork 状态会在操作系统排队下载项时候出现，奇怪......
                // 而且如果 BackgroundTransferStatus 是 PausedNoNetwork，调用 Resume 反而抛出异常。
                BackgroundTransferStatus.Running or BackgroundTransferStatus.PausedNoNetwork => DownloadItemState.Downloading,
                BackgroundTransferStatus.Idle
                    or BackgroundTransferStatus.PausedByApplication
                    or BackgroundTransferStatus.PausedSystemPolicy
                    or BackgroundTransferStatus.PausedRecoverableWebErrorStatus
                    or BackgroundTransferStatus.PausedCostedNetwork => DownloadItemState.Paused,
                BackgroundTransferStatus.Error => DownloadItemState.Error,
                BackgroundTransferStatus.Canceled => DownloadItemState.Canceled,
                BackgroundTransferStatus.Completed => DownloadItemState.Done,
#if DEBUG
                _ => throw new NotImplementedException("未知的 BackgroundTransferStatus 值。")
#else
            _ => default
#endif
            };
            State = dlStateFromOpState;
        }
    }
}
