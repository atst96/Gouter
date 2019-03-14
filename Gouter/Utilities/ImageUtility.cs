using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Gouter.Utilities
{
    internal static class ImageUtility
    {
        public static MemoryStream ShrinkImageData(byte[] data, int iamgeSize)
        {
            var srcStream = new MemoryStream(data);

            using (var srcImage = new Bitmap(srcStream))
            {
                if (srcImage.Width <= iamgeSize && srcImage.Height <= iamgeSize)
                {
                    srcStream.Position = 0;
                    return srcStream;
                }

                int resizeWidth, resizeHeight;

                if (srcImage.Width > srcImage.Height)
                {
                    resizeHeight = (int)(iamgeSize * (srcImage.Height / (double)srcImage.Width));
                    resizeWidth = iamgeSize;
                }
                else if (srcImage.Width < srcImage.Height)
                {
                    resizeWidth = (int)(iamgeSize * (srcImage.Width / (double)srcImage.Height));
                    resizeHeight = iamgeSize;
                }
                else
                {
                    resizeWidth = iamgeSize;
                    resizeHeight = iamgeSize;
                }

                using (srcStream)
                using (var destImage = ResizeImage(srcImage, resizeWidth, resizeHeight))
                {
                    var imageStream = new MemoryStream();
                    destImage.Save(imageStream, ImageFormat.Bmp);

                    return imageStream;
                }
            }
        }

        public static Bitmap ResizeImage(Bitmap srcBitmap, int width, int height)
        {
            var destImage = new Bitmap(width, height);

            using (var g = Graphics.FromImage(destImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(srcBitmap, 0, 0, width, height);
            }

            return destImage;
        }

        public static BitmapImage BitmapImageFromStream(Stream stream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.None;
            image.CreateOptions = BitmapCreateOptions.None;
            image.StreamSource = stream;
            image.EndInit();

            if (image.CanFreeze)
            {
                image.Freeze();
            }

            return image;
        }
    }
}
