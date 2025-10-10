using System.Collections.Specialized;
using System.ComponentModel;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个正在播放列表。
/// </summary>
public sealed class NowPlayingList(IList<MediaPlaybackItem> items) : CustomObservableCollection<MediaPlaybackItem>(items)
{
    private bool shouldMoveToNewItem;
    private TimeSpan newItemPosition;
    private MediaPlaybackState previousState;
    private MediaPlaybackItem previousItem;

    protected override async void InsertItem(int index, MediaPlaybackItem item)
    {
        base.InsertItem(index, item);

        // 处理移动操作的逻辑
        if (shouldMoveToNewItem && ReferenceEquals(item, previousItem))
        {
            MusicService.MoveTo((uint)index);
            await Task.Delay(300);
            MusicService.PlayerPosition = newItemPosition;

            if (previousState != MediaPlaybackState.Paused)
            {
                MusicService.PlayMusic();
            }

            shouldMoveToNewItem = false;
            newItemPosition = TimeSpan.Zero;
            previousItem = null;
        }
    }

    protected override void RemoveItem(int index)
    {
        // 处理移动操作的逻辑
        if (Items[index] == MusicService.CurrentMediaPlaybackItem)
        {
            shouldMoveToNewItem = true;

            previousState = MusicService.PlayerPlayBackState;
            MusicService.PauseMusic();
            newItemPosition = MusicService.PlayerPosition;
            previousItem = MusicService.CurrentMediaPlaybackItem;
        }

        base.RemoveItem(index);
    }

    protected override async void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        await UIThreadHelper.RunOnUIThread(() => base.OnCollectionChanged(e));
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        await UIThreadHelper.RunOnUIThread(() => base.OnPropertyChanged(e));
    }
}
