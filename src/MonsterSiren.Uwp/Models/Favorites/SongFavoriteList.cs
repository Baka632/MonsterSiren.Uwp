using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MonsterSiren.Uwp.Models.Favorites;

public class SongFavoriteList : INotifyPropertyChanged, IEquatable<SongFavoriteList>
{
    public event PropertyChangedEventHandler PropertyChanged;
    private Uri _playlistCoverImageUri;
    private bool isBlocking = false;

    /// <summary>
    /// 收藏夹的封面图。
    /// </summary>
    [JsonIgnore]
    public Uri PlaylistCoverImageUri
    {
        get => _playlistCoverImageUri;
        set
        {
            _playlistCoverImageUri = value;
            OnPropertiesChanged();
        }
    }

    /// <summary>
    /// 收藏夹的总时长。
    /// </summary>
    public TimeSpan TotalDuration { get; private set; }

    /// <summary>
    /// 收藏夹的歌曲个数。
    /// </summary>
    [JsonIgnore]
    public int SongCount { get => Items.Count; }

    /// <summary>
    /// 收藏夹的歌曲列表。
    /// </summary>
    public ObservableCollection<SongFavoriteItem> Items { get; private set; } = [];


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
        _ = SelectCoverImage();
    }

    private async void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (isBlocking)
        {
            return;
        }

        if (e.Action is not NotifyCollectionChangedAction.Move)
        {
            TotalDuration = CalculateTotalTimeSpan();
            OnPropertiesChanged(nameof(TotalDuration));
        }

        OnPropertiesChanged(nameof(SongCount));

        if (e.NewStartingIndex == 0 || e.OldStartingIndex == 0)
        {
            await SelectCoverImage();
        }

        await FavoriteService.SaveSongFavoriteList();
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

    private async Task SelectCoverImage()
    {
        if (Items.Count > 0)
        {
            SongFavoriteItem item = Items[0];

            try
            {
                Uri uri = await MsrModelsHelper.GetAlbumCoverAsync(item.AlbumCid);
                if (PlaylistCoverImageUri != uri)
                {
                    PlaylistCoverImageUri = uri;
                }
            }
            catch (HttpRequestException)
            {
                PlaylistCoverImageUri = null;
            }
            catch (ArgumentOutOfRangeException)
            {
                isBlocking = true;
                Items[0] = item with { IsCorruptedItem = true };
                isBlocking = false;
                PlaylistCoverImageUri = null;
            }
        }
        else
        {
            PlaylistCoverImageUri = null;
        }
    }

    public IEnumerator<SongFavoriteItem> GetEnumerator()
    {
        return Items.GetEnumerator();
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

    /// <summary>
    /// 阻止收藏夹在其集合更新时更新自身信息。请务必在完成操作后调用 <see cref="RestoreInfoUpdateAsync"/> 方法。
    /// </summary>
    public void BlockInfoUpdate()
    {
        isBlocking = true;
    }

    /// <summary>
    /// 恢复收藏夹更新自身信息的功能，并立刻无条件地进行一次信息更新。
    /// </summary>
    public async Task RestoreInfoUpdateAsync()
    {
        isBlocking = false;

        TotalDuration = CalculateTotalTimeSpan();
        OnPropertiesChanged(nameof(TotalDuration));
        OnPropertiesChanged(nameof(SongCount));
        await SelectCoverImage();

        await FavoriteService.SaveSongFavoriteList();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as SongFavoriteList);
    }

    public bool Equals(SongFavoriteList other)
    {
        return other is not null &&
               EqualityComparer<Uri>.Default.Equals(PlaylistCoverImageUri, other.PlaylistCoverImageUri) &&
               TotalDuration.Equals(other.TotalDuration) &&
               SongCount == other.SongCount &&
               EqualityComparer<ObservableCollection<SongFavoriteItem>>.Default.Equals(Items, other.Items);
    }

    public override int GetHashCode()
    {
        int hashCode = 230909774;
        hashCode = hashCode * -1521134295 + EqualityComparer<Uri>.Default.GetHashCode(PlaylistCoverImageUri);
        hashCode = hashCode * -1521134295 + TotalDuration.GetHashCode();
        hashCode = hashCode * -1521134295 + SongCount.GetHashCode();
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
