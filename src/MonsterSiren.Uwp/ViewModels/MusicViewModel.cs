using System.Collections.ObjectModel;
using System.Net.Http;
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
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private ObservableCollection<AlbumInfo> albums;

    public async Task Initialize()
    {
        IsLoading = true;
        try
        {
            if (CacheHelper<ObservableCollection<AlbumInfo>>.Default.TryGetData(CommonValues.AlbumInfoCacheKey, out ObservableCollection<AlbumInfo> infos))
            {
                Albums = infos;
            }
            else
            {
                List<AlbumInfo> albums = (await AlbumService.GetAllAlbums()).ToList();

                for (int i = 0; i < albums.Count; i++)
                {
                    if (albums[i].Artistes is null || albums[i].Artistes.Any() != true)
                    {
                        albums[i] = albums[i] with { Artistes = new string[] { "MSR".GetLocalized() } };
                    }
                }

                Albums = new ObservableCollection<AlbumInfo>(albums);

                CacheHelper<ObservableCollection<AlbumInfo>>.Default.Store(CommonValues.AlbumInfoCacheKey, Albums);
            }


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
    private async Task PlayAlbumForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyWillUpdateMediaMessageToken);

            AlbumDetail albumDetail = await GetAlbumDetail(albumInfo).ConfigureAwait(false);
            List<MediaPlaybackItem> playbackItems = new(albumDetail.Songs.Count());

            foreach (SongInfo songInfo in albumDetail.Songs)
            {
                SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                playbackItems.Add(songDetail.ToMediaPlaybackItem(albumDetail));
            }

            MusicService.ReplaceMusic(playbackItems);
        }
        catch (HttpRequestException)
        {
            WeakReferenceMessenger.Default.Send(string.Empty, CommonValues.NotifyUpdateMediaFailMessageToken);
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    [RelayCommand]
    private async Task AddToPlaylistForAlbumInfo(AlbumInfo albumInfo)
    {
        try
        {
            AlbumDetail albumDetail = await GetAlbumDetail(albumInfo).ConfigureAwait(false);

            foreach (SongInfo songInfo in albumDetail.Songs)
            {
                SongDetail songDetail = await GetSongDetail(songInfo).ConfigureAwait(false);
                MusicService.AddMusic(songDetail.ToMediaPlaybackItem(albumDetail));
            }
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
    }

    private static async Task<AlbumDetail> GetAlbumDetail(AlbumInfo albumInfo)
    {
        AlbumDetail albumDetail;
        if (CacheHelper<AlbumDetail>.Default.TryGetData(albumInfo.Cid, out AlbumDetail detail))
        {
            albumDetail = detail;
        }
        else
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

            CacheHelper<AlbumDetail>.Default.Store(albumInfo.Cid, albumDetail);
        }

        return albumDetail;
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