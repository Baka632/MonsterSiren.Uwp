using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

public class AlbumFavoriteList : INotifyPropertyChanged, IEquatable<AlbumFavoriteList>
{
    public event PropertyChangedEventHandler PropertyChanged;

    private bool isBlocking = false;

    /// <summary>
    /// 收藏夹的专辑个数。
    /// </summary>
    [JsonIgnore]
    public int AlbumCount => Items.Count;

    /// <summary>
    /// 收藏夹的专辑列表。
    /// </summary>
    public ObservableCollection<AlbumFavoriteItem> Items { get; private set; } = [];

    public AlbumFavoriteList() { }

    [JsonConstructor]
    public AlbumFavoriteList(ObservableCollection<AlbumFavoriteItem> items)
    {
        Items = items;
        Items.CollectionChanged += OnItemCollectionChanged;
    }

    private async void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (isBlocking) return;

        if (e.Action != NotifyCollectionChangedAction.Move)
        {
            OnPropertiesChanged(nameof(AlbumCount));
        }

        await FavoriteService.SaveAlbumFavoriteList();
    }

    public IEnumerator<AlbumFavoriteItem> GetEnumerator() => Items.GetEnumerator();

    public async void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        await UIThreadHelper.RunOnUIThread(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

    /// <summary>
    /// 阻止收藏夹在其集合更新时更新自身信息。请务必在完成操作后调用 <see cref="RestoreInfoUpdateAsync"/>。
    /// </summary>
    public void BlockInfoUpdate() => isBlocking = true;

    /// <summary>
    /// 恢复收藏夹更新自身信息的功能，并立刻无条件地进行一次信息更新。
    /// </summary>
    public async Task RestoreInfoUpdateAsync()
    {
        isBlocking = false;
        OnPropertiesChanged(nameof(AlbumCount));
        await FavoriteService.SaveAlbumFavoriteList();
    }

    public override bool Equals(object obj) => Equals(obj as AlbumFavoriteList);
    public bool Equals(AlbumFavoriteList other) =>
        other is not null &&
        AlbumCount == other.AlbumCount &&
        EqualityComparer<ObservableCollection<AlbumFavoriteItem>>.Default.Equals(Items, other.Items);

    public override int GetHashCode()
    {
        var hashCode = -1903991810;
        hashCode = hashCode * -1521134295 + AlbumCount.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<AlbumFavoriteItem>>.Default.GetHashCode(Items);
        return hashCode;
    }

    public static bool operator ==(AlbumFavoriteList left, AlbumFavoriteList right) => EqualityComparer<AlbumFavoriteList>.Default.Equals(left, right);
    public static bool operator !=(AlbumFavoriteList left, AlbumFavoriteList right) => !(left == right);
}