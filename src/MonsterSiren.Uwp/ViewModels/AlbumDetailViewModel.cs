using System.Net.Http;
using Windows.System;

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
}