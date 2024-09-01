﻿using System.Net.Http;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class PlaylistDetailViewModel(PlaylistDetailPage view) : ObservableObject
{
    [ObservableProperty]
    private Playlist currentPlaylist;
    [ObservableProperty]
    private PlaylistItem selectedItem;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public void Initialize(Playlist model)
    {
        CurrentPlaylist = model ?? throw new ArgumentNullException(nameof(model));
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
    }

    [RelayCommand]
    private async Task PlayForCurrentPlaylist()
    {
        if (CurrentPlaylist.SongCount == 0)
        {
            await CommonValues.DisplayContentDialog("NoSongPlayed_Title".GetLocalized(),
                                                    "NoSongPlayed_PlaylistEmpty".GetLocalized(),
                                                    "OK".GetLocalized());
        }
        else
        {
            await PlaylistService.PlayForPlaylistAsync(CurrentPlaylist);
        }
    }

    [RelayCommand]
    private static async Task PlayForItem(PlaylistItem item)
    {
        try
        {
            SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
            AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);

            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);
            await Task.Run(() => MusicService.ReplaceMusic(songDetail.ToMediaPlaybackItem(albumDetail)));
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private static async Task AddItemToNowPlaying(PlaylistItem item)
    {
        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
        AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);

        await Task.Run(() => MusicService.AddMusic(songDetail.ToMediaPlaybackItem(albumDetail)));
    }

    [RelayCommand]
    private async Task AddCurrentPlaylistToNowPlaying()
    {
        await PlaylistService.AddPlaylistToNowPlayingAsync(CurrentPlaylist);
    }

    [RelayCommand]
    private async Task AddItemToAnotherPlaylist(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, SelectedItem);
    }

    [RelayCommand]
    private async Task AddCurrentPlaylistToAnotherPlaylistCommand(Playlist target)
    {
        await PlaylistService.AddItemForPlaylistAsync(target, CurrentPlaylist);
    }

    [RelayCommand]
    private static async Task DownloadForItem(PlaylistItem item)
    {
        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.SongCid);
        AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
        _ = DownloadService.DownloadSong(albumDetail, songDetail);
    }

    [RelayCommand]
    private async Task DownloadForCurrentPlaylist()
    {
        foreach (PlaylistItem item in CurrentPlaylist)
        {
            await DownloadForItem(item);
        }
    }

    [RelayCommand]
    private void RemoveItemFromPlaylist(PlaylistItem item)
    {
        PlaylistService.RemoveItemForPlaylist(CurrentPlaylist, item);
    }

    [RelayCommand]
    private async Task ModifyPlaylist()
    {
        PlaylistInfoDialog dialog = new()
        {
            Title = "PlaylistModifyTitle".GetLocalized(),
            PrimaryButtonText = "PlaylistModifyPrimaryButtonText".GetLocalized(),
            PlaylistTitle = CurrentPlaylist.Title,
            PlaylistDescription = CurrentPlaylist.Description,
            TargetPlaylist = CurrentPlaylist,
        };

        ContentDialogResult result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.ModifyPlaylistAsync(CurrentPlaylist, dialog.PlaylistTitle, dialog.PlaylistDescription);
        }
    }

    [RelayCommand]
    private async Task RemovePlaylist()
    {
        ContentDialogResult result = await CommonValues.DisplayContentDialog("EnsureDelete".GetLocalized(), "",
            "OK".GetLocalized(), "Cancel".GetLocalized());

        if (result == ContentDialogResult.Primary)
        {
            await PlaylistService.RemovePlaylistAsync(CurrentPlaylist);
            ContentFrameNavigationHelper.GoBack();
        }
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        view.SongList.SelectionMode = ListViewSelectionMode.Multiple;
        SelectedSongListItemContextFlyout = view.SongSelectionFlyout;
    }

    [RelayCommand]
    private void StopMultipleSelection()
    {
        view.SongList.SelectionMode = ListViewSelectionMode.Single;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.SongList.SelectAll();
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentPlaylist.SongCount));
    }

    [RelayCommand]
    private async Task PlaySongListSelectedItem()
    {
        // TODO: 对播放列表这种容易变大的东西来说，用 SelectedRanges 更好一些？
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            List<MediaPlaybackItem> songs = new(selectedItems.Count);
            foreach (object item in selectedItems)
            {
                if (item is PlaylistItem playlistItem)
                {
                    await Task.Run(async () =>
                    {
                        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(playlistItem.SongCid);
                        AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(playlistItem.AlbumCid);
                        songs.Add(songDetail.ToMediaPlaybackItem(albumDetail));
                    });
                }
            }

            await Task.Run(() => MusicService.ReplaceMusic(songs));

            StopMultipleSelection();
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToNowPlaying()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        try
        {
            List<MediaPlaybackItem> songs = new(selectedItems.Count);

            foreach (object item in selectedItems)
            {
                if (item is PlaylistItem playlistItem)
                {
                    await Task.Run(async () =>
                    {
                        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(playlistItem.SongCid);
                        AlbumDetail albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(playlistItem.AlbumCid);
                        songs.Add(songDetail.ToMediaPlaybackItem(albumDetail));
                    });
                }
            }

            await Task.Run(() => MusicService.AddMusic(songs));

            StopMultipleSelection();
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddSongListSelectedItemToAnotherPlaylist(Playlist playlist)
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        try
        {
            foreach (object item in selectedItems)
            {
                if (item is PlaylistItem playlistItem)
                {
                    await PlaylistService.AddItemForPlaylistAsync(playlist, playlistItem);
                }
            }
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task DownloadForSongListSelectedItem()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        try
        {
            foreach (object item in selectedItems)
            {
                if (item is PlaylistItem playlistItem)
                {
                    await DownloadForItem(playlistItem);
                }
            }

            StopMultipleSelection();
        }
        catch (HttpRequestException)
        {
        }
    }

    [RelayCommand]
    private void RemoveSongListSelectedItemFromPlaylist()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        foreach (object item in selectedItems)
        {
            if (item is PlaylistItem playlistItem)
            {
                PlaylistService.RemoveItemForPlaylist(CurrentPlaylist, playlistItem);
            }
        }

        StopMultipleSelection();
    }
}