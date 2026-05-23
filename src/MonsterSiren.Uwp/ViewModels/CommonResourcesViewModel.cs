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

    private static async Task Play(CommandParameter parameter)
    {
        ISongCidProvider provider = CommonValues.GetSongCidProvider(parameter.Parameter);
        bool result = await CommonValues.StartPlay(provider);
        parameter.Callback?.Execute(result);
    }

    private static async Task PlayNext(CommandParameter parameter)
    {
        ISongCidProvider provider = CommonValues.GetSongCidProvider(parameter.Parameter);
        bool result = await CommonValues.PlayNext(provider);
        parameter.Callback?.Execute(result);
    }

    private static async Task AddToNowPlaying(CommandParameter parameter)
    {
        ISongCidProvider provider = CommonValues.GetSongCidProvider(parameter.Parameter);
        bool result = await CommonValues.AddToNowPlaying(provider);
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

        ISongCidProvider provider = CommonValues.GetSongCidProvider(data);
        bool result = await CommonValues.AddToPlaylist(playlist, provider);
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
}
