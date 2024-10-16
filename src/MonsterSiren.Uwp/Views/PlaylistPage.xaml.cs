﻿// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsTotalPlaylistEmpty => PlaylistService.TotalPlaylists.Count <= 0;
    public PlaylistViewModel ViewModel { get; }

    public PlaylistPage()
    {
        ViewModel = new PlaylistViewModel(this);
        this.InitializeComponent();
        ViewModel.SelectedPlaylistContextFlyout = PlaylistContextFlyout;
    }

    private void OnPlaylistItemClick(object sender, ItemClickEventArgs e)
    {
        ContentFrameNavigationHelper.Navigate(typeof(PlaylistDetailPage), e.ClickedItem, new SuppressNavigationTransitionInfo());
    }

    private void OnPlaylistItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        string json = JsonSerializer.Serialize((Playlist)dataContext);
        e.Data.SetData(CommonValues.MusicPlaylistFormatId, json);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        PlaylistService.TotalPlaylists.CollectionChanged += OnTotalPlaylistsCollectionChanged;
    }

    protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);

        PlaylistService.TotalPlaylists.CollectionChanged -= OnTotalPlaylistsCollectionChanged;
    }

    private void OnTotalPlaylistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTotalPlaylistEmpty)));
    }

    private void OnGridViewItemRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedPlaylist = (Playlist)element.DataContext;
    }

    private void OnPlaylistContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddToNowPlayingCommand,
                                                                          ViewModel.SelectedPlaylist,
                                                                          ViewModel.AddPlaylistToAnotherPlaylistCommand,
                                                                          ViewModel.SelectedPlaylist);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }

    private void OnPlaylistSelectionFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddToNowPlayingForSelectedItemCommand,
                                                                          null,
                                                                          ViewModel.AddSelectedItemToPlaylistCommand);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }
}
