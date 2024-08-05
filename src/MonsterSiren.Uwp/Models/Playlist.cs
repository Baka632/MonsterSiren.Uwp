using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
public partial class Playlist : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private string _title;
    private string _description;
    private BitmapImage _playlistCoverImage;

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

    // HACK: How to count it...
    /// <summary>
    /// 播放列表的总时长（以毫秒为单位）
    /// </summary>
    public int TotalDurationInMillisecond { get; private set; }

    /// <summary>
    /// 当前播放列表的歌曲个数
    /// </summary>
    [JsonIgnore]
    public int SongCount { get => Items.Count; }

    /// <summary>
    /// 播放列表的歌曲列表
    /// </summary>
    public ObservableCollection<SongDetailAndAlbumDetailPack> Items { get; private set; } = [];

    public Playlist(string title, string description)
    {
        _title = title;
        _description = description;
        Items.CollectionChanged += OnItemCollectionChanged;
    }

    [JsonConstructor]
    public Playlist(string title, string description, ObservableCollection<SongDetailAndAlbumDetailPack> items)
    {
        _title = title;
        _description = description;
        Items = items;
        Items.CollectionChanged += OnItemCollectionChanged;
    }

    private async void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertiesChanged(nameof(TotalDurationInMillisecond));
        OnPropertiesChanged(nameof(SongCount));

        if (Items.Count > 0)
        {
            SongDetailAndAlbumDetailPack pack = Items[0];
            Uri uri = await FileCacheHelper.GetAlbumCoverUriAsync(pack.AlbumDetail)
                ?? new(pack.AlbumDetail.CoverUrl, UriKind.Absolute);

            await UIThreadHelper.RunOnUIThread(() =>
            {
                PlaylistCoverImage = new(uri);
            });
        }
        else
        {
            PlaylistCoverImage = null;
        }
    }

    public IEnumerator<SongDetailAndAlbumDetailPack> GetEnumerator()
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
}
