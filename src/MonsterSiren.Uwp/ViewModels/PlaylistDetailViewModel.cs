using MonsterSiren.Uwp.Models.Abstracts;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="PlaylistDetailPage"/> 提供视图模型。
/// </summary>
public sealed partial class PlaylistDetailViewModel : ObservableObject
{
    private readonly PlaylistDetailPage view;

    [ObservableProperty]
    private Playlist currentPlaylist;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedItemContainsInFavorite))]
    [NotifyPropertyChangedFor(nameof(SelectedItemAdapter))]
    private PlaylistItem selectedItem;
    [ObservableProperty]
    private FlyoutBase selectedSongListItemContextFlyout;

    private SelectionHelper selectionHelper;

    public PlaylistItemAdapter SelectedItemAdapter { get => SelectedItem.ToAdapter(CurrentPlaylist); }
    public bool IsSelectedItemContainsInFavorite { get => FavoriteService.ContainsSong(SelectedItem); }

    public Func<ISongCidProvider> SongCidProviderFactory { get; }

    public PlaylistDetailViewModel(PlaylistDetailPage playlistDetailPage)
    {
        view = playlistDetailPage;
        SongCidProviderFactory = GetSongCidProvider;
    }

    public void Initialize(Playlist model)
    {
        selectionHelper = new(view.SongList, view.SongSelectionFlyout, view.SongContextFlyout, flyout => SelectedSongListItemContextFlyout = flyout);
        CurrentPlaylist = model ?? throw new ArgumentNullException(nameof(model));
        SelectedSongListItemContextFlyout = view.SongContextFlyout;
    }

    [RelayCommand]
    private async Task PlayForCurrentPlaylist()
    {
        await CommonValues.StartPlay(CurrentPlaylist.ToAdapter());
    }

    [RelayCommand]
    private void NotifyIsSelectedItemContainsInFavorite() =>
        OnPropertyChanged(nameof(IsSelectedItemContainsInFavorite));

    [RelayCommand]
    private async Task DownloadForCurrentPlaylist()
    {
        await CommonValues.StartDownload(CurrentPlaylist.ToAdapter());
    }

    [RelayCommand]
    private async Task RemoveItemFromPlaylist(PlaylistItem item)
    {
        await PlaylistService.RemoveItemForPlaylistAsync(CurrentPlaylist, item);
    }

    [RelayCommand]
    private async Task ModifyPlaylist()
    {
        await CommonValues.ShowModifyPlaylistDialog(CurrentPlaylist);
    }

    [RelayCommand]
    private async Task RemovePlaylist()
    {
        await CommonValues.RemovePlaylist(CurrentPlaylist);
    }

    [RelayCommand]
    private void StartMultipleSelection() => selectionHelper.StartMultipleSelection(SelectedItem);

    [RelayCommand]
    private void StopMultipleSelection()
    {
        selectionHelper.StopMultipleSelection();
    }

    [RelayCommand]
    private void SelectAllSongList()
    {
        view.SongList.SelectRange(new ItemIndexRange(0, (uint)CurrentPlaylist.SongCount));
    }

    [RelayCommand]
    private void DeselectAllSongList()
    {
        view.SongList.DeselectRange(new ItemIndexRange(0, (uint)CurrentPlaylist.SongCount));
    }

    private List<PlaylistItem> GetSelectedItems()
    {
        ListView listView = view.SongList;
        List <PlaylistItem> selectedItems = new(5);

        foreach (ItemIndexRange range in listView.SelectedRanges)
        {
            selectedItems.AddRange(CurrentPlaylist.Items.Skip(range.FirstIndex).Take((int)range.Length));
        }

        return selectedItems;
    }

    private PlaylistItemSequenceAdapter GetSongCidProvider() => GetSelectedItems().ToAdapter();
}