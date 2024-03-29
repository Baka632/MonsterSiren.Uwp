﻿using System.Net.Http;
using System.Threading;
using Microsoft.Toolkit.Collections;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MusicPage"/> 提供视图模型
/// </summary>
public sealed partial class MusicViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private bool isRefreshing = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> albums;

    public async Task Initialize()
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;
        try
        {
            if (MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.TryGetData(CommonValues.AlbumInfoCacheKey, out CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> infos))
            {
                Albums = infos;
            }
            else
            {
                IEnumerable<AlbumInfo> albums = await GetAlbumsFromServer();
                Albums = CreateIncrementalLoadingCollection(albums);
                MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.Store(CommonValues.AlbumInfoCacheKey, Albums);
            }

            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException ex)
        {
            ShowInternetError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RefreshAlbums()
    {
        IsRefreshing = true;
        ErrorVisibility = Visibility.Collapsed;
        try
        {
            IEnumerable<AlbumInfo> albumInfos = await GetAlbumsFromServer();

            if (Albums is null || !Albums.CollectionSource.AlbumInfos.SequenceEqual(albumInfos))
            {
                Albums = CreateIncrementalLoadingCollection(albumInfos);
                MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.Store(CommonValues.AlbumInfoCacheKey, Albums);
            }

            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException ex)
        {
            ShowInternetError(ex);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void ShowInternetError(HttpRequestException ex)
    {
        ErrorVisibility = Visibility.Visible;
        ErrorInfo = new ErrorInfo()
        {
            Title = "ErrorOccurred".GetLocalized(),
            Message = "InternetErrorMessage".GetLocalized(),
            Exception = ex
        };
    }

    private static CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo> CreateIncrementalLoadingCollection(IEnumerable<AlbumInfo> albums)
    {
        int loadCount = EnvironmentHelper.IsWindowsMobile ? 5 : 10;
        return new(new AlbumInfoSource(albums), loadCount);
    }

    private async static Task<IEnumerable<AlbumInfo>> GetAlbumsFromServer()
    {
        List<AlbumInfo> albums = await Task.Run(async () =>
        {
            List<AlbumInfo> albumList = (await AlbumService.GetAllAlbumsAsync()).ToList();

            for (int i = 0; i < albumList.Count; i++)
            {
                if (albumList[i].Artistes is null || albumList[i].Artistes.Any() != true)
                {
                    albumList[i] = albumList[i] with { Artistes = ["MSR".GetLocalized()] };
                }

                Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumList[i]);
                if (fileCoverUri != null)
                {
                    albumList[i] = albumList[i] with { CoverUrl = fileCoverUri.ToString() };
                }
            }

            return albumList;
        });

        return albums;
    }

    [RelayCommand]
    private static async Task PlayAlbumForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await GetAlbumDetail(albumInfo).ConfigureAwait(false);
                List<MediaPlaybackItem> playbackItems = new(albumDetail.Songs.Count());

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                    playbackItems.Add(songDetail.ToMediaPlaybackItem(albumDetail));
                }

                if (playbackItems.Count != 0)
                {
                    MusicService.ReplaceMusic(playbackItems);
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
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private static async Task AddToPlaylistForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await GetAlbumDetail(albumInfo).ConfigureAwait(false);
                List<MediaPlaybackItem> playbackItems = new(albumDetail.Songs.Count());

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                    playbackItems.Add(songDetail.ToMediaPlaybackItem(albumDetail));
                }

                MusicService.AddMusic(playbackItems);
            });
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }
    
    [RelayCommand]
    private static async Task DownloadForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            await Task.Run(async () =>
            {
                AlbumDetail albumDetail = await GetAlbumDetail(albumInfo).ConfigureAwait(false);

                foreach (SongInfo songInfo in albumDetail.Songs)
                {
                    SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                    _ = DownloadService.DownloadSong(albumDetail, songDetail);
                }
            });
        }
        catch (HttpRequestException)
        {
        }
    }

    private static async Task<AlbumDetail> GetAlbumDetail(AlbumInfo albumInfo)
    {
        AlbumDetail albumDetail;
        if (MemoryCacheHelper<AlbumDetail>.Default.TryGetData(albumInfo.Cid, out AlbumDetail detail))
        {
            albumDetail = detail;
        }
        else
        {
            albumDetail = await AlbumService.GetAlbumDetailedInfoAsync(albumInfo.Cid);

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

            if (albumDetail.Songs.Any())
            {
                MemoryCacheHelper<AlbumDetail>.Default.Store(albumInfo.Cid, albumDetail);
            }
        }

        return albumDetail;
    }

    private static async Task<SongDetail> GetSongDetail(SongInfo songInfo)
    {
        if (MemoryCacheHelper<SongDetail>.Default.TryGetData(songInfo.Cid, out SongDetail detail))
        {
            return detail;
        }
        else
        {
            SongDetail songDetail = await SongService.GetSongDetailedInfoAsync(songInfo.Cid);
            MemoryCacheHelper<SongDetail>.Default.Store(songInfo.Cid, songDetail);

            return songDetail;
        }
    }

    public static async Task DisplayContentDialog(string title, string message, string primaryButtonText = "", string closeButtonText = "")
    {
        await UIThreadHelper.RunOnUIThread(async () =>
        {
            ContentDialog contentDialog = new()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText
            };

            await contentDialog.ShowAsync();
        });
    }
}

public class AlbumInfoSource(IEnumerable<AlbumInfo> infos) : IIncrementalSource<AlbumInfo>
{
    public IEnumerable<AlbumInfo> AlbumInfos { get; } = new List<AlbumInfo>(infos);

    public async Task<IEnumerable<AlbumInfo>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            return AlbumInfos.Skip(pageIndex * pageSize).Take(pageSize);
        }, cancellationToken);
    }
}