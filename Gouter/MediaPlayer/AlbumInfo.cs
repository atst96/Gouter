using ATL;
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
    /// <summary>
    /// アルバム情報
    /// </summary>
    internal class AlbumInfo : NotificationObject, IPlaylistInfo
    {
        /// <summary>アルバムの内部ID</summary>
        public int Id { get; }

        /// <summary>アルバム識別キー</summary>
        public string Key { get; }

        /// <summary>アルバム名</summary>
        public string Name { get; private set; }

        /// <summary>アーティスト名</summary>
        public string Artist { get; private set; }

        /// <summary>アルバム情報登録日時</summary>
        public DateTimeOffset RegisteredAt { get; }

        /// <summary>アルバム情報更新日時</summary>
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>アルバムアートの最大サイズ</summary>
        private const int MaxImageSize = 128;
        
        /// <summary>トラック情報からアルバム情報を生成する</summary>
        /// <param name="id">アルバムID</param>
        /// <param name="key">アルバムキー</param>
        /// <param name="track">トラック情報</param>
        public AlbumInfo(int id, string key, Track track)
        {
            this.Id = id;
            this.Key = key;
            this.Name = track.Album;
            this.Artist = track.GetAlbumArtist();
            this.IsCompilation = track.GetIsCompiatilnAlbum();
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

        /// <summary>DBのモデルデータからアルバム情報を生成する</summary>
        /// <param name="dataModel">DBモデル</param>
        public AlbumInfo(AlbumDataModel dataModel)
        {
            this.Key = dataModel.Key;
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

        private MemoryStream _artworkStream;
        /// <summary>アートワークの生データ</summary>
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
        /// <summary>アートワーク</summary>
        public ImageSource Artwork
        {
            get => this._artwork;
            private set => this.SetProperty(ref this._artwork, value);
        }

        /// <summary>コンピレーションアルバムか否かのフラグ</summary>
        public bool IsCompilation { get; private set; }

        /// <summary>プレイリスト情報</summary>
        public AlbumPlaylist Playlist { get; }
    }
}
