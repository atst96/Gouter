using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ATL;

namespace Gouter.Utils
{
    /// <summary>
    /// 画像処理に関するユーティリティクラス
    /// </summary>
    internal static class ImageUtil
    {
        /// <summary>
        /// 指定サイズ以上の画像をアスペクト比を維持して最大サイズまで縮小する。
        /// </summary>
        /// <param name="data">画像データ</param>
        /// <param name="maxSize">最大サイズ</param>
        /// <returns></returns>
        public static MemoryStream ShrinkImageData(byte[] data, int maxSize)
        {
            var srcStream = new MemoryStream(data);

            using (var srcImage = new Bitmap(srcStream))
            {
                if (srcImage.Width <= maxSize && srcImage.Height <= maxSize)
                {
                    // 幅／高さともに最大サイズ以下であれば縮小処理を省く
                    srcStream.Position = 0;
                    return srcStream;
                }

                int resizeWidth, resizeHeight;

                if (srcImage.Width > srcImage.Height)
                {
                    resizeHeight = (int)(maxSize * (srcImage.Height / (double)srcImage.Width));
                    resizeWidth = maxSize;
                }
                else if (srcImage.Width < srcImage.Height)
                {
                    resizeWidth = (int)(maxSize * (srcImage.Width / (double)srcImage.Height));
                    resizeHeight = maxSize;
                }
                else
                {
                    resizeWidth = maxSize;
                    resizeHeight = maxSize;
                }

                using var destImage = ResizeImage(srcImage, resizeWidth, resizeHeight);
                try
                {
                    var imageStream = new MemoryStream();
                    destImage.Save(imageStream, ImageFormat.Tiff);

                    return imageStream;
                }
                finally
                {
                    srcStream.Dispose();
                }
            }
        }

        /// <summary>
        /// 画像をリサイズする。
        /// </summary>
        /// <param name="srcBitmap">対象画像</param>
        /// <param name="width">出力画像の幅</param>
        /// <param name="height">出力画像の高さ</param>
        /// <returns></returns>
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

        /// <summary>
        /// ストリームから<see cref="BitmapImage"/>を生成する。
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <returns><see cref="BitmapImage"/></returns>
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

        private static BitmapImage _missingAlbumImage;
        private static BitmapImage _missingMusicImage;

        /// <summary>
        /// アートワーク未設定時の画像（アルバム用）を取得する。
        /// </summary>
        /// <returns></returns>
        public static BitmapImage GetMissingAlbumImage()
            => _missingAlbumImage ??= GetImage(PathUtil.GetEmbeddedResourcePath("missing_album.png"));

        /// <summary>
        /// アートワーク未設定時の画像（トラック用）を取得する。
        /// </summary>
        /// <returns></returns>
        public static BitmapImage GetMissingMusicImage()
            => _missingMusicImage ??= GetImage(PathUtil.GetEmbeddedResourcePath("missing_music.png"));


        /// <summary>
        /// アートワークの最大サイズ
        /// </summary>
        public const int AlbumArtworkMaxSize = 120;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static BitmapImage GetImage(string uri)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.None;
            image.UriSource = new Uri(uri);
            image.EndInit();
            image.Freeze();

            return image;
        }

        public static byte[] GetArtworkData(this Track track)
        {
            var picture = track.EmbeddedPictures.FirstOrDefault();
            return picture?.PictureData;
        }
    }
}
