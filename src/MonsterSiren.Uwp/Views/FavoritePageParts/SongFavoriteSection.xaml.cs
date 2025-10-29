using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using MonsterSiren.Uwp.Models.Favorites;
using MonsterSiren.Uwp.ViewModels.FavoriteParts;
using Windows.UI.Xaml.Documents;

namespace MonsterSiren.Uwp.Views.FavoritePageParts;

public sealed partial class SongFavoriteSection : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public SongFavoriteSectionViewModel ViewModel { get; }

    public bool IsSongFavoriteEmpty { get => FavoriteService.SongFavoriteList.SongCount <= 0; }

    public SongFavoriteSection()
    {
        ViewModel = new(this);
        this.InitializeComponent();
        ViewModel.SelectedSongListItemContextFlyout = SongContextFlyout;
    }

    private void OnSongFavoriteListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertiesChanged(nameof(IsSongFavoriteEmpty));
    }

    /// <summary>
    /// 通知运行时属性已经发生更改。
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称，其填充是自动完成的。</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnListViewItemSongContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;

        flyout.Items.Clear();

        MenuFlyoutItem addToNowPlayingItem = CommonValues.CreateAddToNowPlayingItem(ViewModel.AddSongFavoriteToNowPlayingCommand, null);
        MenuFlyoutSubItem addToPlaylistSubItem = CommonValues.CreateAddToPlaylistSubItem(ViewModel.AddSongFavoriteToPlaylistCommandCommand);

        flyout.Items.Add(addToNowPlayingItem);
        flyout.Items.Add(addToPlaylistSubItem);
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        if (e.Items.Count <= 0)
        {
            e.Cancel = true;
            return;
        }

        List<SongFavoriteItem> items = new(e.Items.Count);

        foreach (object item in e.Items)
        {
            if (item is SongFavoriteItem favoriteItem)
            {
                items.Add(favoriteItem);
            }
        }

        string json = JsonSerializer.Serialize(items);

        e.Data.SetData(CommonValues.MusicSongFavoriteItemsFormatId, json);
    }

    private async void OnListViewItemGridDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        SongFavoriteItem favoriteItem = (SongFavoriteItem)element.DataContext;

        await ViewModel.PlayForSongItemCommand.ExecuteAsync(favoriteItem);
    }

    private void OnListViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedSongItem = (SongFavoriteItem)element.DataContext;
    }

    private async void OnAlbumTitleHyperlinkClick(Hyperlink sender, HyperlinkClickEventArgs args)
    {
        TextBlock parent = sender.FindAscendant<TextBlock>();

        if (parent?.DataContext is SongFavoriteItem item)
        {
            try
            {
                AlbumDetail detail = await MsrModelsHelper.GetAlbumDetailAsync(item.AlbumCid);
                ContentFrameNavigationHelper.Navigate(typeof(AlbumDetailPage), detail, CommonValues.DefaultTransitionInfo);
            }
            catch
            {
            }
        }
    }

    private void OnMoreOptionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        Button button = (Button)sender;
        ViewModel.SelectedSongItem = (SongFavoriteItem)button.DataContext;
    }

    private void OnSongContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddSongToNowPlayingCommand,
                                                                          ViewModel.SelectedSongItem,
                                                                          ViewModel.AddSongToPlaylistCommand);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }

    private void OnSongSelectionFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddSongListSelectedItemToNowPlayingCommand,
                                                                          null,
                                                                          ViewModel.AddSongListSelectedItemToPlaylistCommand);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }

    private void OnSongFavoriteSectionLoaded(object sender, RoutedEventArgs e)
    {
        FavoriteService.SongFavoriteList.Items.CollectionChanged += OnSongFavoriteListCollectionChanged;
    }

    private void OnSongFavoriteSectionUnloaded(object sender, RoutedEventArgs e)
    {
        FavoriteService.SongFavoriteList.Items.CollectionChanged -= OnSongFavoriteListCollectionChanged;
    }
}
