// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MonsterSiren.Uwp.Views;

// TODO: 优化一下性能

// TODO: 考虑加一个连接动画？

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class PlaylistDetailPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsPlaylistEmpty { get => (ViewModel.CurrentPlaylist.Items.Count) <= 0; }

    public PlaylistDetailViewModel ViewModel { get; }

    public PlaylistDetailPage()
    {
        ViewModel = new PlaylistDetailViewModel(this);
        this.InitializeComponent();
    }

    public static string PlaylistTotalDurationToString(TimeSpan timeSpan)
    {
        TimeSpan span = timeSpan;
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
        OnPropertiesChanged(nameof(IsPlaylistEmpty));
    }

    private void OnSongListViewItemsDragStarting(object sender, DragItemsStartingEventArgs e)
    {
        // TODO: 不要只选一个嘛，多选拖拽怎么办？

        object dataContext = e.Items.FirstOrDefault();

        if (dataContext is null)
        {
            e.Cancel = true;
            return;
        }

        PlaylistItem pack = (PlaylistItem)dataContext;

        string json = JsonSerializer.Serialize(pack);

        e.Data.SetData(CommonValues.MusicPlaylistItemFormatId, json);
    }

    private void OnListViewItemGridRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = (FrameworkElement)sender;
        ViewModel.SelectedItem = (PlaylistItem)element.DataContext;
    }

    private void OnMoreOptionButtonTapped(object sender, TappedRoutedEventArgs e)
    {
        Button button = (Button)sender;
        ViewModel.SelectedItem = (PlaylistItem)button.DataContext;
    }

    /// <summary>
    /// 通知运行时属性已经发生更改
    /// </summary>
    /// <param name="propertyName">发生更改的属性名称,其填充是自动完成的</param>
    public void OnPropertiesChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnSongContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddItemToNowPlayingCommand,
                                                                          ViewModel.SelectedItem,
                                                                          ViewModel.AddItemToAnotherPlaylistCommand,
                                                                          ViewModel.CurrentPlaylist);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }

    private void OnListViewItemSongContextFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;

        flyout.Items.Clear();

        MenuFlyoutItem addToNowPlayingItem = CommonValues.CreateAddToNowPlayingItem(ViewModel.AddCurrentPlaylistToNowPlayingCommand, null);
        MenuFlyoutSubItem addToPlaylistSubItem = CommonValues.CreateAddToPlaylistSubItem(ViewModel.AddCurrentPlaylistToAnotherPlaylistCommandCommand, ViewModel.CurrentPlaylist);

        flyout.Items.Add(addToNowPlayingItem);
        flyout.Items.Add(addToPlaylistSubItem);
    }

    private void OnSongSelectionFlyoutOpening(object sender, object e)
    {
        MenuFlyout flyout = (MenuFlyout)sender;
        MenuFlyoutItemBase target = flyout.Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = flyout.Items.IndexOf(target);
        flyout.Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddSongListSelectedItemToNowPlayingCommand,
                                                                          null,
                                                                          ViewModel.AddSongListSelectedItemToAnotherPlaylistCommand,
                                                                          ViewModel.CurrentPlaylist);
        subItem.Tag = "Placeholder_For_AddTo";
        flyout.Items.Insert(targetIndex, subItem);
    }
}
