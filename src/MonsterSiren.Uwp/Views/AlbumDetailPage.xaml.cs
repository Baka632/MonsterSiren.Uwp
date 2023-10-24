// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Text.Json;
using Windows.UI.Xaml.Media.Animation;

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
        animation?.TryStart(AlbumCover, new UIElement[]
        {
                AlbumName,
                AlbumArtists,
                SeparatorStackPanel,
                DetailScrollViewer,
                ControlBarStackPanel
        });

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

    private void OnListViewItemDragStarting(UIElement sender, DragStartingEventArgs args)
    {
        DragOperationDeferral deferral = args.GetDeferral();

        object dataContext = ((FrameworkElement)sender).DataContext;
        SongInfoAndAlbumDetailPack pack = new((SongInfo)dataContext, ViewModel.CurrentAlbumDetail);

        using MemoryStream stream = new();
        JsonSerializer.SerializeAsync(stream, pack);

        stream.Seek(0, SeekOrigin.Begin);

        StreamReader reader = new(stream);
        string json = reader.ReadToEnd();

        args.Data.SetData(CommonValues.MusicSongInfoAndAlbumPackDetailFormatId, json);

        deferral.Complete();
    }
}
