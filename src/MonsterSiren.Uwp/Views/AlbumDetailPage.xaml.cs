using System.Net.Http;
using System.Text.Json;
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

        e.Data.SetData(CommonValues.MusicSongInfoAndAlbumPackDetailFormatId, json);
    }

    private async void OnSongDurationTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        SongInfo songInfo = (SongInfo)textBlock.DataContext;
        textBlock.Text = "-:-";

        try
        {
            SongDetail detail = await SongDetailHelper.GetSongDetailAsync(songInfo);
            TimeSpan? span = await SongDetailHelper.GetSongDurationAsync(detail);

            if (span.HasValue)
            {
                textBlock.Text = span.Value.ToString(@"m\:ss");
            }
            else
            {
                textBlock.Visibility = Visibility.Collapsed;
            }
        }
        catch (HttpRequestException)
        {
            textBlock.Visibility = Visibility.Collapsed;
        }
    }
}
