using System.Collections.Specialized;
using System.Net.Http;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using MonsterSiren.Uwp.Models.Adapters;
using MonsterSiren.Uwp.Models.Favorites;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 专辑详细信息页。
/// </summary>
public sealed partial class AlbumDetailPage : Page
{
    private bool enableBackAnimation = true;

    public AlbumDetailViewModel ViewModel { get; }

    public AlbumDetailPage()
    {
        ViewModel = new AlbumDetailViewModel(this);
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(CommonValues.AlbumInfoForwardConnectedAnimationKeyForMusicPage);
        animation?.TryStart(AlbumCover,
        [
                AlbumName,
                AlbumArtists,
                SeparatorStackPanel,
                DetailScrollViewer,
                ControlBarStackPanel
        ]);

        if (e.Parameter is AlbumInfo albumInfo)
        {
            await ViewModel.Initialize(albumInfo.Name, albumInfo.Cid, albumInfo.Artistes, albumInfo.Intro, null,
                                       albumInfo.CoverUrl).ConfigureAwait(false);
        }
        else if (e.Parameter is ValueTuple<AlbumInfo, bool> tuple)
        {
            albumInfo = tuple.Item1;
            enableBackAnimation = tuple.Item2;

            await ViewModel.Initialize(albumInfo.Name, albumInfo.Cid, albumInfo.Artistes, albumInfo.Intro, null,
                                       albumInfo.CoverUrl).ConfigureAwait(false);
        }
        else if (e.Parameter is AlbumDetail detail)
        {
            enableBackAnimation = false;
            await ViewModel.Initialize(detail.Name, detail.Cid, null, detail.Intro, detail.Songs,
                                       detail.CoverUrl).ConfigureAwait(false);
        }
        else if (e.Parameter is AlbumFavoriteItem item)
        {
            enableBackAnimation = false;
            await ViewModel.Initialize(item.AlbumName, item.AlbumCid, item.Artistes).ConfigureAwait(false);
        }

        FavoriteService.SongFavoriteList.Items.CollectionChanged -= OnSongFavoriteListCollectionChanged;
        FavoriteService.SongFavoriteList.Items.CollectionChanged += OnSongFavoriteListCollectionChanged;
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        FavoriteService.SongFavoriteList.Items.CollectionChanged -= OnSongFavoriteListCollectionChanged;

        if (enableBackAnimation && e.NavigationMode == NavigationMode.Back)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(CommonValues.AlbumInfoBackConnectedAnimationKeyForMusicPage, AlbumCover);
        }
    }

    private void OnSongFavoriteListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        IEnumerable<SongFavoriteItem> listA = e.NewItems?.Cast<SongFavoriteItem>() ?? [];
        IEnumerable<SongFavoriteItem> listB = e.OldItems?.Cast<SongFavoriteItem>() ?? [];
        IEnumerable<SongFavoriteItem> list = listA.Concat(listB);

        IEnumerable<SongInfo> targetSongInfos = ViewModel.DisplaySource.Songs
            .Join(list,
                  info => info.Cid,
                  item => item.SongCid,
                  (info, item) => info);

        foreach (SongInfo songInfo in targetSongInfos)
        {
            int index = SongList.Items.IndexOf(songInfo);
            DependencyObject dep = SongList.ContainerFromIndex(index);

            if (dep is null)
            {
                continue;
            }

            ToggleButton toggleButton = (ToggleButton)dep.FindDescendantByName("SongFavoriteToggleButton");
            CheckSongInFavoriteAndUpdateToggleButton(toggleButton, songInfo);
        }
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        DragHelper.WriteDataToDragItemsStartingEventArgs<SongInfo>(e);
    }

    private void OnSongDurationTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        SongInfo songInfo = (SongInfo)textBlock.DataContext;
        textBlock.Text = "-:-";

        _ = Task.Run(async () =>
        {
            try
            {
                SongDetail detail = await MsrModelsHelper.GetSongDetailAsync(songInfo.Cid);
                TimeSpan? span = await MsrModelsHelper.GetSongDurationAsync(detail);

                await UIThreadHelper.RunOnUIThread(() =>
                {
                    if (span.HasValue)
                    {
                        textBlock.Text = span.Value.ToString(@"m\:ss");
                    }
                    else
                    {
                        textBlock.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (HttpRequestException)
            {
                await UIThreadHelper.RunOnUIThread(() => textBlock.Visibility = Visibility.Collapsed);
            }
        });
    }

    private void OnListViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedSongInfo = (SongInfo)element.DataContext;
    }

    private void OnMoreOptionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        Button button = (Button)sender;
        ViewModel.SelectedSongInfo = (SongInfo)button.DataContext;
    }

    private void OnIndexTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        int index = SongList.Items.IndexOf(textBlock.DataContext);
        textBlock.Text = $"{index + 1}.";
    }

    private async void OnListViewItemGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        SongInfo songInfo = (SongInfo)element.DataContext;

        await CommonValues.StartPlay(songInfo.ToAdapter());
    }

    private void OnSongFavoriteToggleButtonLoaded(object sender, RoutedEventArgs e)
    {
        ToggleButton toggleButton = (ToggleButton)sender;
        SongInfo songInfo = (SongInfo)toggleButton.DataContext;
        CheckSongInFavoriteAndUpdateToggleButton(toggleButton, songInfo);
    }

    private async void OnSongFavoriteToggleButtonClick(object sender, RoutedEventArgs e)
    {
        ToggleButton toggleButton = (ToggleButton)sender;
        SongInfo songInfo = (SongInfo)toggleButton.DataContext;

        bool isFavorite = FavoriteService.ContainsSong(songInfo);
        if (isFavorite)
        {
            await CommonValues.RemoveFromFavorite(songInfo.ToAdapter());
        }
        else
        {
            await CommonValues.AddToFavorite(songInfo.ToAdapter());
        }
    }

    private static void CheckSongInFavoriteAndUpdateToggleButton(ToggleButton toggleButton, SongInfo songInfo)
    {
        bool isFavorite = FavoriteService.ContainsSong(songInfo);
        toggleButton.IsChecked = isFavorite;
    }
}
