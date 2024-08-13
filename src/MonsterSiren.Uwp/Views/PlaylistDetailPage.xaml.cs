﻿// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;

namespace MonsterSiren.Uwp.Views;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistDetailPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsPlaylistEmpty { get => (ViewModel.CurrentPlaylist.Items.Count) <= 0; }

    public PlaylistDetailViewModel ViewModel { get; } = new PlaylistDetailViewModel();

    public PlaylistDetailPage()
    {
        this.InitializeComponent();
    }

    public static string PlaylistTotalDurationToString(TimeSpan? timeSpan)
    {
        if (timeSpan.HasValue)
        {
            TimeSpan span = timeSpan.Value;
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
        else
        {
            return string.Format("MinutesAndSecondsFormat".GetLocalized(), 0, 0);
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Playlist model = (Playlist)e.Parameter;
        ViewModel.Initialize(model);
        ViewModel.CurrentPlaylist.Items.CollectionChanged += OnTotalPlaylistsCollectionChanged;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        ViewModel.CurrentPlaylist.Items.CollectionChanged -= OnTotalPlaylistsCollectionChanged;
    }

    private void OnTotalPlaylistsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaylistEmpty)));
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        SongDetailAndAlbumDetailPack pack = (SongDetailAndAlbumDetailPack)dataContext;

        string json = JsonSerializer.Serialize(pack);

        e.Data.SetData(CommonValues.MusicSongDetailAndAlbumDetailPackFormatId, json);
    }

    private async void OnSongDurationTextBlockLoaded(object sender, RoutedEventArgs e)
    {
        TextBlock textBlock = (TextBlock)sender;
        SongDetail detail = (SongDetail)textBlock.DataContext;
        textBlock.Text = "-:-";

        try
        {
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

    private void OnListViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedPack = (SongDetailAndAlbumDetailPack)element.DataContext;
    }

    private void OnMoreOptionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        Button button = (Button)sender;
        ViewModel.SelectedPack = (SongDetailAndAlbumDetailPack)button.DataContext;
    }

    private void OnAddSongToAnotherPlaylistSubItemLoaded(object sender, RoutedEventArgs e)
    {
        MenuFlyoutSubItem subItem = (MenuFlyoutSubItem)sender;
        FillSubItemWithCommand(subItem, ViewModel.AddPackToAnotherPlaylistCommand);
    }

    private void OnAddPlaylistToAnotherPlaylistSubItemLoaded(object sender, RoutedEventArgs e)
    {
        MenuFlyoutSubItem subItem = (MenuFlyoutSubItem)sender;
        FillSubItemWithCommand(subItem, ViewModel.AddCurrentPlaylistToAnotherPlaylistCommandCommand);
    }

    private void FillSubItemWithCommand(MenuFlyoutSubItem subItem, ICommand command)
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
                    IsEnabled = ViewModel.CurrentPlaylist != playlist
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