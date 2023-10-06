using System.Net.Http;
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
                albumDetail = await AlbumService.GetAlbumDetailedInfo(albumInfo.Cid);
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