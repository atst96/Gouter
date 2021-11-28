using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
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
        public static byte[] ShrinkImageData(byte[] data, int maxSize)
        {
            using var srcStream = new MemoryStream(data);
            var srcImage = BitmapFrame.Create(srcStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

            (double sourceWidth, double sourceHeight) = (srcImage.PixelWidth, srcImage.PixelHeight);

            if (sourceWidth <= maxSize && sourceHeight <= maxSize)
            {
                // 幅／高さともに最大サイズ以下であれば縮小処理を省く
                return data;
            }

            double scale = Math.Min(maxSize / sourceWidth, maxSize / sourceHeight);

            var destImage = ResizeImage(srcImage, scale);

            using var imageStream = new MemoryStream(data.Length);

            SaveTiff(destImage, imageStream);
            imageStream.Position = 0;

            return imageStream.ToArray();
        }

        /// <summary>
        /// 画像をリサイズする。
        /// </summary>
        /// <param name="srcBitmap">対象画像</param>
        /// <param name="width">出力画像の幅</param>
        /// <param name="height">出力画像の高さ</param>
        /// <returns></returns>
        public static BitmapSource ResizeImage(BitmapSource srcBitmap, double scale)
        {
            return new TransformedBitmap(srcBitmap, new ScaleTransform(scale, scale));
        }

        /// <summary>
        /// <see cref="BitmapSource"/>をストリームに保存します。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destStream"></param>
        public static void SaveTiff(BitmapSource source, Stream destStream)
        {
            var encoder = new TiffBitmapEncoder
            {
                Compression = TiffCompressOption.Default,
                Frames = new BitmapFrame[]
                {
                    BitmapFrame.Create(source)
                },
            };

            encoder.Save(destStream);
        }

        /// <summary>
        /// ストリームから<see cref="BitmapImage"/>を生成する。
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <returns><see cref="BitmapImage"/></returns>
        public static BitmapSource BitmapSourceFromStream(Stream stream)
        {
            var bitmap = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            if (bitmap.CanFreeze)
            {
                bitmap.Freeze();
            }

            return bitmap;
        }

        private static BitmapSource _missingAlbumImage;
        private static BitmapSource _missingMusicImage;

        /// <summary>
        /// アートワーク未設定時の画像（アルバム用）を取得する。
        /// </summary>
        /// <returns></returns>
        public static BitmapSource GetMissingAlbumImage()
            => _missingAlbumImage ??= GetImage(PathUtil.GetEmbeddedResourcePath("missing_album.png"));

        /// <summary>
        /// アートワーク未設定時の画像（トラック用）を取得する。
        /// </summary>
        /// <returns></returns>
        public static BitmapSource GetMissingMusicImage()
            => _missingMusicImage ??= GetImage(PathUtil.GetEmbeddedResourcePath("missing_music.png"));


        /// <summary>
        /// アートワークの最大サイズ
        /// </summary>
        public const int AlbumArtworkMaxSize = 200;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static BitmapSource GetImage(string uri)
        {
            return BitmapFrame.Create(new Uri(uri), BitmapCreateOptions.None, BitmapCacheOption.OnDemand);
        }

        public static byte[] GetArtworkData(this Track track)
        {
            var picture = track.EmbeddedPictures.FirstOrDefault();
            return picture?.PictureData;
        }
    }
}
