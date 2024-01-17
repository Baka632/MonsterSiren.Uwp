using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;
using Windows.Networking.BackgroundTransfer;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个下载项
/// </summary>
public sealed record DownloadItem : INotifyPropertyChanged
{
    private ulong progress;

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
    public ulong Progress
    {
        get => progress;
        set
        {
            progress = value;
            OnPropertiesChanged();
        }
    }

    public DownloadItem(DownloadOperation op, string name, CancellationTokenSource cancelToken)
    {
        Operation = op;
        Name = name;
        CancelToken = cancelToken;
    }

    public event PropertyChangedEventHandler PropertyChanged;

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
