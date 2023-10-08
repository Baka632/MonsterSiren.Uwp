using System.Net.Http;
using MonsterSiren.Api.Models.Song;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

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
        List<MediaPlaybackItem> items = new(CurrentAlbumDetail.Songs.Count());

        try
        {
            if (MusicService.IsPlayerPlaylistHasMusic)
            {
                MusicService.StopMusic();
            }

            foreach (SongInfo item in CurrentAlbumDetail.Songs)
            {
                SongDetail songDetail = await SongService.GetSongDetailedInfo(item.Cid);
                items.Add(songDetail.ToMediaPlaybackItem(CurrentAlbumDetail));
            }

            foreach (MediaPlaybackItem item in items)
            {
                MusicService.AddMusic(item);
            }
        }
        catch (HttpRequestException)
        {
            await DisplayContentDialog("ErrorOccurred".GetLocalized(), "InternetErrorMessage".GetLocalized(), closeButtonText: "Close".GetLocalized());
        }
        finally
        {
            items.Clear();
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