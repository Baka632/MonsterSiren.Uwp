using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using MonsterSiren.Uwp.Helpers.Tile;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MonsterSiren.Uwp.Services;

public partial class MusicInfoService : IDisposable
{
    private const string DefaultTileImageFolderName = "TileImage";
    private static readonly StorageFolder tempFolder = ApplicationData.Current.LocalCacheFolder;
    private static readonly SemaphoreSlim tileFileSemaphore = new(1);
    private bool isUpdatingTile;
    private bool disposedValue;

    private async Task CreateNowPlayingTile()
    {
        isUpdatingTile = true;
        AdaptiveTileBuilder builder = new();

        if (IsLoadingMedia)
        {
            return;
        }

        MusicDisplayProperties currentMusicProperty = CurrentMusicProperties;

        if (MusicService.CurrentMediaPlaybackItem is not null)
        {
            MediaPlaybackItem currentMediaItem = MusicService.CurrentMediaPlaybackItem;
            await tileFileSemaphore.WaitAsync();
            try
            {
                RandomAccessStreamReference cover = currentMediaItem.GetDisplayProperties().Thumbnail;
                using IRandomAccessStreamWithContentType coverStream = await cover.OpenReadAsync();

                StorageFolder tileFolder = await tempFolder.CreateFolderAsync(DefaultTileImageFolderName, CreationCollisionOption.OpenIfExists);
                StorageFile file = await tileFolder.CreateFileAsync("Tile.png", CreationCollisionOption.OpenIfExists);

                StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync();
                await RandomAccessStream.CopyAsync(coverStream, transaction.Stream);
                await transaction.CommitAsync();
                transaction.Dispose();

                string imagePath = file.Path;
                builder.TileSmall.AddBackgroundImage(imagePath, 0);
                builder.TileMedium.AddPeekImage(imagePath, 0);
                builder.TileWide.AddPeekImage(imagePath, 0);
                builder.TileLarge.AddPeekImage(imagePath, 0);
            }
            finally
            {
                tileFileSemaphore.Release();
            }
        }

        builder.TileMedium
            .AddAdaptiveText(currentMusicProperty.Title, true, hintMaxLines: 2)
            .AddAdaptiveText(currentMusicProperty.Artist, true, hintMaxLines: 1)
            .AddAdaptiveText(currentMusicProperty.AlbumTitle, true);

        builder.TileWide
            .AddAdaptiveText(currentMusicProperty.Title, true, AdaptiveTextStyle.Body)
            .AddAdaptiveText(currentMusicProperty.Artist, true)
            .AddAdaptiveText(currentMusicProperty.AlbumTitle, true);

        if (EnvironmentHelper.IsWindowsMobile != true)
        {
            // Windows 10 Mobile 不支持大型磁贴，所以在 Win10M 上不需要添加大型磁贴特定的元素

            builder.TileLarge
                .AddAdaptiveText(currentMusicProperty.Title, true, AdaptiveTextStyle.Title, hintMaxLines: 2)
                .AddAdaptiveText(currentMusicProperty.Artist, true, hintMaxLines: 1)
                .AddAdaptiveText(currentMusicProperty.AlbumTitle, true, hintMaxLines: 1);

            MusicDisplayProperties nextMusicProps = null;

            if (IsShuffle == true)
            {
                List<MediaPlaybackItem> shuffledList = [.. MusicService.CurrentShuffledMediaPlaybackList];
                int index = shuffledList.IndexOf(MusicService.CurrentMediaPlaybackItem);

                TrySetNextMusicProps(ref nextMusicProps, shuffledList, index);
            }
            else
            {
                NowPlayingList nowPlayingList = MusicService.CurrentMediaPlaybackList;
                int index = nowPlayingList.IndexOf(MusicService.CurrentMediaPlaybackItem);

                TrySetNextMusicProps(ref nextMusicProps, nowPlayingList, index);
            }

            if (nextMusicProps != null)
            {
                builder.TileLarge
                    .AddAdaptiveText(string.Empty)
                    .AddAdaptiveText(nextMusicProps.Title, true, hintMaxLines: 2)
                    .AddAdaptiveText(nextMusicProps.Artist)
                    .AddAdaptiveText(nextMusicProps.AlbumTitle, true);
            }
        }

        TileHelper.ShowTitle(builder.Build());
        isUpdatingTile = false;

        static void TrySetNextMusicProps(ref MusicDisplayProperties nextMusicProps, IReadOnlyList<MediaPlaybackItem> items, int index)
        {
            if (items.Count > index + 1)
            {
                nextMusicProps = items[index + 1].GetDisplayProperties().MusicProperties;
            }
            else if (MusicService.PlayerRepeatingState == PlayerRepeatingState.RepeatAll)
            {
                nextMusicProps = items[0].GetDisplayProperties()?.MusicProperties;
            }
        }
    }

    private static void DeleteNowPlayingTile()
    {
        TileHelper.DeleteTile();
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            tileFileSemaphore.Dispose();
            themeListener.Dispose();
            disposedValue = true;
        }
    }

    ~MusicInfoService()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
