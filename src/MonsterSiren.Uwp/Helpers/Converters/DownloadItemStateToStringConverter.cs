namespace MonsterSiren.Uwp.Helpers.Converters;

public sealed class DownloadItemStateToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DownloadItemState state)
        {
            return state switch
            {
                DownloadItemState.Downloading => "DownloadItemState_Downloading".GetLocalized(),
                DownloadItemState.Transcoding => "DownloadItemState_Transcoding".GetLocalized(),
                DownloadItemState.WritingTag => "DownloadItemState_WritingTag".GetLocalized(),
                DownloadItemState.Paused => "DownloadItemState_Paused".GetLocalized(),
                DownloadItemState.Done => "DownloadItemState_Done".GetLocalized(),
                DownloadItemState.Error => "DownloadItemState_Error".GetLocalized(),
                DownloadItemState.Canceled => "DownloadItemState_Canceled".GetLocalized(),
                _ => throw new NotImplementedException()
            };
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
