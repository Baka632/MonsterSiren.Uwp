using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

/// <summary>
/// 歌曲收藏夹。
/// </summary>
public sealed class SongFavoriteList : FavoriteList<SongFavoriteItem>, IEquatable<SongFavoriteList>
{
    /// <summary>
    /// 收藏夹的总时长。
    /// </summary>
    public TimeSpan TotalDuration { get; private set; }

    public SongFavoriteList()
    {
    }

    [JsonConstructor]
    public SongFavoriteList(ObservableCollection<SongFavoriteItem> items,
                    TimeSpan totalDuration)
    {
        TotalDuration = totalDuration;
        Items = items;
        Items.CollectionChanged += OnItemCollectionChanged;
    }

    private async void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsBlocking)
        {
            return;
        }

        if (e.Action is not NotifyCollectionChangedAction.Move)
        {
            TotalDuration = CalculateTotalTimeSpan();
            OnPropertiesChanged(nameof(TotalDuration));
        }

        OnPropertiesChanged(nameof(Count));

        await FavoriteService.SaveFavoriteList(FavoriteType.Song);
    }

    private TimeSpan CalculateTotalTimeSpan()
    {
        TimeSpan span = TimeSpan.Zero;
        foreach (SongFavoriteItem item in Items)
        {
            span += item.SongDuration;
        }
        return span;
    }

    public IEnumerator<SongFavoriteItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    public override async Task RestoreInfoUpdateAsync()
    {
        await base.RestoreInfoUpdateAsync();

        TotalDuration = CalculateTotalTimeSpan();
        OnPropertiesChanged(nameof(TotalDuration));
        OnPropertiesChanged(nameof(Count));

        await FavoriteService.SaveFavoriteList(FavoriteType.Song);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as SongFavoriteList);
    }

    public bool Equals(SongFavoriteList other)
    {
        return other is not null &&
               TotalDuration.Equals(other.TotalDuration) &&
               Count == other.Count &&
               Items.SequenceEqual(other.Items);
    }

    public override int GetHashCode()
    {
        int hashCode = 230909774;
        hashCode = hashCode * -1521134295 + TotalDuration.GetHashCode();
        hashCode = hashCode * -1521134295 + Count.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<SongFavoriteItem>>.Default.GetHashCode(Items);
        return hashCode;
    }

    public static bool operator ==(SongFavoriteList left, SongFavoriteList right)
    {
        return EqualityComparer<SongFavoriteList>.Default.Equals(left, right);
    }

    public static bool operator !=(SongFavoriteList left, SongFavoriteList right)
    {
        return !(left == right);
    }
}
