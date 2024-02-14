namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class NewsCategoryToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is NewsCategory category)
        {
            return category switch
            {
                NewsCategory.NewSongs => "NewsCategory_NewSongs".GetLocalized(),
                NewsCategory.LaunchSpeech => "NewsCategory_LaunchSpeech".GetLocalized(),
                NewsCategory.NewsAlert => "NewsCategory_NewsAlert".GetLocalized(),
                NewsCategory.ArtistState => "NewsCategory_ArtistState".GetLocalized(),
                NewsCategory.SpecialRadio => "NewsCategory_SpecialRadio".GetLocalized(),
                _ => DependencyProperty.UnsetValue
            };
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
