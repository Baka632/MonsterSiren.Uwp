using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 提供视图模型
/// </summary>
public partial class AlbumDetailViewModel(AlbumDetailPage view) : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private AlbumInfo _currentAlbumInfo;
    [ObservableProperty]
    private AlbumDetail _currentAlbumDetail;
    [ObservableProperty]
    private bool isSongsEmpty;
    [ObservableProperty]
    private SongInfo selectedSongInfo;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public async Task Initialize(AlbumInfo albumInfo)
    {
        IsLoading = true;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
        CurrentAlbumInfo = albumInfo;
        AlbumDetail albumDetail;

        try
        {
            albumDetail = await MsrModelsHelper.GetAlbumDetailAsync(albumInfo.Cid);

            CurrentAlbumDetail = albumDetail;
            ErrorVisibility = Visibility.Collapsed;

            IsSongsEmpty = CurrentAlbumDetail.Songs.Any() != true;
        }
        catch (HttpRequestException ex)
        {
            ErrorVisibility = Visibility.Visible;
            ErrorInfo = new ErrorInfo()
            {
                Title = "ErrorOccurred".GetLocalized(),
                Message = "InternetErrorMessage".GetLocalized(),
                Exception = ex
            };
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PlayForCurrentAlbumDetail()
    {
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            await Task.Run(async () =>
            {
                List<MediaPlaybackItem> items = new(CurrentAlbumDetail.Songs.Count());

                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    items.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
                }

                if (items.Count != 0)
                {
                    MusicService.ReplaceMusic(items);
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
                }
            });
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToNowPlayingForCurrentAlbumDetail()
    {
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        bool shouldSendUpdateMediaMessage = MusicService.IsPlayerPlaylistHasMusic != true;
        if (shouldSendUpdateMediaMessage)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);
        }

        try
        {
            await Task.Run(async () =>
            {
                List<MediaPlaybackItem> playbackItems = new(CurrentAlbumDetail.Songs.Count());
                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    playbackItems.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
                }

                MusicService.AddMusic(playbackItems);
            });
        }
        catch (HttpRequestException)
        {
            if (shouldSendUpdateMediaMessage)
            {
                WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            }
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistForCurrentAlbumDetail(Playlist playlist)
    {
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        try
        {
            await Task.Run(async () =>
            {
                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, CurrentAlbumDetail);
                }
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task DownloadForCurrentAlbumDetail()
    {
        if (CurrentAlbumDetail.Songs is null)
        {
            return;
        }

        try
        {
            await Task.Run(async () =>
            {
                foreach (SongInfo item in CurrentAlbumDetail.Songs)
                {
                    SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(item.Cid).ConfigureAwait(false);
                    _ = DownloadService.DownloadSong(CurrentAlbumDetail, songDetail);
                }
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task PlayForSongInfo(SongInfo songInfo)
    {
        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                MusicService.ReplaceMusic(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
            });
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToNowPlayingForSongInfo(SongInfo songInfo)
    {
        bool shouldSendUpdateMediaMessage = MusicService.IsPlayerPlaylistHasMusic != true;
        if (shouldSendUpdateMediaMessage)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);
        }

        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                MusicService.AddMusic(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
            });
        }
        catch (HttpRequestException)
        {
            if (shouldSendUpdateMediaMessage)
            {
                WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            }
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistForSongInfo(Playlist playlist)
    {
        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(SelectedSongInfo.Cid).ConfigureAwait(false);
                await PlaylistService.AddItemForPlaylistAsync(playlist, songDetail, CurrentAlbumDetail);
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task DownloadForSongInfo(SongInfo songInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                _ = DownloadService.DownloadSong(CurrentAlbumDetail, songDetail);
            });
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private static void CopySongNameToClipboard(SongInfo songInfo)
    {
        DataPackage package = new()
        {
            RequestedOperation = DataPackageOperation.Copy
        };
        package.SetText(songInfo.Name);
        Clipboard.SetContent(package);
    }

    [RelayCommand]
    private void StartMultipleSelection()
    {
        // Single 模式只能选一个
        ItemIndexRange range = view.SongList.SelectedRanges.FirstOrDefault();

        view.SongList.SelectionMode = ListViewSelectionMode.Multiple;

        if (range is not null)
        {
            view.SongList.SelectRange(range);
        }

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
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentAlbumDetail.Songs.Count()));
    }

    [RelayCommand]
    private async Task PlayForListViewSelectedItem()
    {
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
                if (item is SongInfo songInfo)
                {
                    await Task.Run(async () =>
                    {
                        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                        songs.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
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
    private async Task AddToNowPlayingForListViewSelectedItem()
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        bool shouldSendUpdateMediaMessage = MusicService.IsPlayerPlaylistHasMusic != true;
        if (shouldSendUpdateMediaMessage)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);
        }

        try
        {
            List<MediaPlaybackItem> songs = new(selectedItems.Count);

            foreach (object item in selectedItems)
            {
                if (item is SongInfo songInfo)
                {
                    await Task.Run(async () =>
                    {
                        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                        songs.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
                    });
                }
            }

            await Task.Run(() => MusicService.AddMusic(songs));

            StopMultipleSelection();
        }
        catch (HttpRequestException)
        {
            if (shouldSendUpdateMediaMessage)
            {
                WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            }
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistForListViewSelectedItem(Playlist playlist)
    {
        IList<object> selectedItems = view.SongList.SelectedItems;

        if (selectedItems.Count == 0)
        {
            return;
        }

        try
        {
            List<ValueTuple<SongDetail, AlbumDetail>> details = [];
            foreach (object item in selectedItems)
            {
                if (item is SongInfo songInfo)
                {
                    await Task.Run(async () =>
                    {
                        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                        details.Add((songDetail, CurrentAlbumDetail));
                    });
                }
            }

            await PlaylistService.AddItemsForPlaylistAsync(playlist, details);
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task DownloadForListViewSelectedItem()
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
                if (item is SongInfo songInfo)
                {
                    await Task.Run(async () =>
                    {
                        SongDetail songDetail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid).ConfigureAwait(false);
                        _ = DownloadService.DownloadSong(CurrentAlbumDetail, songDetail);
                    });
                }
            }

            StopMultipleSelection();
        }
        catch (HttpRequestException)
        {
            await CommonValues.DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }
}