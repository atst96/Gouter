using ATL;
using Gouter.DataModels;
using Gouter.Utils;
using System;
using System.Linq;
using System.Windows.Media;

namespace Gouter
{
    /// <summary>
    /// アルバム情報
    /// </summary>
    internal class AlbumInfo : NotificationObject, IPlaylistInfo
    {
        /// <summary>
        /// アルバムの内部ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// アルバム識別キー
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// アルバム名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// アーティスト名
        /// </summary>
        public string Artist { get; private set; }

        /// <summary>
        /// アルバム情報登録日時
        /// </summary>
        public DateTimeOffset RegisteredAt { get; }

        /// <summary>
        /// アルバム情報更新日時
        /// </summary>
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>
        /// アルバムアートの最大サイズ
        /// </summary>
        private const int MaxImageSize = 80;

        /// <summary>
        /// トラック情報からアルバム情報を生成する
        /// </summary>
        /// <param name="id">アルバムID</param>
        /// <param name="key">アルバムキー</param>
        /// <param name="track">トラック情報</param>
        public AlbumInfo(int id, string key, Track track, byte[] artwork)
        {
            this.Id = id;
            this.Key = key;
            this.Name = track.Album;
            this.Artist = track.GetAlbumArtist();
            this.IsCompilation = track.GetIsCompiatilnAlbum();
            this.RegisteredAt = DateTimeOffset.Now;
            this.UpdatedAt = this.RegisteredAt;

            if (artwork?.Length > 0)
            {
                this.SetArtworkData(artwork);
            }

            this.Playlist = new AlbumPlaylist(this);
        }

        /// <summary>
        /// DBのモデルデータからアルバム情報を生成する
        /// </summary>
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
                this.SetArtworkData(artwork);
            }

            this.Playlist = new AlbumPlaylist(this);
        }

        private bool _isArtworkFound = false;
        private byte[] _tempArtworkData = null;

        private object @_lockObj = new object();

        private void SetArtworkData(byte[] imageData)
        {
            lock (this.@_lockObj)
            {
                this._tempArtworkData = imageData;
                this._isArtworkFound = true;
            }
        }

        private ImageSource _artwork;
        /// <summary>
        /// アートワーク
        /// </summary>
        public ImageSource Artwork
        {
            get
            {
                lock (this._lockObj)
                {
                    if (this._artwork == null)
                    {
                        if (!this._isArtworkFound)
                        {
                            this._artwork = ImageUtil.GetMissingAlbumImage();
                        }
                        else
                        {
                            var imageStream = ImageUtil.ShrinkImageData(this._tempArtworkData, MaxImageSize);
                            this._artwork = ImageUtil.BitmapImageFromStream(imageStream);

                            this._tempArtworkData = null;
                        }
                    }

                    return this._artwork;
                }
            }
        }

        /// <summary>
        /// コンピレーションアルバムか否かのフラグ
        /// </summary>
        public bool IsCompilation { get; private set; }

        /// <summary>
        /// プレイリスト情報
        /// </summary>
        public AlbumPlaylist Playlist { get; }
    }
}
