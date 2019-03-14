using ATL;
using Gouter.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gouter
{
    internal class AlbumInfo : NotificationObject
    {
        private const int MaxImageSize = 128;

        public AlbumInfo(string key, Track track)
        {
            this.Tracks = new ConcurrentNotifiableCollection<TrackInfo>();

            this.Id = AlbumManager.GenerateId();
            this.Key = key;
            this.Name = track.Album;
            this.Artist = AlbumManager.GetAlbumArtist(track);

            var artwork = track.EmbeddedPictures.FirstOrDefault();

            if (artwork?.PictureData?.Length > 0)
            {
                this.ArtworkStream = ImageUtility.ShrinkImageData(artwork.PictureData, MaxImageSize);
            }
        }

        public AlbumInfo(int id, string key, string name, string artist, bool isCompilation, byte[] artwork)
        {
            this.Tracks = new ConcurrentNotifiableCollection<TrackInfo>();

            this.Id = id;
            this.Key = key;
            this.Name = name;
            this.Artist = artist;
            this.IsCompilation = isCompilation;
            if (artwork?.Length > 0)
            {
                this.ArtworkStream = new MemoryStream(artwork);
            }
        }

        public int Id { get; private set; }

        public string Key { get; private set; }

        public string Name { get; private set; }

        public string Artist { get; private set; }

        public MemoryStream ArtworkStream { get; private set; }

        private bool _isArtworkLoaded = false;

        private ImageSource _artwork;

        public ImageSource Artwork
        {
            get
            {
                if (this._isArtworkLoaded)
                {
                    return this._artwork;
                }

                this._isArtworkLoaded = true;

                if (this.ArtworkStream == null)
                {
                    return null;
                }

                this.ArtworkStream.Position = 0;

                this._artwork = ImageUtility.BitmapImageFromStream(this.ArtworkStream);

                return this._artwork;
            }
        }

        public bool IsCompilation { get; private set; }

        public ConcurrentNotifiableCollection<TrackInfo> Tracks { get; }
    }
}
