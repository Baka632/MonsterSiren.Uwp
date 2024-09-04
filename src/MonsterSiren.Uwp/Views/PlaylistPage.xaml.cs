﻿// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using MonsterSiren.Uwp.Models;
using Windows.UI.Xaml.Media.Animation;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistPage : Page, INotifyPropertyChanged
{
    private object _storedGridViewItem;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsTotalPlaylistEmpty => PlaylistService.TotalPlaylists.Count <= 0;
    public PlaylistViewModel ViewModel { get; } = new PlaylistViewModel();

    public PlaylistPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    private void OnPlaylistItemClick(object sender, ItemClickEventArgs e)
    {
        _storedGridViewItem = e.ClickedItem;
        PlaylistGridView.PrepareConnectedAnimation(CommonValues.PlaylistDetailForwardConnectedAnimationKey, e.ClickedItem, "PlaylistCoverGrid");
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

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        PlaylistService.TotalPlaylists.CollectionChanged += OnTotalPlaylistsCollectionChanged;

        if (_storedGridViewItem is not null && e.NavigationMode == NavigationMode.Back)
        {
            PlaylistGridView.ScrollIntoView(_storedGridViewItem);
            PlaylistGridView.UpdateLayout();

            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation(CommonValues.PlaylistDetailBackConnectedAnimationKey);
            if (animation != null)
            {
                await PlaylistGridView.TryStartConnectedAnimationAsync(animation, _storedGridViewItem, "PlaylistCoverGrid");
            }

            PlaylistGridView.Focus(FocusState.Programmatic);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

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
}
