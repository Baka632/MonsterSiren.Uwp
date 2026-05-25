using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.UI.Controls;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.ViewModels.FavoriteParts;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Views.FavoritePageParts;

public sealed partial class AlbumFavoriteSection : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public AlbumFavoriteSectionViewModel ViewModel { get; }

    public bool IsAlbumFavoriteEmpty { get => FavoriteService.AlbumFavoriteList.Count <= 0; }

    public AlbumFavoriteSection()
    {
        ViewModel = new(this);
        this.InitializeComponent();
        ViewModel.SelectedAlbumInfoContextFlyout = AlbumContextFlyout;
    }

    private void OnAlbumFavoriteSectionLoaded(object sender, RoutedEventArgs e)
    {
        FavoriteService.AlbumFavoriteList.Items.CollectionChanged += OnAlbumFavoriteListCollectionChanged;
    }

    private void OnAlbumFavoriteSectionUnloaded(object sender, RoutedEventArgs e)
    {
        FavoriteService.AlbumFavoriteList.Items.CollectionChanged -= OnAlbumFavoriteListCollectionChanged;
    }

    private void OnAlbumFavoriteListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertiesChanged(nameof(IsAlbumFavoriteEmpty));
    }

    /// <summary>
    /// 通知运行时属性已经发生更改。
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称，其填充是自动完成的。</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnGridViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        AlbumFavoriteItem item = (AlbumFavoriteItem)element.DataContext;

        ViewModel.SelectedAlbumItem = item;
    }

    private void OnAlbumGridViewContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        Grid grid = (Grid)args.ItemContainer.ContentTemplateRoot;
        ImageEx image = (ImageEx)grid.FindName("AlbumImage");

        if (args.InRecycleQueue)
        {
            image.Source = null;
        }
        else
        {
            args.RegisterUpdateCallback(async (sender, args) =>
            {
                try
                {
                    image.Source = null;

                    AlbumFavoriteItem item = (AlbumFavoriteItem)args.Item;

                    _ = CommonValues.LoadAndCacheMusicCover(image, item);
                }
                catch (Exception)
                {
                    Debugger.Break();
                }
            });
        }
        args.Handled = true;
    }

    private static async Task<AlbumInfo> GetAlbumInfoFromFavoriteItemAsync(AlbumFavoriteItem item)
    {
        return (await CommonValues.GetOrFetchAlbums()).CollectionSource.First(info => info.Cid == item.AlbumCid);
    }

    private async void OnAlbumGridViewDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        AlbumFavoriteItem item = (AlbumFavoriteItem)dataContext;
        AlbumInfo info = await GetAlbumInfoFromFavoriteItemAsync(item);
        string json = JsonSerializer.Serialize(info);
        e.Data.SetData(CommonValues.MusicAlbumInfoFormatId, json);
    }

    private async void OnAlbumGridViewItemClicked(object sender, ItemClickEventArgs e)
    {
        AlbumGridView.PrepareConnectedAnimation(CommonValues.AlbumInfoForwardConnectedAnimationKeyForMusicPage, e.ClickedItem, "AlbumImage");

        ContentFrameNavigationHelper.Navigate(typeof(AlbumDetailPage), e.ClickedItem, new SuppressNavigationTransitionInfo());
    }
}
