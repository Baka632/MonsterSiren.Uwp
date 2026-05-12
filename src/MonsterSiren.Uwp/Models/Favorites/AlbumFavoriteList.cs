using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

/// <summary>
/// 专辑收藏夹。
/// </summary>
public sealed class AlbumFavoriteList : FavoriteList<AlbumFavoriteItem>, IEquatable<AlbumFavoriteList>
{
    public AlbumFavoriteList(){ }

    [JsonConstructor]
    public AlbumFavoriteList(ObservableCollection<AlbumFavoriteItem> items)
    {
        Items = items;
        Items.CollectionChanged += OnItemCollectionChanged;
    }

    private async void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsBlocking) return;

        if (e.Action != NotifyCollectionChangedAction.Move)
        {
            OnPropertiesChanged(nameof(Count));
        }

        await FavoriteService.SaveFavoriteList(FavoriteType.Album);
    }

    public IEnumerator<AlbumFavoriteItem> GetEnumerator() => Items.GetEnumerator();

    public override async Task RestoreInfoUpdateAsync()
    {
        await base.RestoreInfoUpdateAsync();
        OnPropertiesChanged(nameof(Count));
        await FavoriteService.SaveFavoriteList(FavoriteType.Album);
    }

    public override bool Equals(object obj) => Equals(obj as AlbumFavoriteList);
    public bool Equals(AlbumFavoriteList other) =>
        other is not null &&
        Count == other.Count &&
        Items.SequenceEqual(other.Items);

    public override int GetHashCode()
    {
        int hashCode = -1903991810;
        hashCode = hashCode * -1521134295 + Count.GetHashCode();
        foreach (AlbumFavoriteItem item in Items)
        {
            hashCode = hashCode * -1521134295 + EqualityComparer<AlbumFavoriteItem>.Default.GetHashCode(item);
        }
        return hashCode;
    }

    public static bool operator ==(AlbumFavoriteList left, AlbumFavoriteList right) => EqualityComparer<AlbumFavoriteList>.Default.Equals(left, right);
    public static bool operator !=(AlbumFavoriteList left, AlbumFavoriteList right) => !(left == right);
}