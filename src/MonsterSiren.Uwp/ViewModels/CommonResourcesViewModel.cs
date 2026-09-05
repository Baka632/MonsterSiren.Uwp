using System.Windows.Input;
using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Playlists;
using Windows.ApplicationModel.DataTransfer;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class CommonResourcesViewModel
{
    public static readonly CommonResourcesViewModel Shared = new();

    public ICommand PlayCommand { get; } = new AsyncRelayCommand<CommandParameter>(Play);
    public ICommand PlayNextCommand { get; } = new AsyncRelayCommand<CommandParameter>(PlayNext);
    public ICommand AddToNowPlayingCommand { get; } = new AsyncRelayCommand<CommandParameter>(AddToNowPlaying);
    public ICommand StartDownloadCommand { get; } = new AsyncRelayCommand<CommandParameter>(StartDownload);
    public ICommand AddItemToPlaylistCommand { get; } = new AsyncRelayCommand<CommandParameter>(AddItemToPlaylist);
    public ICommand RemoveItemFromPlaylistCommand { get; } = new AsyncRelayCommand<CommandParameter>(RemoveItemFromPlaylist);
    public ICommand AddItemToFavoriteCommand { get; } = new AsyncRelayCommand<CommandParameter>(AddItemToFavorite);
    public ICommand RemoveItemFromFavoriteCommand { get; } = new AsyncRelayCommand<CommandParameter>(RemoveItemFromFavorite);
    public ICommand CopyItemNameToClipboardCommand { get; } = new RelayCommand<CommandParameter>(CopyItemNameToClipboard);
    public ICommand ModifyPlaylistCommand { get; } = new AsyncRelayCommand<CommandParameter>(ModifyPlaylist);
    public ICommand RemovePlaylistCommand { get; } = new AsyncRelayCommand<CommandParameter>(RemovePlaylist);

    private static async Task Play(CommandParameter parameter)
    {
        bool result = TryGetPlaylistSequence(parameter.Parameter, out IEnumerable<Playlist> playlists)
            ? await CommonValues.StartPlay(playlists)
            : await CommonValues.StartPlay(CommonValues.GetSongCidProvider(parameter.Parameter));
        parameter.Callback?.Execute(result);
    }

    private static async Task PlayNext(CommandParameter parameter)
    {
        bool result = TryGetPlaylistSequence(parameter.Parameter, out IEnumerable<Playlist> playlists)
            ? await CommonValues.PlayNext(playlists)
            : await CommonValues.PlayNext(CommonValues.GetSongCidProvider(parameter.Parameter));
        parameter.Callback?.Execute(result);
    }

    private static async Task AddToNowPlaying(CommandParameter parameter)
    {
        bool result = TryGetPlaylistSequence(parameter.Parameter, out IEnumerable<Playlist> playlists)
            ? await CommonValues.AddToNowPlaying(playlists)
            : await CommonValues.AddToNowPlaying(CommonValues.GetSongCidProvider(parameter.Parameter));
        parameter.Callback?.Execute(result);
    }

    private static async Task StartDownload(CommandParameter parameter)
    {
        ISongCidProvider provider = CommonValues.GetSongCidProvider(parameter.Parameter);
        bool result = await CommonValues.StartDownload(provider);
        parameter.Callback?.Execute(result);
    }

    private static async Task AddItemToPlaylist(CommandParameter parameter)
    {
        (Playlist playlist, object data) = (ValueTuple<Playlist, object>)parameter.Parameter;

        bool result;
        if (TryGetPlaylistSequence(data, out IEnumerable<Playlist> playlists))
        {
            await PlaylistService.AddItemsForPlaylistAsync(playlist, playlists);
            result = true;
        }
        else
        {
            result = await CommonValues.AddToPlaylist(playlist, CommonValues.GetSongCidProvider(data));
        }

        parameter.Callback?.Execute(result);
    }

    private static async Task RemoveItemFromPlaylist(CommandParameter parameter)
    {
        (Playlist playlist, object data) = (ValueTuple<Playlist, object>)parameter.Parameter;

        ISongCidProvider provider = CommonValues.GetSongCidProvider(data);
        bool result = await CommonValues.RemoveFromPlaylist(playlist, provider);
        parameter.Callback?.Execute(result);
    }

    private static async Task AddItemToFavorite(CommandParameter parameter)
    {
        IFavoriteAddable favoriteAddable = CommonValues.GetFavoriteAddable(parameter.Parameter);
        bool result = await CommonValues.AddToFavorite(favoriteAddable);
        parameter.Callback?.Execute(result);
    }

    private static async Task RemoveItemFromFavorite(CommandParameter parameter)
    {
        IFavoriteAddable favoriteAddable = CommonValues.GetFavoriteAddable(parameter.Parameter);
        bool result = await CommonValues.RemoveFromFavorite(favoriteAddable);
        parameter.Callback?.Execute(result);
    }

    private static void CopyItemNameToClipboard(CommandParameter parameter)
    {
        INameProvider nameProvider = CommonValues.GetNameProvider(parameter.Parameter);

        DataPackage package = new()
        {
            RequestedOperation = DataPackageOperation.Copy
        };
        package.SetText(nameProvider.Name);
        Clipboard.SetContent(package);

        parameter.Callback?.Execute(true);
    }

    private static async Task ModifyPlaylist(CommandParameter parameter)
    {
        Playlist playlist = (Playlist)parameter.Parameter;
        bool result = await CommonValues.ShowModifyPlaylistDialog(playlist);
        parameter.Callback?.Execute(result);
    }

    private static async Task RemovePlaylist(CommandParameter parameter)
    {
        bool result = TryGetPlaylistSequence(parameter.Parameter, out IEnumerable<Playlist> playlists)
            ? await CommonValues.RemovePlaylists(playlists)
            : await CommonValues.RemovePlaylist((Playlist)parameter.Parameter);
        parameter.Callback?.Execute(result);
    }

    private static bool TryGetPlaylistSequence(object obj, out IEnumerable<Playlist> playlists)
    {
        if (obj is Func<IEnumerable<Playlist>> factory)
        {
            playlists = factory();
            return true;
        }
        else
        {
            playlists = null;
            return false;
        }
    }
}
