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
        new Playlist("偶像空的专属播放列表✨", ""),
        new Playlist("霜叶的播放列表❄️", ""),
        new Playlist("阿米娅的小提琴合集🎻", ""),
        new Playlist("小刻de画图写话🎨", ""),
        new Playlist("音律联觉合集📀", ""),
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
    /// <returns></returns>
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
    /// 播放指定的播放列表
    /// </summary>
    /// <param name="playlist">要播放的播放列表</param>
    public static async Task PlayForPlaylist(Playlist playlist)
    {
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
