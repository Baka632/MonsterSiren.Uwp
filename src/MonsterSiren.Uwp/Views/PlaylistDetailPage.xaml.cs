// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.Models.Playlists;
using Windows.UI.Xaml.Documents;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistDetailPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsPlaylistEmpty { get => (ViewModel.CurrentPlaylist.Items.Count) <= 0; }

    public PlaylistDetailViewModel ViewModel { get; }

    public PlaylistDetailPage()
    {
        ViewModel = new PlaylistDetailViewModel(this);
        this.InitializeComponent();
    }

    public static string PlaylistTotalDurationToString(TimeSpan timeSpan)
    {
        TimeSpan span = timeSpan;
        if (span.Hours == 0)
        {
            return string.Format("MinutesAndSecondsFormat".GetLocalized(),
                                 span.Minutes,
                                 span.Seconds);
        }
        else
        {
            return string.Format("HoursAndMinutesFormat".GetLocalized(),
                                 (span.Days * 24) + span.Hours,
                                 span.Minutes);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Playlist playlist)
        {
            ViewModel.Initialize(playlist);
            ViewModel.CurrentPlaylist.Items.CollectionChanged += OnTotalPlaylistsCollectionChanged;
        }

        FavoriteService.SongFavoriteList.Items.CollectionChanged -= OnSongFavoriteListCollectionChanged;
        FavoriteService.SongFavoriteList.Items.CollectionChanged += OnSongFavoriteListCollectionChanged;
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        ViewModel.CurrentPlaylist.Items.CollectionChanged -= OnTotalPlaylistsCollectionChanged;
        FavoriteService.SongFavoriteList.Items.CollectionChanged -= OnSongFavoriteListCollectionChanged;
    }

    private void OnSongFavoriteListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        IEnumerable<SongFavoriteItem> listA = e.NewItems?.Cast<SongFavoriteItem>() ?? [];
        IEnumerable<SongFavoriteItem> listB = e.OldItems?.Cast<SongFavoriteItem>() ?? [];
        IEnumerable<SongFavoriteItem> list = listA.Concat(listB);

        IEnumerable<PlaylistItem> targetSongInfos = ViewModel.CurrentPlaylist.Items
            .Join(list,
                  playlistItem => playlistItem.SongCid,
                  favoriteItem => favoriteItem.SongCid,
                  (playlistItem, favoriteItem) => playlistItem);

        foreach (PlaylistItem item in targetSongInfos)
        {
            int index = SongList.Items.IndexOf(item);
            DependencyObject dep = SongList.ContainerFromIndex(index);

            if (dep is null)
            {
                continue;
            }

            ToggleButton toggleButton = (ToggleButton)dep.FindDescendantByName("SongFavoriteToggleButton");
            CheckSongInFavoriteAndUpdateToggleButton(toggleButton, item);
        }
    }

    private void OnTotalPlaylistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertiesChanged(nameof(IsPlaylistEmpty));
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        DragHelper.WriteDataToDragItemsStartingEventArgs<PlaylistItem>(e);
    }

    private void OnListViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedItem = (PlaylistItem)element.DataContext;
    }

    private void OnMoreOptionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        Button button = (Button)sender;
        ViewModel.SelectedItem = (PlaylistItem)button.DataContext;
    }

    /// <summary>
    /// 通知运行时属性已经发生更改。
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称，其填充是自动完成的。</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void OnListViewItemGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        PlaylistItem playlistItem = (PlaylistItem)element.DataContext;

        await CommonValues.StartPlay(playlistItem.ToAdapter(ViewModel.CurrentPlaylist));
    }

    private async void OnAlbumTitleTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        PlaylistItem item = (PlaylistItem)textBlock.DataContext;
        Run run = (Run)textBlock.FindName("AlbumTitleHyperlinkRun");

        if (string.IsNullOrWhiteSpace(item.AlbumTitle))
        {
            run.Text = "...";
            try
            {
                AlbumDetail detail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                run.Text = detail.Name;
                ToolTipService.SetToolTip(textBlock, new ToolTip() { Content = detail.Name });
                await FillAlbumTitleAndSaveAsync(ViewModel.CurrentPlaylist, item, detail.Name);
            }
            catch
            {
            }
        }
        else
        {
            run.Text = item.AlbumTitle;
            ToolTipService.SetToolTip(textBlock, new ToolTip() { Content = item.AlbumTitle });
        }
    }

    private async void OnAlbumTitleHyperlinkClick(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        TextBlock parent = sender.FindAscendant<TextBlock>();

        if (parent?.DataContext is PlaylistItem item)
        {
            try
            {
                AlbumDetail detail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);

                if (string.IsNullOrWhiteSpace(item.AlbumTitle))
                {
                    await FillAlbumTitleAndSaveAsync(ViewModel.CurrentPlaylist, item, detail.Name);
                }

                ContentFrameNavigationHelper.Navigate(typeof(AlbumDetailPage), detail, CommonValues.DefaultTransitionInfo);
            }
            catch
            {
            }
        }
    }

    private static async Task FillAlbumTitleAndSaveAsync(Playlist playlist, PlaylistItem targetItem, string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException($"“{nameof(newTitle)}”不能为 null 或空白。", nameof(newTitle));
        }

        int targetIndex = playlist.Items.IndexOf(targetItem);
        if (targetIndex != -1)
        {
            playlist.Items[targetIndex] = targetItem with { AlbumTitle = newTitle };
            await PlaylistService.SavePlaylistAsync(playlist);
        }
    }

    private async void OnSongFavoriteToggleButtonClick(object sender, RoutedEventArgs e)
    {
        ToggleButton toggleButton = (ToggleButton)sender;
        PlaylistItem playlistItem = (PlaylistItem)toggleButton.DataContext;

        bool isFavorite = FavoriteService.ContainsSong(playlistItem);
        if (isFavorite)
        {
            await CommonValues.RemoveFromFavorite(playlistItem.ToAdapter(ViewModel.CurrentPlaylist));
        }
        else
        {
            await CommonValues.AddToFavorite(playlistItem.ToAdapter(ViewModel.CurrentPlaylist));
        }
    }

    private void OnSongFavoriteToggleButtonLoaded(object sender, RoutedEventArgs e)
    {
        ToggleButton toggleButton = (ToggleButton)sender;
        PlaylistItem playlistItem = (PlaylistItem)toggleButton.DataContext;
        CheckSongInFavoriteAndUpdateToggleButton(toggleButton, playlistItem);
    }

    private static void CheckSongInFavoriteAndUpdateToggleButton(ToggleButton toggleButton, PlaylistItem playlistItem)
    {
        bool isFavorite = FavoriteService.ContainsSong(playlistItem);
        toggleButton.IsChecked = isFavorite;
    }
}
