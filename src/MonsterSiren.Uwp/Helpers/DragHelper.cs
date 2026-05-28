using System.Text.Json;
using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
using Windows.ApplicationModel.DataTransfer;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为拖拽操作提供帮助的类。
/// </summary>
public static class DragHelper
{
    public const string MusicAlbumInfosFormatId = "Music_AlbumInfos_DataPackage_FormatId";
    public const string MusicSongInfosFormatId = "Music_SongInfos_DataPackage_FormatId";
    public const string MusicPlaylistsFormatId = "Music_Playlists_DataPackage_FormatId";
    public const string MusicPlaylistItemsFormatId = "Music_PlaylistItems_DataPackage_FormatId";
    public const string MusicSongFavoriteItemsFormatId = "Music_SongFavoriteItems_DataPackage_FormatId";
    public const string MusicAlbumFavoriteItemsFormatId = "Music_AlbumFavoriteItems_DataPackage_FormatId";

    /// <summary>
    /// 检查指定的包格式 ID 是否可被接受。
    /// </summary>
    /// <param name="contentFormatId">指定的包格式 ID。</param>
    /// <returns>指示包格式 ID 是否可被接受的值。</returns>
    public static bool CanAcceptPackage(string contentFormatId)
    {
        return contentFormatId switch
        {
            MusicSongInfosFormatId
            or MusicAlbumInfosFormatId
            or MusicPlaylistsFormatId
            or MusicPlaylistItemsFormatId
            or MusicSongFavoriteItemsFormatId
            or MusicAlbumFavoriteItemsFormatId=> true,
            _ => false
        };
    }

    /// <summary>
    /// 处理传递的 <see cref="DragEventArgs"/> 实例。
    /// </summary>
    /// <param name="args">根据拖拽内容可否接受而进行相应配置的 <see cref="DragEventArgs"/> 实例。</param>
    /// <param name="acceptedDragCaption">当拖拽内容可接受时显示的提示字符串。</param>
    public static void HandleDragEventArgs(DragEventArgs args, string acceptedDragCaption)
    {
        if (args is null)
        {
            return;
        }

        DataPackageView dataView = args.DataView;
        if (dataView.AvailableFormats.Any(CanAcceptPackage))
        {
            args.AcceptedOperation = DataPackageOperation.Link;
            args.DragUIOverride.Caption = acceptedDragCaption;
        }
        else
        {
            args.AcceptedOperation = DataPackageOperation.None;
        }
    }

    /// <summary>
    /// 处理传入数据包的数据，并对获得的 <see cref="ISongCidProvider"/> 进行“添加到正在播放”的操作。
    /// </summary>
    /// <param name="dataView">传入的数据包。</param>
    public static async Task HandleDataAndPlayNextAsync(DataPackageView dataView)
    {
        await HandleDataCore(dataView,
                       async playlists => await CommonValues.AddToNowPlaying(playlists),
                       async provider => await CommonValues.AddToNowPlaying(provider));
    }

    /// <summary>
    /// 处理传入数据包的数据，并对获得的 <see cref="ISongCidProvider"/> 进行“添加到播放列表”的操作。
    /// </summary>
    /// <param name="dataView">传入的数据包。</param>
    /// <param name="playlist">目标播放列表。</param>
    public static async Task HandleDataAndAddToPlaylistAsync(DataPackageView dataView, Playlist playlist)
    {
        await HandleDataCore(dataView,
                       async playlists => await PlaylistService.AddItemsForPlaylistAsync(playlist, playlists),
                       async provider => await CommonValues.AddToPlaylist(playlist, provider));
    }

    private static async Task HandleDataCore(DataPackageView dataView, Func<IEnumerable<Playlist>, Task> doOperationForPlaylist, Func<ISongCidProvider, Task> doOperationForISongCidProvider)
    {
        bool playlistsSuccess = await TryGetPlaylistsAndDoOperation(dataView, doOperationForPlaylist);

        if (!playlistsSuccess)
        {
            ISongCidProvider provider = await GetSongCidProvider(dataView);

            if (provider != null)
            {
                await doOperationForISongCidProvider?.Invoke(provider);
            }
        }
    }

    /// <summary>
    /// 为 <see cref="DragItemsStartingEventArgs"/> 写入用于拖动操作的数据。
    /// </summary>
    /// <typeparam name="T">拖动内容的集合内容类型。</typeparam>
    /// <param name="args">一个 <see cref="DragItemsStartingEventArgs"/> 实例。</param>
    /// <exception cref="NotImplementedException"><see cref="DragItemsStartingEventArgs.Items"/> 中的内容尚不支持。</exception>
    public static void WriteDataToDragItemsStartingEventArgs<T>(DragItemsStartingEventArgs args)
    {
        IEnumerable<T> values = args.Items.Cast<T>();

        if (values is null || !values.Any())
        {
            args.Cancel = true;
            return;
        }

        (string formatId, string json) = values switch
        {
            IEnumerable<SongInfo> songInfos => (MusicSongInfosFormatId, JsonSerializer.Serialize(songInfos)),
            IEnumerable<AlbumInfo> albumInfos => (MusicAlbumInfosFormatId, JsonSerializer.Serialize(albumInfos)),
            IEnumerable<Playlist> playlists => (MusicPlaylistsFormatId, JsonSerializer.Serialize(playlists)),
            IEnumerable<PlaylistItem> playlistItems => (MusicPlaylistItemsFormatId, JsonSerializer.Serialize(playlistItems)),
            IEnumerable<SongFavoriteItem> songFavoriteItems => (MusicSongFavoriteItemsFormatId, JsonSerializer.Serialize(songFavoriteItems)),
            IEnumerable<AlbumFavoriteItem> albumFavoriteItems => (MusicAlbumFavoriteItemsFormatId, JsonSerializer.Serialize(albumFavoriteItems)),
            _ => throw new NotImplementedException("尚未实现这类模型的处理流程。")
        };

        args.Data.SetData(formatId, json);
    }

    private static async Task<ISongCidProvider> GetSongCidProvider(DataPackageView dataView)
    {
        ISongCidProvider provider;

        if (dataView.Contains(MusicAlbumInfosFormatId))
        {
            string json = (string)await dataView.GetDataAsync(MusicAlbumInfosFormatId);
            provider = JsonSerializer.Deserialize<IEnumerable<AlbumInfo>>(json).ToAdapter();
        }
        else if (dataView.Contains(MusicSongInfosFormatId))
        {
            string json = (string)await dataView.GetDataAsync(MusicSongInfosFormatId);
            provider = JsonSerializer.Deserialize<IEnumerable<SongInfo>>(json).ToAdapter();
        }
        else if (dataView.Contains(MusicPlaylistItemsFormatId))
        {
            string json = (string)await dataView.GetDataAsync(MusicPlaylistItemsFormatId);
            provider = JsonSerializer.Deserialize<IEnumerable<PlaylistItem>>(json).ToAdapter();
        }
        else if (dataView.Contains(MusicSongFavoriteItemsFormatId))
        {
            string json = (string)await dataView.GetDataAsync(MusicSongFavoriteItemsFormatId);
            provider = JsonSerializer.Deserialize<IEnumerable<SongFavoriteItem>>(json).ToAdapter();
        }
        else if (dataView.Contains(MusicAlbumFavoriteItemsFormatId))
        {
            string json = (string)await dataView.GetDataAsync(MusicAlbumFavoriteItemsFormatId);
            provider = JsonSerializer.Deserialize<IEnumerable<AlbumFavoriteItem>>(json).ToAdapter();
        }
        else
        {
#if DEBUG
            throw new NotImplementedException("指定的 FormatID 未实现。");
#else
            return null;
#endif
        }

        return provider;
    }

    private static async Task<bool> TryGetPlaylistsAndDoOperation(DataPackageView dataView, Func<IEnumerable<Playlist>, Task> successOperation)
    {
        if (dataView.Contains(MusicPlaylistsFormatId))
        {
            string json = (string)await dataView.GetDataAsync(MusicPlaylistsFormatId);
            IEnumerable<Playlist> playlists = JsonSerializer.Deserialize<IEnumerable<Playlist>>(json);
            await successOperation?.Invoke(playlists);
            return true;
        }
        else
        {
            return false;
        }
    }
}
