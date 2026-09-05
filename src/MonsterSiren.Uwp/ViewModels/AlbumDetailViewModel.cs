using System.Net.Http;
using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="AlbumDetailPage"/> 提供视图模型。
/// </summary>
public partial class AlbumDetailViewModel : ObservableObject
{
    private readonly AlbumDetailPage view;
    private SelectionHelper selectionHelper;
    private bool supressCurrentAlbumFavoriteStateUpdate;

    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private bool isSongsEmpty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedSongInfoContainsInFavorite))]
    private SongInfo selectedSongInfo;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;
    [ObservableProperty]
    private AlbumDetailDisplaySource displaySource = new() { Artistes = [] };
    [ObservableProperty]
    private AlbumCoverLoadArgs coverLoadArgs;
    [ObservableProperty]
    private AlbumDetailAdapter albumProvider;
    [ObservableProperty]
    private bool currentAlbumFavoriteState;

    public Func<ISongCidProvider> SongCidProviderFactory { get; }
    public bool IsSelectedSongInfoContainsInFavorite { get => FavoriteService.ContainsSong(SelectedSongInfo); }

    public AlbumDetailViewModel(AlbumDetailPage albumDetailPage)
    {
        view = albumDetailPage;
        SongCidProviderFactory = GetSongCidProvider;
    }

    async partial void OnCurrentAlbumFavoriteStateChanged(bool value)
    {
        if (supressCurrentAlbumFavoriteStateUpdate)
        {
            return;
        }

        if (value)
        {
            await CommonValues.AddToFavorite(AlbumProvider);
        }
        else
        {
            await CommonValues.RemoveFromFavorite(AlbumProvider);
        }
    }

    /// <summary>
    /// 初始化 <see cref="AlbumDetailViewModel"/>。
    /// </summary>
    /// <param name="albumName">专辑名称。</param>
    /// <param name="albumCid">专辑 CID。</param>
    /// <param name="artistes">专辑艺术家。</param>
    /// <param name="albumIntro">专辑引言。</param>
    /// <param name="songs">专辑歌曲列表。</param>
    /// <param name="coverUri">专辑封面。</param>
    public async Task Initialize(string albumName, string albumCid, IEnumerable<string> artistes = null, string albumIntro = null, IEnumerable<SongInfo> songs = null, string coverUri = null)
    {
        selectionHelper = new(view.SongList, view.SongSelectionFlyout, view.SongContextFlyout, flyout => SelectedSongListItemContextFlyout = flyout);

        IsLoading = true;
        SelectedSongListItemContextFlyout = view.SongContextFlyout;

        try
        {
            AlbumDetailDisplaySource displaySource = new()
            {
                AlbumName = albumName,
                AlbumCid = albumCid,
                Artistes = artistes switch
                {
                    IEnumerable<string> when artistes.Any() => artistes,
                    _ => (await GetAlbumInfo(albumCid)).Artistes,
                },
                AlbumIntro = albumIntro switch
                {
                    string => albumIntro,
                    _ => (await GetAlbumDetail(albumCid)).Intro
                },
                Songs = songs switch
                {
                    IEnumerable<SongInfo> when songs.Any() => songs,
                    _ => (await GetAlbumDetail(albumCid)).Songs
                }
            };

            CoverLoadArgs = new()
            {
                AlbumCid = albumCid,
                CoverUri = coverUri switch
                {
                    string => coverUri,
                    _ => (await GetAlbumDetail(albumCid)).CoverUrl
                }
            };

            AlbumProvider = (await GetAlbumDetail(albumCid)).ToAdapter();

            DisplaySource = displaySource;

            ErrorVisibility = Visibility.Collapsed;
            IsSongsEmpty = displaySource.Songs.Any() != true;

            supressCurrentAlbumFavoriteStateUpdate = true;
            CurrentAlbumFavoriteState = FavoriteService.ContainsAlbum(displaySource.AlbumCid);
            supressCurrentAlbumFavoriteStateUpdate = false;
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

        static async Task<AlbumInfo> GetAlbumInfo(string cid)
        {
            AlbumInfo info = (await CommonValues.GetOrFetchAlbums()).CollectionSource
                .Single(info => info.Cid == cid);
            return info;
        }

        static async Task<AlbumDetail> GetAlbumDetail(string cid)
        {
            return await MsrModelsHelper.GetAlbumDetailAsync(cid);
        }
    }

    [RelayCommand]
    private async Task PlayForCurrentAlbumDetail() => await CommonValues.StartPlay(AlbumProvider);

    [RelayCommand]
    private async Task DownloadForCurrentAlbumDetail() => await CommonValues.StartDownload(AlbumProvider);

    [RelayCommand]
    private void NotifyIsSelectedSongInfoContainsInFavoriteChanged()
        => OnPropertyChanged(nameof(IsSelectedSongInfoContainsInFavorite));

    [RelayCommand]
    private void StartMultipleSelection() => selectionHelper.StartMultipleSelection(SelectedSongInfo);

    [RelayCommand]
    private void StopMultipleSelection() => selectionHelper.StopMultipleSelection();

    [RelayCommand]
    private void SelectAllSongList() => selectionHelper.SelectList(DisplaySource.Songs.Count());

    [RelayCommand]
    private void DeselectAllSongList() => selectionHelper.DeselectList(DisplaySource.Songs.Count());

    private List<SongInfo> GetSelectedItems()
    {
        ListView listView = view.SongList;
        List <SongInfo> selectedItems = new(5);

        foreach (ItemIndexRange range in listView.SelectedRanges)
        {
            selectedItems.AddRange(DisplaySource.Songs.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }

    private SongInfoSequenceAdapter GetSongCidProvider() => GetSelectedItems().ToAdapter();
}