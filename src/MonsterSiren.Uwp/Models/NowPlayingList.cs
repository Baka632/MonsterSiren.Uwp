using Windows.Media.Playback;

namespace MonsterSiren.Uwp.Models;

/// <summary>
/// 表示一个播放列表
/// </summary>
public sealed class NowPlayingList : CustomObservableCollection<MediaPlaybackItem>
{
    private bool shouldMoveToNewItem;
    private TimeSpan newItemPosition;
    private MediaPlaybackState previousState;

    public NowPlayingList(IList<MediaPlaybackItem> items) : base(items)
    {
    }

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
}
