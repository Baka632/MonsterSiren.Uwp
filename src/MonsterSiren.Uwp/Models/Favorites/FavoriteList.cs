using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

/// <summary>
/// 为收藏夹提供基类。
/// </summary>
/// <typeparam name="T">收藏夹内容的类型。</typeparam>
public abstract class FavoriteList<T> : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 收藏夹的项目个数。
    /// </summary>
    [JsonIgnore]
    public int Count => Items.Count;

    /// <summary>
    /// 收藏夹的项目列表。
    /// </summary>
    public ObservableCollection<T> Items { get; protected set; } = [];

    /// <summary>
    /// 指示收藏夹是否被阻止更新信息。
    /// </summary>
    protected bool IsBlocking { get; set;  }

    /// <summary>
    /// 阻止收藏夹在其集合更新时更新自身信息。请务必在完成操作后调用 <see cref="RestoreInfoUpdateAsync"/>。
    /// </summary>
    public virtual void BlockInfoUpdate() => IsBlocking = true;

    /// <summary>
    /// 恢复收藏夹更新自身信息的功能，并立刻无条件地进行一次信息更新。
    /// </summary>
    public virtual async Task RestoreInfoUpdateAsync() => IsBlocking = false;

    /// <summary>
    /// 通知运行时属性已经发生更改。
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称，其填充是自动完成的。</param>
    protected async void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }
}
