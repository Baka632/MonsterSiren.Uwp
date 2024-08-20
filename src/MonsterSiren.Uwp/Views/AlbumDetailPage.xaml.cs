using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 专辑详细信息页
/// </summary>
public sealed partial class AlbumDetailPage : Page
{
    public AlbumDetailViewModel ViewModel { get; } = new AlbumDetailViewModel();

    public AlbumDetailPage()
    {
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
            await ViewModel.Initialize(albumInfo).ConfigureAwait(false);
        }
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        if (e.NavigationMode == NavigationMode.Back)
        {
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(CommonValues.AlbumInfoBackConnectedAnimationKeyForMusicPage, AlbumCover);
        }
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        SongInfoAndAlbumDetailPack pack = new((SongInfo)dataContext, ViewModel.CurrentAlbumDetail);

        string json = JsonSerializer.Serialize(pack);

        e.Data.SetData(CommonValues.MusicSongInfoAndAlbumDetailPackFormatId, json);
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

    private void OnAddSongInfoToPlaylistSubItemLoaded(object sender, RoutedEventArgs e)
    {
        MenuFlyoutSubItem subItem = (MenuFlyoutSubItem)sender;
        FillSubItemWithCommand(subItem, ViewModel.AddToPlaylistForSongInfoCommand);
    }

    private void OnAddAlbumDetailToPlaylistSubItemLoaded(object sender, RoutedEventArgs e)
    {
        MenuFlyoutSubItem subItem = (MenuFlyoutSubItem)sender;
        FillSubItemWithCommand(subItem, ViewModel.AddToPlaylistForCurrentAlbumDetailCommand);
    }

    private static void FillSubItemWithCommand(MenuFlyoutSubItem subItem, ICommand command)
    {
        if (PlaylistService.TotalPlaylists.Count > 0)
        {
            subItem.Items.Clear();
            subItem.IsEnabled = true;

            foreach (Playlist playlist in PlaylistService.TotalPlaylists)
            {
                MenuFlyoutItem item = new()
                {
                    DataContext = playlist,
                    Text = playlist.Title,
                    Icon = new FontIcon()
                    {
                        Glyph = "\uEC4F"
                    },
                    Command = command,
                    CommandParameter = playlist,
                };
                subItem.Items.Add(item);
            }
        }
        else
        {
            subItem.IsEnabled = false;
        }
    }
}
