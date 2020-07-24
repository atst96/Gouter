using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Gouter.Utils;

namespace Gouter.Behaviors
{
    internal static class TrackBehavior
    {
        public static TrackInfo GetAlbumArt(DependencyObject obj) => (TrackInfo)obj.GetValue(AlbumArtProperty);

        public static void SetAlbumArt(DependencyObject obj, TrackInfo value) => obj.SetValue(AlbumArtProperty, value);

        public static readonly DependencyProperty AlbumArtProperty =
            DependencyProperty.RegisterAttached("AlbumArt",
                typeof(TrackInfo), typeof(TrackBehavior),
                new PropertyMetadata(null, OnAlbumArtPropertyChanged));

        private static void OnAlbumArtPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = d as Image;
            if (d == null)
            {
                return;
            }

            if (e.NewValue == null)
            {
                BindingOperations.ClearBinding(d, Image.SourceProperty);
                return;
            }

            if (e.NewValue is TrackInfo trackInfo)
            {
                if (File.Exists(trackInfo.Path))
                {
                    try
                    {
                        var track = new ATL.Track(trackInfo.Path);
                        var albumArtData = track.EmbeddedPictures.FirstOrDefault();

                        if (albumArtData?.PictureData?.Length > 0)
                        {
                            var imageSource = new BitmapImage();
                            imageSource.BeginInit();
                            imageSource.CacheOption = BitmapCacheOption.None;
                            imageSource.StreamSource = ImageUtil.ShrinkImageData(albumArtData.PictureData, 128);
                            imageSource.EndInit();
                            imageSource.Freeze();

                            image.SetValue(Image.SourceProperty, imageSource);

                            return;
                        }
                    }
                    catch { /* pass */ }
                }
            }

            d.SetValue(Image.SourceProperty, ImageUtil.GetMissingMusicImage());
        }
    }
}
