﻿using ATL;
using Gouter.DataModels;
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
            this.RegisteredAt = DateTimeOffset.Now;
            this.UpdatedAt = this.RegisteredAt;

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

        public AlbumInfo(AlbumDataModel dataModel)
            : this(dataModel.Key)
        {
            this.Id = dataModel.Id;
            this.Name = dataModel.Name;
            this.Artist = dataModel.Artist;
            this.IsCompilation = dataModel.IsCompilation ?? false;
            this.RegisteredAt = dataModel.CreatedAt;
            this.UpdatedAt = dataModel.UpdatedAt;

            var artwork = dataModel.Artwork;

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
        public DateTimeOffset RegisteredAt { get; }
        public DateTimeOffset UpdatedAt { get; private set; }

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
