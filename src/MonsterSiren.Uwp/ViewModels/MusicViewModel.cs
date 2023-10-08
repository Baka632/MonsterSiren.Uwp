using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;

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
}