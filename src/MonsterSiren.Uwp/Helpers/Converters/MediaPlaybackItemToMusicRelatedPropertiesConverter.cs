using Windows.Media;
using Windows.Media.Playback;

namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class MediaPlaybackItemToMusicRelatedPropertiesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MediaPlaybackItem playbackItem && parameter is string args)
        {
            MusicDisplayProperties musicProperties = playbackItem.GetDisplayProperties().MusicProperties;

            return args switch
            {
                "AlbumArtist" => musicProperties.AlbumArtist,
                "MusicTitle" => musicProperties.Title,
                "MusicAlbum" => musicProperties.AlbumTitle,
                "MusicDuration" => playbackItem.Source.Duration is null ? "-:-" : playbackItem.Source.Duration.Value.ToString(@"m\:ss"),
                "MusicTrackNumber" => musicProperties.TrackNumber,
                _ => DependencyProperty.UnsetValue,
            };
        }
        else
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
