using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Media.Playback;
using Windows.Storage;

namespace MonsterSiren.Uwp.Services;

/// <summary>
/// 应用程序播放列表服务
/// </summary>
public static class PlaylistService
{
    public const string PlaylistFileExtension = ".sora-playlist";
    private static readonly SemaphoreSlim playlistFileSemaphore = new(1);
    private static string _playlistSavePath;

    /// <summary>
    /// 当前可用的播放列表集合
    /// </summary>
    public static ObservableCollection<Playlist> TotalPlaylists { get; } = [];

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

        TotalPlaylists.Clear();
        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(PlaylistSavePath);
        foreach (StorageFile item in (await folder.GetFilesAsync())
            .Where(file => file.Name.EndsWith(PlaylistFileExtension, StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                using Stream utf8Json = await item.OpenStreamForReadAsync();
                Playlist playlist = await JsonSerializer.DeserializeAsync<Playlist>(utf8Json);
                TotalPlaylists.Add(playlist);
            }
            catch (JsonException)
            {
                // Just a bad file, ignore it.
            }
        }

        foreach (Playlist playlist in TotalPlaylists)
        {
            foreach (Playlist itemNoDuplicate in TotalPlaylists.Where(item => item != playlist))
            {
                if (playlist.Title == itemNoDuplicate.Title)
                {
                    playlist.Title = $"{playlist.Title} - {"DuplicateFile".GetLocalized()}";
                    await SavePlaylistAsync(playlist);
                }
            }
        }
    }

    /// <summary>
    /// 创建新的播放列表
    /// </summary>
    /// <param name="title">播放列表标题</param>
    /// <param name="description">播放列表描述</param>
    /// <exception cref="ArgumentException"><paramref name="title"/> 为 null 或空白。</exception>
    /// <exception cref="InvalidOperationException">已经包含了一个名称相同的播放列表。</exception>
    public static async Task CreateNewPlaylistAsync(string title, string description)
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

        await SavePlaylistAsync(playlist);
        TotalPlaylists.Add(playlist);
    }

    /// <summary>
    /// 移除指定的播放列表
    /// </summary>
    /// <param name="playlist">一个 <see cref="Playlist"/> 实例</param>
    public static async Task RemovePlaylistAsync(Playlist playlist)
    {
        if (playlist is not null)
        {
            TotalPlaylists.Remove(playlist);
            await RemovePlaylistFile(playlist, StorageDeleteOption.Default);
        }
    }

    /// <summary>
    /// 将播放列表保存为文件
    /// </summary>
    /// <param name="playlist">播放列表实例</param>
    /// <param name="formerTitle">播放列表实例的先前标题</param>
    public static async Task SavePlaylistAsync(Playlist playlist, string formerSaveName = null)
    {
        await playlistFileSemaphore.WaitAsync();

        try
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(PlaylistSavePath);

            if (formerSaveName is not null)
            {
                string formerPlaylistFileName = GetPlaylistFileName(formerSaveName);
                if (await folder.FileExistsAsync(formerPlaylistFileName))
                {
                    StorageFile formerFile = await folder.GetFileAsync(formerPlaylistFileName);
                    try
                    {
                        using Stream formerFileStream = await formerFile.OpenStreamForReadAsync();
                        Playlist formerPlaylist = await JsonSerializer.DeserializeAsync<Playlist>(formerFileStream);

                        if (formerPlaylist.PlaylistId == playlist.PlaylistId)
                        {
                            await formerFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }
                    }
                    catch (JsonException)
                    {
                        // ;-)
                    }
                }
            }

            string playlistSaveName = GetPlaylistFileName(playlist);

            CreationCollisionOption option;
            if (await folder.FileExistsAsync(playlistSaveName))
            {
                StorageFile duplicateFile = await folder.GetFileAsync(playlistSaveName);
                using Stream duplicateFileStream = await duplicateFile.OpenStreamForReadAsync();

                try
                {
                    Playlist mayDuplicatePlaylist = await JsonSerializer.DeserializeAsync<Playlist>(duplicateFileStream);
                    option = mayDuplicatePlaylist.PlaylistId == playlist.PlaylistId
                        ? CreationCollisionOption.OpenIfExists
                        : CreationCollisionOption.GenerateUniqueName;
                }
                catch (JsonException)
                {
                    option = CreationCollisionOption.OpenIfExists;
                }
            }
            else
            {
                option = CreationCollisionOption.OpenIfExists;
            }

            StorageFile playlistFile = await folder.CreateFileAsync(playlistSaveName, option);
            playlist.PlaylistSaveName = playlistFile.DisplayName;
            using Stream stream = await playlistFile.OpenStreamForWriteAsync();
            stream.SetLength(0);

            await JsonSerializer.SerializeAsync(stream, playlist, CommonValues.DefaultJsonSerializerOptions);
        }
        finally
        {
            playlistFileSemaphore.Release();
        }
    }

    /// <summary>
    /// 更改播放列表信息
    /// </summary>
    /// <param name="playlist">播放列表实例</param>
    /// <param name="newTitle">播放列表的新标题</param>
    /// <param name="newDescription">播放列表的新描述</param>
    public static async Task ModifyPlaylistAsync(Playlist playlist, string newTitle, string newDescription)
    {
        bool isModifyTitle = false;
        string formerSaveName = null;
        if (newTitle is not null)
        {
            formerSaveName = playlist.PlaylistSaveName;
            playlist.Title = newTitle;
            playlist.PlaylistSaveName = CommonValues.ReplaceInvaildFileNameChars(newTitle);
            isModifyTitle = true;
        }

        if (newDescription is not null)
        {
            playlist.Description = newDescription;
        }

        if (isModifyTitle)
        {
            await SavePlaylistAsync(playlist, formerSaveName);
        }
    }

    /// <summary>
    /// 播放指定的播放列表
    /// </summary>
    /// <param name="playlist">要播放的播放列表</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task PlayForPlaylistAsync(Playlist playlist)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(playlist, box);
        await MusicService.ReplaceMusic(items);
        box.Unbox();
    }

    /// <summary>
    /// 播放指定的播放列表序列
    /// </summary>
    /// <param name="playlists">要播放的播放列表序列</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlists"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task PlayForPlaylistsAsync(IEnumerable<Playlist> playlists)
    {
        if (playlists is null)
        {
            throw new ArgumentNullException(nameof(playlists));
        }

        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(playlists, box);
        await MusicService.ReplaceMusic(items);
        box.Unbox();
    }

    /// <summary>
    /// 将指定的播放列表添加到正在播放列表中
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task AddPlaylistToNowPlayingAsync(Playlist playlist)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        ExceptionBox box = new();
        IAsyncEnumerable<MediaPlaybackItem> items = CommonValues.GetMediaPlaybackItems(playlist, box);
        await MusicService.AddMusic(items);
        box.Unbox();
    }

    /// <summary>
    /// 向指定的播放列表添加歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="songDetail">表示歌曲详细信息的 <see cref="SongDetail"/> 实例</param>
    /// <param name="albumDetail">表示歌曲所属专辑详细信息的 <see cref="AlbumDetail"/> 实例</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException"><paramref name="songDetail"/> 中所属专辑的 CID 和 <paramref name="albumDetail"/> 中的 CID 不符。</exception>
    /// <exception cref="HttpRequestException">由于网络问题，操作失败</exception>
    public static async Task AddItemForPlaylistAsync(Playlist playlist, SongDetail songDetail, AlbumDetail albumDetail)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (songDetail.AlbumCid != albumDetail.Cid)
        {
            throw new ArgumentException("歌曲信息中所属专辑的 CID 和专辑信息中的 CID 不符。");
        }

        TimeSpan? duration = await MsrModelsHelper.GetSongDurationAsync(songDetail);
        PlaylistItem item = new(songDetail.Cid, albumDetail.Cid, songDetail.Name, duration ?? TimeSpan.Zero);
        await AddItemForPlaylistAsync(playlist, item);
    }

    /// <summary>
    /// 向指定的播放列表添加歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="item">一个 <see cref="PlaylistItem"/> 实例</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    public static async Task AddItemForPlaylistAsync(Playlist playlist, PlaylistItem item)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (playlist.Items.Contains(item))
        {
            return;
        }

        await UIThreadHelper.RunOnUIThread(() =>
        {
            playlist.Items.Add(item);
        });
    }

    /// <summary>
    /// 向指定的播放列表添加另一个播放列表内的歌曲
    /// </summary>
    /// <param name="target">要添加歌曲的播放列表</param>
    /// <param name="source">提供歌曲的播放列表</param>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> 或 <paramref name="source"/> 为 <see langword="null"/>。</exception>
    public static async Task AddItemForPlaylistAsync(Playlist target, Playlist source)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        await AddItemsForPlaylistAsync(target, source.Items);
    }

    /// <summary>
    /// 向指定的播放列表添加一组歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="items">包含 <see cref="PlaylistItem"/> 项的集合</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 或 <paramref name="items"/> 为 <see langword="null"/>。</exception>
    public static async Task AddItemsForPlaylistAsync(Playlist playlist, IEnumerable<PlaylistItem> items)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (!items.Any())
        {
            return;
        }

        try
        {
            playlist.BlockInfoUpdate();
            await UIThreadHelper.RunOnUIThread(() =>
            {
                foreach (PlaylistItem item in items)
                {
                    if (playlist.Items.Contains(item))
                    {
                        continue;
                    }

                    playlist.Items.Add(item);
                }
            });
        }
        finally
        {
            await playlist.RestoreInfoUpdateAsync();
        }
    }

    /// <summary>
    /// 向指定的播放列表添加一组歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="tuples">包含 <see cref="SongDetail"/> 和 <see cref="AlbumDetail"/> 元组的集合</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 或 <paramref name="tuples"/> 为 <see langword="null"/>。</exception>
    public static async Task AddItemsForPlaylistAsync(Playlist playlist, IEnumerable<ValueTuple<SongDetail, AlbumDetail>> tuples)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (tuples is null)
        {
            throw new ArgumentNullException(nameof(tuples));
        }

        List<PlaylistItem> items = new(tuples.Count());

        foreach ((SongDetail songDetail, AlbumDetail albumDetail) in tuples)
        {
            if (songDetail.AlbumCid != albumDetail.Cid)
            {
                throw new ArgumentException($"歌曲信息中，{songDetail.Name} 所属专辑的 CID 和专辑信息 {albumDetail.Name} 中的 CID 不符。");
            }

            TimeSpan? duration = await MsrModelsHelper.GetSongDurationAsync(songDetail);
            PlaylistItem item = new(songDetail.Cid, albumDetail.Cid, songDetail.Name, duration ?? TimeSpan.Zero);
            items.Add(item);
        }

        await AddItemsForPlaylistAsync(playlist, items);
    }

    /// <summary>
    /// 向指定的播放列表添加一组歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="items">包含 <see cref="PlaylistItem"/> 项的集合</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 或 <paramref name="items"/> 为 <see langword="null"/>。</exception>
    public static async Task AddItemsForPlaylistAsync(Playlist playlist, IAsyncEnumerable<PlaylistItem> items)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        try
        {
            playlist.BlockInfoUpdate();
            await UIThreadHelper.RunOnUIThread(async () =>
            {
                await foreach (PlaylistItem item in items)
                {
                    if (playlist.Items.Contains(item))
                    {
                        continue;
                    }

                    playlist.Items.Add(item);
                }
            });
        }
        finally
        {
            await playlist.RestoreInfoUpdateAsync();
        }
    }

    /// <summary>
    /// 向指定的播放列表添加一组歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="tuples">包含 <see cref="SongDetail"/> 和 <see cref="AlbumDetail"/> 元组的集合</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 或 <paramref name="tuples"/> 为 <see langword="null"/>。</exception>
    public static async Task AddItemsForPlaylistAsync(Playlist playlist, IAsyncEnumerable<ValueTuple<SongDetail, AlbumDetail>> tuples)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        if (tuples is null)
        {
            throw new ArgumentNullException(nameof(tuples));
        }

        try
        {
            playlist.BlockInfoUpdate();

            await foreach ((SongDetail songDetail, AlbumDetail albumDetail) in tuples)
            {
                if (songDetail.AlbumCid != albumDetail.Cid)
                {
                    throw new ArgumentException($"歌曲信息中，{songDetail.Name} 所属专辑的 CID 和专辑信息 {albumDetail.Name} 中的 CID 不符。");
                }

                TimeSpan? duration = await MsrModelsHelper.GetSongDurationAsync(songDetail);
                PlaylistItem item = new(songDetail.Cid, albumDetail.Cid, songDetail.Name, duration ?? TimeSpan.Zero);

                await UIThreadHelper.RunOnUIThread(() =>
                {
                    if (!playlist.Items.Contains(item))
                    {
                        playlist.Items.Add(item);
                    }
                });
            }
        }
        finally
        {
            await playlist.RestoreInfoUpdateAsync();
        }
    }

    /// <summary>
    /// 在指定的播放列表中移除歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="item">一个 <see cref="PlaylistItem"/> 实例</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    public static async Task RemoveItemForPlaylistAsync(Playlist playlist, PlaylistItem item)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        await UIThreadHelper.RunOnUIThread(() => playlist.Items.Remove(item));
    }

    /// <summary>
    /// 在指定的播放列表中移除一组歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="items">包含 <see cref="PlaylistItem"/> 的集合</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 或 <paramref name="items"/> 为 <see langword="null"/>。</exception>
    public static async Task RemoveItemsForPlaylist(Playlist playlist, IEnumerable<PlaylistItem> items)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        try
        {
            playlist.BlockInfoUpdate();

            await UIThreadHelper.RunOnUIThread(() =>
            {
                foreach (PlaylistItem item in items)
                {
                    playlist.Items.Remove(item);
                }
            });
        }
        finally
        {
            await playlist.RestoreInfoUpdateAsync();
        }
    }

    /// <summary>
    /// 在指定的播放列表中移除歌曲
    /// </summary>
    /// <param name="playlist">指定的播放列表</param>
    /// <param name="songCid">歌曲 CID</param>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/>。</exception>
    public static void RemoveItemForPlaylist(Playlist playlist, string songCid)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        PlaylistItem targetItem = playlist.Items.FirstOrDefault(item => item.SongCid == songCid);
        playlist.Items.Remove(targetItem);
    }

    private static async Task RemovePlaylistFile(Playlist playlist, StorageDeleteOption deleteOption = StorageDeleteOption.PermanentDelete)
    {
        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(PlaylistSavePath);
        string playlistSaveName = GetPlaylistFileName(playlist);
        if (await folder.FileExistsAsync(playlistSaveName))
        {
            StorageFile formerFile = await folder.GetFileAsync(playlistSaveName);
            await formerFile?.DeleteAsync(deleteOption);
        }
    }

    /// <summary>
    /// 获取播放列表的文件名称
    /// </summary>
    /// <param name="playlist">播放列表实例</param>
    /// <returns>播放列表的文件名称</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playlist"/> 为 <see langword="null"/></exception>
    public static string GetPlaylistFileName(Playlist playlist)
    {
        if (playlist is null)
        {
            throw new ArgumentNullException(nameof(playlist));
        }

        return GetPlaylistFileName(playlist.PlaylistSaveName);
    }

    /// <summary>
    /// 检查播放列表中的项目是否全部有效，并标记播放列表中的无效项
    /// </summary>
    /// <param name="playlist">播放列表实例</param>
    /// <returns>指示播放列表中的项目是否全部有效的值</returns>
    public static async Task<bool> CheckPlaylistAndMarkInvaildItemAsync(Playlist playlist)
    {
        bool isAllVaild = true;

        for (int i = 0; i < playlist.Items.Count; i++)
        {
            PlaylistItem item = playlist.Items[i];

            try
            {
                _ = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
                _ = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
            }
            catch (ArgumentOutOfRangeException)
            {
                await UIThreadHelper.RunOnUIThread(() => playlist.Items[i] = item with { IsCorruptedItem = true });
                isAllVaild = false;
            }
        }

        return isAllVaild;
    }

    private static string GetPlaylistFileName(string title)
    {
        return $"{title}{PlaylistFileExtension}";
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

public sealed class PlaylistServiceExceptionRecipient : IRecipient<Exception>
{
    public void Receive(Exception message)
    {
        throw message;
    }
}