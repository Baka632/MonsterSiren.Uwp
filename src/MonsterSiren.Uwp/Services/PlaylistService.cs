using System.Collections.ObjectModel;
using Windows.Media.Playback;
using Windows.Storage;

namespace MonsterSiren.Uwp.Services;

public static class PlaylistService
{
    private static bool _isInitialized;
    private static string _playlistSavePath;

    /// <summary>
    /// 当前可用的播放列表集合
    /// </summary>
    public static ObservableCollection<Playlist> TotalPlaylists { get; } =
    [
        new Playlist("偶像空的专属播放列表✨", "闪闪发光！"),
        new Playlist("霜叶的播放列表❄️", "“哼——哼哼♪哼......哼哼......♪”"),
        new Playlist("阿米娅的小提琴合集🎻", ""),
        new Playlist("小刻de画图写话🎨", "来一起玩耍！"),
        new Playlist("音律联觉合集📀", "曲调无限延伸，风格变幻多样，\n从经典到新锐，将大地的旋律倾情奉上。"),
    ];

    /// <summary>
    /// 指示应用是否因某种原因而改变播放列表文件夹的默认路径
    /// </summary>
    public static bool PlaylistPathRedirected { get; private set; }

    /// <summary>
    /// 获取或设置播放列表的保存路径
    /// </summary>
    public static string PlaylistSavePath
    {
        get => _playlistSavePath;
        set
        {
            SettingsHelper.Set(CommonValues.PlaylistSavePathSettingsKey, value);
            _playlistSavePath = value;
            PlaylistPathRedirected = false;
        }
    }

    /// <summary>
    /// 初始化播放列表服务
    /// </summary>
    public static async Task Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        if (SettingsHelper.TryGet(CommonValues.PlaylistSavePathSettingsKey, out string path) && await IsFolderExist(path))
        {
            PlaylistSavePath = path;
        }
        else
        {
            if (path != null)
            {
                PlaylistPathRedirected = true;
            }

            try
            {
                StorageLibrary musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                StorageFolder storageFolder = await musicLib.SaveFolder.CreateFolderAsync(App.AppDisplayName, CreationCollisionOption.OpenIfExists);
                StorageFolder targetFolder = await storageFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
                path = targetFolder.Path;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or NullReferenceException)
            {
                StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
                StorageFolder targetFolder = await localCacheFolder.CreateFolderAsync("Playlists", CreationCollisionOption.OpenIfExists);
                path = targetFolder.Path;
            }

            PlaylistSavePath = path;
        }

        _isInitialized = true;
    }

    /// <summary>
    /// 创建新的播放列表
    /// </summary>
    /// <param name="title">播放列表标题</param>
    /// <param name="description">播放列表描述</param>
    /// <exception cref="ArgumentException"><paramref name="title"/> 为 null 或空白。</exception>
    /// <exception cref="InvalidOperationException">已经包含了一个名称相同的播放列表。</exception>
    public static void CreateNewPlaylist(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException($"“{nameof(title)}”不能为 null 或空白。", nameof(title));
        }

        Playlist playlist = new(title, description ?? string.Empty);
        if (TotalPlaylists.Any(item => item.Title == playlist.Title))
        {
            throw new InvalidOperationException("已经包含了一个名称相同的播放列表。");
        }

        TotalPlaylists.Add(playlist);
    }

    /// <summary>
    /// 移除指定的播放列表
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例</param>
    public static void RemovePlaylist(Playlist playlist)
    {
        TotalPlaylists.Remove(playlist);
    }

    /// <summary>
    /// 播放指定的播放列表
    /// </summary>
    /// <param name="playlist">要播放的播放列表</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    public static async Task PlayForPlaylistAsync(Playlist playlist)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

        await Task.Run(() =>
        {
            List<MediaPlaybackItem> list = new(playlist.Items.Count);
            foreach (SongDetailAndAlbumDetailPack item in playlist)
            {
                list.Add(item.SongDetail.ToMediaPlaybackItem(item.AlbumDetail));
            }

            if (list.Count != 0)
            {
                MusicService.ReplaceMusic(list);
            }
            else
            {
                WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            }
        });
    }

    /// <summary>
    /// 将指定的播放列表添加到正在播放列表中
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    public static async Task AddPlaylistToNowPlayingAsync(Playlist playlist)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        await Task.Run(() =>
        {
            List<MediaPlaybackItem> list = new(playlist.Items.Count);
            foreach (SongDetailAndAlbumDetailPack item in playlist)
            {
                list.Add(item.SongDetail.ToMediaPlaybackItem(item.AlbumDetail));
            }

            MusicService.AddMusic(list);
        });
    }

    /// <summary>
    /// 向指定的播放列表添加歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="songDetail">表示歌曲详细信息的 <see cref="SongDetail"/> 实例</param>
    /// <param name="albumDetail">表示歌曲所属专辑详细信息的 <see cref="AlbumDetail"/> 实例</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException"><paramref name="songDetail"/> 中所属专辑的 CID 和 <paramref name="albumDetail"/> 中的 CID 不符。</exception>
    public static async Task AddItemForPlaylist(Playlist playlist, SongDetail songDetail, AlbumDetail albumDetail)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (songDetail.AlbumCid != albumDetail.Cid)
        {
            throw new ArgumentException("歌曲信息中所属专辑的 CID 和专辑信息中的 CID 不符。");
        }

        SongDetailAndAlbumDetailPack pack = new(songDetail, albumDetail);
        await AddItemForPlaylistAsync(playlist, pack);
    }

    /// <summary>
    /// 向指定的播放列表添加歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="pack">一个 <see cref="SongDetailAndAlbumDetailPack"/> 实例</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">歌曲信息中所属专辑的 CID 和专辑信息中的 CID 不符。</exception>
    public static async Task AddItemForPlaylistAsync(Playlist playlist, SongDetailAndAlbumDetailPack pack)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (pack.SongDetail.AlbumCid != pack.AlbumDetail.Cid)
        {
            throw new ArgumentException("歌曲信息中所属专辑的 CID 和专辑信息中的 CID 不符。");
        }

        if (playlist.Items.Contains(pack))
        {
            return;
        }

        await UIThreadHelper.RunOnUIThread(() =>
        {
            playlist.Items.Add(pack);
        });
    }

    /// <summary>
    /// 在指定的播放列表中移除歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="pack">一个 <see cref="SongDetailAndAlbumDetailPack"/> 实例</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    public static void RemoveItemForPlaylist(Playlist playlist, SongDetailAndAlbumDetailPack pack)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        playlist.Items.Remove(pack);
    }

    /// <summary>
    /// 在指定的播放列表中移除歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="songDetail">一个 <see cref="SongDetail"/> 实例</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    public static void RemoveItemForPlaylist(Playlist playlist, SongDetail songDetail)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        SongDetailAndAlbumDetailPack targetItem = playlist.Items.FirstOrDefault(pack => pack.SongDetail == songDetail);
        playlist.Items.Remove(targetItem);
    }

    private static async Task<bool> IsFolderExist(string path)
    {
        try
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            return folder != null;
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException or ArgumentException)
        {
            return false;
        }
    }
}
