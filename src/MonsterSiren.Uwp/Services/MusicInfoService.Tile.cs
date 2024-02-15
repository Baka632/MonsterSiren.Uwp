using System.Reflection;
using Microsoft.Toolkit.Uwp.Notifications;
using MonsterSiren.Uwp.Helpers.Tile;
using MonsterSiren.Uwp.Models;
using Windows.Data.Xml.Dom;
using Windows.Media;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.Services;

public partial class MusicInfoService
{
    private void CreateNowPlayingTile()
    {
        //string tileXml = $"""
        //    <tile>
        //      <visual>
        //        <binding template="TileSmall">
        //         <image src="{CurrentMediaCover.UriSource}" placement="background"/>
        //        </binding>
        //        <binding template="TileMedium">
        //          <image src="{CurrentMediaCover.UriSource}" placement="peek"/>
        //          <text hint-wrap="true" hint-align="Left" >{CurrentMusicProperties.Title}</text>
        //          <text hint-wrap="true">{CurrentMusicProperties.Artist}</text>
        //          <text hint-wrap="true">{CurrentMusicProperties.AlbumTitle}</text>
        //        </binding>
        //        <binding template="TileWide">
        //          <image src="{CurrentMediaCover.UriSource}" placement="peek"/>
        //          <text hint-style="Subtitle" hint-wrap="true" hint-align="Left" >{CurrentMusicProperties.Title}</text>
        //          <text hint-wrap="true">{CurrentMusicProperties.Artist}</text>
        //          <text hint-wrap="true">{CurrentMusicProperties.AlbumTitle}</text>
        //        </binding>
        //        <binding template="TileLarge">
        //          <image src="{CurrentMediaCover.UriSource}" placement="peek"/>
        //          <text hint-style="Title" hint-wrap="true" hint-align="Left" >{CurrentMusicProperties.Title}</text>
        //          <text hint-wrap="true">{CurrentMusicProperties.Artist}</text>
        //          <text hint-wrap="true">{CurrentMusicProperties.AlbumTitle}</text>
        //        </binding>
        //      </visual>
        //    </tile>
        //    """;

        AdaptiveTileBuilder builder = new();

        if (CurrentMediaCover.UriSource is not null)
        {
            string uri = CurrentMediaCover.UriSource.ToString();

            builder.TileSmall.AddBackgroundImage(uri, 0);
            builder.TileMedium.AddPeekImage(uri, 0);
            builder.TileWide.AddPeekImage(uri, 0);
            builder.TileLarge.AddPeekImage(uri, 0);
        }

        builder.TileMedium
            .AddAdaptiveText(CurrentMusicProperties.Title, true, hintMaxLines: 2)
            .AddAdaptiveText(CurrentMusicProperties.Artist, true, hintMaxLines: 1)
            .AddAdaptiveText(CurrentMusicProperties.AlbumTitle, true);

        builder.TileWide
            .AddAdaptiveText(CurrentMusicProperties.Title, true)
            .AddAdaptiveText(CurrentMusicProperties.Artist, true)
            .AddAdaptiveText(CurrentMusicProperties.AlbumTitle, true);

        builder.TileLarge
            .AddAdaptiveText(CurrentMusicProperties.Title, true, AdaptiveTextStyle.Title, hintMaxLines: 2)
            .AddAdaptiveText(CurrentMusicProperties.Artist, true, hintMaxLines: 1)
            .AddAdaptiveText(CurrentMusicProperties.AlbumTitle, true, hintMaxLines: 1);

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

        TileHelper.ShowTitle(builder.Build());

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
}
