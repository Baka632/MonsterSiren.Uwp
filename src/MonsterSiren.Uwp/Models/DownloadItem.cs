using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Networking.BackgroundTransfer;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个下载项
/// </summary>
public sealed record DownloadItem : INotifyPropertyChanged
{
    private double _progress;
    private bool _IsPaused;
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 表示当前的下载操作
    /// </summary>
    public DownloadOperation Operation { get; init; }
    /// <summary>
    /// 当前下载项的名称
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// 表示指示下载操作是否应当取消的 <see cref="CancellationTokenSource"/>
    /// </summary>
    public CancellationTokenSource CancelToken { get; init; }

    /// <summary>
    /// 表示下载操作的进度
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            OnPropertiesChanged();
        }
    }

    /// <summary>
    /// 指示下载操作是否暂停的值
    /// </summary>
    public bool IsPaused
    {
        get => _IsPaused;
        set
        {
            _IsPaused = value;
            OnPropertiesChanged();
        }
    }

    public DownloadItem(DownloadOperation op, string name, CancellationTokenSource cancelToken)
    {
        Operation = op;
        Name = name;
        CancelToken = cancelToken;
    }

    /// <summary>
    /// 恢复下载
    /// </summary>
    public void ResumeDownload()
    {
        Operation.Resume();
        IsPaused = false;
    }
    
    /// <summary>
    /// 暂停下载
    /// </summary>
    public void PauseDownload()
    {
        Operation.Pause();
        IsPaused = true;
    }

    /// <summary>
    /// 通知运行时属性已经发生更改
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
    public async void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }
}
