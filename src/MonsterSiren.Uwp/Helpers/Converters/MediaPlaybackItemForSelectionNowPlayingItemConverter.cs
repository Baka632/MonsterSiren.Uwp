using Windows.Media.Playback;
using Windows.UI;

namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class MediaPlaybackItemForSelectionNowPlayingItemConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MediaPlaybackItem playbackItem && parameter is string args)
        {
            switch (args)
            {
                case "AccentColor":
                    {
                        if (playbackItem == MusicService.CurrentMediaPlaybackItem)
                        {
                            Color accentColor = (Color)Application.Current.Resources["SystemAccentColorLight2"];
                            return new SolidColorBrush(accentColor);
                        }
                        else
                        {
                            Color defaultColor = (Color)Application.Current.Resources["SystemBaseHighColor"];
                            return new SolidColorBrush(defaultColor);
                        }
                    }

                case "IndicatorLoad":
                    return playbackItem == MusicService.CurrentMediaPlaybackItem;
                default:
                    return DependencyProperty.UnsetValue;
            }
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
