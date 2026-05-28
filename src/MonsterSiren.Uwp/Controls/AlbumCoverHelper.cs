using Microsoft.Toolkit.Uwp.UI.Controls;

namespace MonsterSiren.Uwp.Controls;

public sealed class AlbumCoverHelper : DependencyObject
{
    public static AlbumCoverLoadArgs GetAlbumCoverLoadArgs(DependencyObject obj)
        => (AlbumCoverLoadArgs)obj.GetValue(AlbumCoverLoadArgsProperty);

    public static void SetAlbumCoverLoadArgs(DependencyObject obj, AlbumCoverLoadArgs value)
        => obj.SetValue(AlbumCoverLoadArgsProperty, value);

    public static readonly DependencyProperty AlbumCoverLoadArgsProperty =
        DependencyProperty.RegisterAttached("AlbumCoverLoadArgs", typeof(AlbumCoverLoadArgs), typeof(AlbumCoverHelper), new PropertyMetadata(default(AlbumCoverLoadArgs), OnAlbumCoverLoadArgsChanged));

    private static async void OnAlbumCoverLoadArgsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != e.NewValue)
        {
            AlbumCoverLoadArgs loadArgs = GetAlbumCoverLoadArgs(d);

            if (string.IsNullOrWhiteSpace(loadArgs.AlbumCid) || string.IsNullOrWhiteSpace(loadArgs.CoverUri))
            {
                return;
            }

            ImageEx image = (ImageEx)d;
            image.DataContext = loadArgs;

            await CommonValues.LoadAndCacheMusicCover(image, loadArgs);
        }
    }
}
