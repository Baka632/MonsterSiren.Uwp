using Microsoft.Toolkit.Uwp.Notifications;
using MonsterSiren.Uwp.Helpers.Tile;
using Windows.Data.Xml.Dom;

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

        TileHelper.ShowTitle(builder.Build());
    }

    private static void DeleteNowPlayingTile()
    {
        TileHelper.DeleteTile();
    }
}
