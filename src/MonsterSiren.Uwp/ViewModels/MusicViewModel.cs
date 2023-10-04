using System.Collections.ObjectModel;
using System.Net.Http;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MusicPage"/> 提供视图模型
/// </summary>
public sealed partial class MusicViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading;
    [ObservableProperty]
    private ObservableCollection<AlbumInfo> albums;

    public async void Initialize()
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
                IEnumerable<AlbumInfo> albums = await AlbumService.GetAllAlbums();
                Albums = new ObservableCollection<AlbumInfo>(albums);

                CacheHelper<ObservableCollection<AlbumInfo>>.Default.Store(CommonValues.AlbumInfoCacheKey, Albums);
            }
        }
        catch (HttpRequestException ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorInfo()
            {
                Title = "ErrorOccurred".GetLocalized(),
                Message = "InternetErrorMessage".GetLocalized(),
                Exception = ex
            }, CommonValues.ApplicationErrorMessageToken);
        }
        IsLoading = false;
    }
}