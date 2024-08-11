using System.Net.Http;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private MsrIncrementalCollection<NewsInfo> _newsList;
    [ObservableProperty]
    private MsrIncrementalCollection<AlbumInfo> _albumList;
    [ObservableProperty]
    private bool isAlbumListEmpty;
    [ObservableProperty]
    private bool isNewsListEmpty;

    partial void OnAlbumListChanged(MsrIncrementalCollection<AlbumInfo> value)
    {
        IsAlbumListEmpty = !value.Any();
    }

    partial void OnNewsListChanged(MsrIncrementalCollection<NewsInfo> value)
    {
        IsNewsListEmpty = !value.Any();
    }

    public async Task Initialize(string keyword)
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;

        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return;
            }

            SearchAlbumAndNewsResult albumAndNewsWarpper = await SearchService.SearchAlbumAndNewsAsync(keyword);

            List<AlbumInfo> albumList = albumAndNewsWarpper.Albums.List.ToList();

            if (await TryFillArtistAndReplaceCachedAlbumCover(albumList))
            {
                albumAndNewsWarpper = albumAndNewsWarpper with
                {
                    Albums = albumAndNewsWarpper.Albums with { List = albumList }
                };
            }

            NewsList = new MsrIncrementalCollection<NewsInfo>(albumAndNewsWarpper.News, async lastInfo => await SearchService.SearchNewsAsync(keyword, lastInfo.Cid));
            AlbumList = new MsrIncrementalCollection<AlbumInfo>(albumAndNewsWarpper.Albums, async lastInfo =>
            {
                ListPackage<AlbumInfo> listPackage = await SearchService.SearchAlbumAsync(keyword, lastInfo.Cid);
                List<AlbumInfo> list = listPackage.List.ToList();

                return await TryFillArtistAndReplaceCachedAlbumCover(list)
                ? listPackage with { List = list }
                : listPackage;
            });

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

        static async Task<bool> TryFillArtistAndReplaceCachedAlbumCover(List<AlbumInfo> albumList)
        {
            bool isModify = false;

            for (int i = 0; i < albumList.Count; i++)
            {
                if (albumList[i].Artistes is null || albumList[i].Artistes.Any() != true)
                {
                    albumList[i] = albumList[i] with { Artistes = ["MSR".GetLocalized()] };
                    isModify = true;
                }

                Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(albumList[i]);
                if (fileCoverUri != null)
                {
                    albumList[i] = albumList[i] with { CoverUrl = fileCoverUri.ToString() };
                    isModify = true;
                }
            }

            return isModify;
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
}