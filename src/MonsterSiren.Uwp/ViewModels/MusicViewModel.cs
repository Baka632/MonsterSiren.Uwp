using System.Net.Http;
using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MusicPage"/> 提供视图模型。
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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedAlbumInfoContainsInFavorite))]
    private AlbumInfo selectedAlbumInfo;
    [ObservableProperty]
    private FlyoutBase selectedAlbumInfoContextFlyout;

    private readonly MusicPage view;
    private SelectionHelper selectionHelper;

    public bool IsSelectedAlbumInfoContainsInFavorite { get => FavoriteService.ContainsAlbum(SelectedAlbumInfo); }
    public Func<ISongCidProvider> SongCidProviderFactory { get; }

    public MusicViewModel(MusicPage musicPage)
    {
        SongCidProviderFactory = GetSongCidProvider;
        view = musicPage;
    }

    public async Task Initialize()
    {
        selectionHelper = new(view.ContentGridView, view.AlbumSelectionFlyout, view.AlbumContextFlyout, flyout => SelectedAlbumInfoContextFlyout = flyout);

        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;
        SelectedAlbumInfoContextFlyout = view.AlbumContextFlyout;

        try
        {
            Albums = await CommonValues.GetOrFetchAlbums();
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
            IEnumerable<AlbumInfo> albumInfos = await CommonValues.GetAlbumsFromServer();

            if (Albums is null || !Albums.CollectionSource.SequenceEqual(albumInfos))
            {
                Albums = CommonValues.CreateAlbumInfoIncrementalLoadingCollection(albumInfos);
                MemoryCacheHelper<CustomIncrementalLoadingCollection<AlbumInfoSource, AlbumInfo>>.Default.Store(CommonValues.AlbumInfoCacheKey, Albums);
            }

            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException ex)
        {
            if (Albums is not null && Albums.Count > 0)
            {
                await CommonValues.DisplayInternetErrorDialog();
            }
            else
            {
                ShowInternetError(ex);
            }
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

    [RelayCommand]
    private void NotifySelectedAlbumInfoContainsInFavoriteChanged()
        => OnPropertyChanged(nameof(IsSelectedAlbumInfoContainsInFavorite));

    [RelayCommand]
    private void StartMultipleSelection() => selectionHelper.StartMultipleSelection(SelectedAlbumInfo);

    [RelayCommand]
    private void StopMultipleSelection() => selectionHelper.StopMultipleSelection();

    [RelayCommand]
    private void SelectAllSongList() => selectionHelper.SelectList(Albums.CollectionSource.Count);

    [RelayCommand]
    private void DeselectAllSongList() => selectionHelper.DeselectList(Albums.CollectionSource.Count);

    private List<AlbumInfo> GetSelectedItems()
    {
        ListViewBase viewBase = view.ContentGridView;

        int listCapacity = 0;

        foreach (ItemIndexRange range in viewBase.SelectedRanges)
        {
            listCapacity += (int)range.Length;
        }

        List<AlbumInfo> selectedItems = new(listCapacity);

        foreach (ItemIndexRange range in viewBase.SelectedRanges)
        {
            selectedItems.AddRange(Albums.CollectionSource.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }

    private AlbumInfoSequenceAdapter GetSongCidProvider() => GetSelectedItems().ToAdapter();
}