using ATL;
using Gouter.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Gouter
{
    internal class AlbumInfo : NotificationObject, IPlaylistInfo
    {
        private readonly static AlbumManager AlbumManager = App.AlbumManager;

        private const int MaxImageSize = 128;

        private AlbumInfo(string key)
        {
            this.Key = key;
        }

        public AlbumInfo(string key, Track track)
            : this(key)
        {
            this.Id = AlbumManager.GenerateId();
            this.Name = track.Album;
            this.Artist = AlbumManager.GetAlbumArtist(track);

            var artwork = track.EmbeddedPictures.FirstOrDefault();

            if (artwork?.PictureData?.Length > 0)
            {
                this.ArtworkStream = ImageUtility.ShrinkImageData(artwork.PictureData, MaxImageSize);
            }
            else
            {
                this.Artwork = ImageUtility.GetMissingAlbumImage();
            }

            this.Playlist = new AlbumPlaylist(this);
        }

        public AlbumInfo(int id, string key, string name, string artist, bool isCompilation, byte[] artwork)
            : this(key)
        {
            this.Id = id;
            this.Name = name;
            this.Artist = artist;
            this.IsCompilation = isCompilation;

            if (artwork?.Length > 0)
            {
                this.ArtworkStream = new MemoryStream(artwork);
            }
            else
            {
                this.Artwork = ImageUtility.GetMissingAlbumImage();
            }

            this.Playlist = new AlbumPlaylist(this);
        }

        public int Id { get; }

        public string Key { get; }

        public string Name { get; private set; }

        public string Artist { get; private set; }

        private MemoryStream _artworkStream;
        public MemoryStream ArtworkStream
        {
            get => this._artworkStream;
            set
            {
                if (this.SetProperty(ref this._artworkStream, value))
                {
                    if (value == null || value.Length == 0)
                    {
                        this.Artwork = ImageUtility.GetMissingAlbumImage();
                    }
                    else
                    {
                        this.Artwork = ImageUtility.BitmapImageFromStream(value);
                    }
                }
            }
        }

        private ImageSource _artwork;
        public ImageSource Artwork
        {
            get => this._artwork;
            private set => this.SetProperty(ref this._artwork, value);
        }

        public bool IsCompilation { get; private set; }

        public AlbumPlaylist Playlist { get; }
    }
}
