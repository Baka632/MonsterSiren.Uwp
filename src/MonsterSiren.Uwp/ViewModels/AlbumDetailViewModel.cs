﻿using System.Net.Http;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 提供视图模型
/// </summary>
public partial class AlbumDetailViewModel : ObservableObject
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

    public async Task Initialize(AlbumInfo albumInfo)
    {
        IsLoading = true;
        CurrentAlbumInfo = albumInfo;
        AlbumDetail albumDetail;

        try
        {
            if (CacheHelper<AlbumDetail>.Default.TryGetData(albumInfo.Cid, out AlbumDetail detail))
            {
                albumDetail = detail;
            }
            else
            {
                await Task.Run(async () =>
                {
                    albumDetail = await AlbumService.GetAlbumDetailedInfo(albumInfo.Cid);

                    bool shouldUpdate = false;
                    foreach (SongInfo item in albumDetail.Songs)
                    {
                        if (item.Artists is null || item.Artists.Any() != true)
                        {
                            shouldUpdate = true;
                            break;
                        }
                    }

                    if (shouldUpdate)
                    {
                        List<SongInfo> songs = albumDetail.Songs.ToList();
                        for (int i = 0; i < songs.Count; i++)
                        {
                            SongInfo songInfo = songs[i];
                            if (songInfo.Artists is null || songInfo.Artists.Any() != true)
                            {
                                songs[i] = songInfo with { Artists = new string[] { "MSR".GetLocalized() } };
                            }
                        }

                        albumDetail = albumDetail with { Songs = songs };
                    }
                });

                CacheHelper<AlbumDetail>.Default.Store(albumInfo.Cid, albumDetail);
            }

            CurrentAlbumDetail = albumDetail;
            ErrorVisibility = Visibility.Collapsed;
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
    public async Task PlayForCurrentAlbumDetail()
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
                    SongDetail songDetail = await GetSongDetail(item).ConfigureAwait(false);
                    items.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
                }

                MusicService.ReplaceMusic(items);
            });
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    public async Task AddToPlaylistForCurrentAlbumDetail()
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
                    SongDetail songDetail = await GetSongDetail(item).ConfigureAwait(false);
                    MusicService.AddMusic(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
                }
            });
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    public async Task PlayForSongInfo(SongInfo songInfo)
    {
        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            await Task.Run(async () =>
            {
                SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                MusicService.ReplaceMusic(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
            });
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    public async Task AddToPlaylistForSongInfo(SongInfo songInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                MusicService.AddMusic(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
            });
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    private static async Task<SongDetail> GetSongDetail(SongInfo songInfo)
    {
        if (CacheHelper<SongDetail>.Default.TryGetData(songInfo.Cid, out SongDetail detail))
        {
            return detail;
        }
        else
        {
            SongDetail songDetail = await SongService.GetSongDetailedInfo(songInfo.Cid);
            CacheHelper<SongDetail>.Default.Store(songInfo.Cid, songDetail);

            return songDetail;
        }
    }

    public static async Task DisplayContentDialog(string title, string message, string primaryButtonText = "", string closeButtonText = "")
    {
        ContentDialog contentDialog = new()
        {
            Title = title,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            CloseButtonText = closeButtonText
        };

        await contentDialog.ShowAsync();
    }
}