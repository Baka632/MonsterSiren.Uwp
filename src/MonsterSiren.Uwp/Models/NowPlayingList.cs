using System.Collections.Specialized;
using System.ComponentModel;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个播放列表
/// </summary>
public sealed class NowPlayingList(IList<MediaPlaybackItem> items) : CustomObservableCollection<MediaPlaybackItem>(items)
{
    private bool shouldMoveToNewItem;
    private TimeSpan newItemPosition;
    private MediaPlaybackState previousState;

    protected override async void InsertItem(int index, MediaPlaybackItem item)
    {
        base.InsertItem(index, item);

        if (shouldMoveToNewItem)
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
        }
    }

    protected override void RemoveItem(int index)
    {
        if (Items[index] == MusicService.CurrentMediaPlaybackItem)
        {
            shouldMoveToNewItem = true;

            previousState = MusicService.PlayerPlayBackState;
            MusicService.PauseMusic();
            newItemPosition = MusicService.PlayerPosition;
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
