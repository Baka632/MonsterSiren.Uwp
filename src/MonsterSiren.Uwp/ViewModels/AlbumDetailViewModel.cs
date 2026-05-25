using System.Net.Http;
using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 提供视图模型。
/// </summary>
public partial class AlbumDetailViewModel : ObservableObject
{
    private readonly AlbumDetailPage view;

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
    [NotifyPropertyChangedFor(nameof(IsSelectedSongInfoContainsInFavorite))]
    private SongInfo selectedSongInfo;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    public Func<ISongCidProvider> SongCidProviderFactory { get; }
    public bool IsSelectedSongInfoContainsInFavorite { get => FavoriteService.ContainsSong(SelectedSongInfo); }

    public AlbumDetailViewModel(AlbumDetailPage albumDetailPage)
    {
        view = albumDetailPage;
        SongCidProviderFactory = GetSongCidProvider;
    }

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

    public async Task Initialize(AlbumDetail albumDetail)
    {
        IsLoading = true;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;

        try
        {
            // 先用比较准确的，计算出来的 AlbumInfo（不那么准确的地方在专辑艺术家这里，笨笨 yj 的锅）。
            // 如果这里不先顶上，那么会出现异常，
            // 因为查询准确的 AlbumInfo 是异步操作，因此 UI 线程在查询过程中会先去处理 UI 的其他事情，
            // 而由于 AlbumInfo 的内容为空，视图方面相关操作会出现问题。
            CurrentAlbumInfo = new(albumDetail.Cid,
                                   albumDetail.Name,
                                   albumDetail.Intro,
                                   albumDetail.Belong,
                                   albumDetail.CoverUrl,
                                   albumDetail.CoverDeUrl,
                                   [.. albumDetail.Songs.SelectMany(info => info.Artists).Distinct()]);
            
            CurrentAlbumDetail = albumDetail;
            IsSongsEmpty = CurrentAlbumDetail.Songs.Any() != true;

            // 之后再去查完全准确的 AlbumInfo
            CurrentAlbumInfo = (await CommonValues.GetOrFetchAlbums()).CollectionSource
                .Single(info => info.Cid == albumDetail.Cid);

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

    public async Task Initialize(AlbumFavoriteItem favoriteItem)
    {
        IsLoading = true;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;

        try
        {
            CurrentAlbumInfo = new(favoriteItem.AlbumCid,
                                   favoriteItem.AlbumName,
                                   string.Empty,
                                   string.Empty,
                                   string.Empty,
                                   string.Empty,
                                   favoriteItem.Artistes);
            
            CurrentAlbumDetail = await MsrModelsHelper.GetAlbumDetailAsync(favoriteItem.AlbumCid);
            IsSongsEmpty = CurrentAlbumDetail.Songs.Any() != true;

            // 之后再去查完全准确的 AlbumInfo
            CurrentAlbumInfo = (await CommonValues.GetOrFetchAlbums()).CollectionSource
                .Single(info => info.Cid == favoriteItem.AlbumCid);

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
    private async Task PlayForCurrentAlbumDetail()
    {
        await CommonValues.StartPlay(CurrentAlbumDetail.ToAdapter());
    }

    [RelayCommand]
    private async Task DownloadForCurrentAlbumDetail()
    {
        await CommonValues.StartDownload(CurrentAlbumDetail.ToAdapter());
    }

    [RelayCommand]
    private void NotifyIsSelectedSongInfoContainsInFavoriteChanged()
        => OnPropertyChanged(nameof(IsSelectedSongInfoContainsInFavorite));

    [RelayCommand]
    private void StartMultipleSelection()
    {
        ListView songList = view.SongList;
        songList.SelectionMode = ListViewSelectionMode.Multiple;
        songList.SelectedItem = SelectedSongInfo;

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
        view.SongList.SelectRange(new ItemIndexRange(0, (uint)CurrentAlbumDetail.Songs.Count()));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentAlbumDetail.Songs.Count()));
    }

    private List<SongInfo> GetSelectedItems()
    {
        ListView listView = view.SongList;
        List <SongInfo> selectedItems = new(5);

        foreach (ItemIndexRange range in listView.SelectedRanges)
        {
            selectedItems.AddRange(CurrentAlbumDetail.Songs.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }

    private SongInfoSequenceAdapter GetSongCidProvider() => GetSelectedItems().ToAdapter();
}