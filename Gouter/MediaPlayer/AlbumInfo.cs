using System;
using System.Windows.Media;
using ATL;
using Gouter.DataModels;
using Gouter.Utils;

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
        /// アートワークID
        /// </summary>
        public string ArtworkId { get; private set; }

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
        public AlbumInfo(int id, string key, Track track, string artworkId)
        {
            this.Id = id;
            this.Key = key;
            this.Name = track.Album;
            this.Artist = track.GetAlbumArtist();
            this.IsCompilation = track.GetIsCompiatilnAlbum();
            this.ArtworkId = artworkId;
            this.RegisteredAt = DateTimeOffset.Now;
            this.UpdatedAt = this.RegisteredAt;
            this._isArtworkFound = artworkId != null;

            this.Playlist = new AlbumPlaylist(this);
        }

        /// <summary>
        /// DBのモデルデータからアルバム情報を生成する
        /// </summary>
        /// <param name="album">アルバム情報</param>
        /// <param name="artwork">アートワーク情報</param>
        public AlbumInfo(AlbumDataModel album, AlbumArtworksDataModel artwork)
        {
            this.Key = album.Key;
            this.Id = album.Id;
            this.Name = album.Name;
            this.Artist = album.Artist;
            this.ArtworkId = album.ArtworkId;
            this.IsCompilation = album.IsCompilation ?? false;
            this.RegisteredAt = album.CreatedAt;
            this.UpdatedAt = album.UpdatedAt;
            this._isArtworkFound = album.ArtworkId != null;

            this.Playlist = new AlbumPlaylist(this);
        }

        private bool _isArtworkFound = false;

        private object @_lockObj = new object();

        /// <summary>
        /// アートワーク
        /// </summary>
        public ImageSource Artwork
        {
            get
            {
                if (!this._isArtworkFound)
                {
                    return ImageUtil.GetMissingAlbumImage();
                }
                else
                {
                    var awkMgr = App.Instance.MediaManager.Artwork;
                    using var stream = awkMgr.GetStream(this);

                    return stream == null
                        ? ImageUtil.GetMissingAlbumImage()
                        : ImageUtil.BitmapSourceFromStream(stream);
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
