using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Windows.UI.Xaml.Media.Imaging;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个播放列表的类
/// </summary>
/// <param name="title">播放列表标题</param>
/// <param name="description">播放列表描述</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public partial class Playlist : INotifyPropertyChanged, IEquatable<Playlist>
{
    public event PropertyChangedEventHandler PropertyChanged;
    private string _title;
    private string _description;
    private BitmapImage _playlistCoverImage;
    private bool isBlocking = false;

    /// <summary>
    /// 播放列表的标题
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertiesChanged();
        }
    }

    /// <summary>
    /// 播放列表的描述
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertiesChanged();
        }
    }

    /// <summary>
    /// 播放列表的封面图
    /// </summary>
    [JsonIgnore]
    public BitmapImage PlaylistCoverImage
    {
        get => _playlistCoverImage;
        set
        {
            _playlistCoverImage = value;
            OnPropertiesChanged();
        }
    }

    /// <summary>
    /// 播放列表的总时长
    /// </summary>
    public TimeSpan TotalDuration { get; private set; }

    /// <summary>
    /// 播放列表的 ID
    /// </summary>
    public Guid PlaylistId { get; private set; }

    /// <summary>
    /// 播放列表的保存名称
    /// </summary>
    public string PlaylistSaveName { get; set; }

    /// <summary>
    /// 当前播放列表的歌曲个数
    /// </summary>
    [JsonIgnore]
    public int SongCount { get => Items.Count; }

    /// <summary>
    /// 播放列表的歌曲列表
    /// </summary>
    public ObservableCollection<PlaylistItem> Items { get; private set; } = [];

    public Playlist(string title, string description)
    {
        _title = title;
        _description = description;
        PlaylistId = Guid.NewGuid();
        PlaylistSaveName = CommonValues.ReplaceInvaildFileNameChars(title);
        Items.CollectionChanged += OnItemCollectionChanged;
    }

    [JsonConstructor]
    public Playlist(string title,
                    string description,
                    ObservableCollection<PlaylistItem> items,
                    TimeSpan totalDuration,
                    Guid playlistId,
                    string playlistSaveName)
    {
        _title = title;
        _description = description;
        TotalDuration = totalDuration;
        PlaylistId = playlistId == Guid.Empty ? Guid.NewGuid() : playlistId;
        PlaylistSaveName = string.IsNullOrWhiteSpace(playlistSaveName)
            ? CommonValues.ReplaceInvaildFileNameChars(title)
            : playlistSaveName;
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
        
        await PlaylistService.SavePlaylistAsync(this);
    }

    private TimeSpan CalculateTotalTimeSpan()
    {
        TimeSpan span = TimeSpan.Zero;
        foreach (PlaylistItem item in Items)
        {
            span += item.SongDuration;
        }
        return span;
    }

    private async Task SelectCoverImage()
    {
        if (Items.Count > 0)
        {
            try
            {
                PlaylistItem item = Items[0];
                Uri uri = await MsrModelsHelper.GetAlbumCoverAsync(item.AlbumCid);

                await UIThreadHelper.RunOnUIThread(() =>
                {
                    if (PlaylistCoverImage?.UriSource == uri)
                    {
                        return;
                    }

                    PlaylistCoverImage = new(uri);
                });
            }
            catch (HttpRequestException)
            {
                PlaylistCoverImage = null;
            }
        }
        else
        {
            PlaylistCoverImage = null;
        }
    }

    public IEnumerator<PlaylistItem> GetEnumerator()
    {
        return Items.GetEnumerator();
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

    private string GetDebuggerDisplay() => Title;

    public override bool Equals(object obj)
    {
        return Equals(obj as Playlist);
    }

    /// <summary>
    /// 阻止播放列表在其集合更新时更新自身信息。请务必在完成操作后调用 <see cref="RestoreInfoUpdateAsync"/> 方法。
    /// </summary>
    public void BlockInfoUpdate()
    {
        isBlocking = true;
    }

    /// <summary>
    /// 恢复播放列表更新自身信息的功能，并立刻无条件地进行一次信息更新。
    /// </summary>
    public async Task RestoreInfoUpdateAsync()
    {
        isBlocking = false;

        TotalDuration = CalculateTotalTimeSpan();
        OnPropertiesChanged(nameof(TotalDuration));
        OnPropertiesChanged(nameof(SongCount));
        await SelectCoverImage();
        await PlaylistService.SavePlaylistAsync(this);
    }

    public bool Equals(Playlist other)
    {
        return other is not null &&
               Title == other.Title &&
               Description == other.Description &&
               TotalDuration == other.TotalDuration &&
               SongCount == other.SongCount &&
               PlaylistId == other.PlaylistId &&
               PlaylistSaveName == other.PlaylistSaveName &&
               Items.SequenceEqual(other.Items);
    }

    public override int GetHashCode()
    {
        int hashCode = 1910073755;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
        hashCode = hashCode * -1521134295 + EqualityComparer<BitmapImage>.Default.GetHashCode(PlaylistCoverImage);
        hashCode = hashCode * -1521134295 + TotalDuration.GetHashCode();
        hashCode = hashCode * -1521134295 + PlaylistId.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PlaylistSaveName);
        hashCode = hashCode * -1521134295 + SongCount.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<ObservableCollection<PlaylistItem>>.Default.GetHashCode(Items);
        return hashCode;
    }

    public static bool operator ==(Playlist left, Playlist right)
    {
        return EqualityComparer<Playlist>.Default.Equals(left, right);
    }

    public static bool operator !=(Playlist left, Playlist right)
    {
        return !(left == right);
    }
}
