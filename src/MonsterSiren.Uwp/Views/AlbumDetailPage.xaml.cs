// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

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
            if (VisualTreeHelper.GetParent(AlbumCover) is not null)
            {
                ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(CommonValues.AlbumInfoBackConnectedAnimationKeyForMusicPage, AlbumCover);
            }
        }
    }
}
