using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using ATL;
using Gouter.DataModels;
using Gouter.Utils;

namespace Gouter.Managers
{
    /// <summary>
    /// アルバム管理を行うクラス
    /// </summary>
    internal class AlbumManager : IDisposable
    {
        /// <summary>
        /// データベース
        /// </summary>
        private readonly Database _database;

        /// <summary>
        /// アートワーク管理クラス
        /// </summary>
        public ArtworkManager Artwork { get; }

        /// <summary>
        /// アルバム名の比較を行うComparer
        /// </summary>
        public static readonly StringComparer AlbumNameComparer = StringComparer.CurrentCultureIgnoreCase;

        /// <summary>
        /// アルバムの最終ID
        /// </summary>
        private volatile int _albumLatestIdx = -1;

        /// <summary>
        /// アルバムIDとアルバム情報が対応したマップ
        /// </summary>
        private readonly Dictionary<int, AlbumInfo> _albumIdMap = new Dictionary<int, AlbumInfo>();

        /// <summary>
        /// アルバムキーとアルバム情報が対応したマップ
        /// </summary>
        private readonly Dictionary<string, AlbumInfo> _albumKeyMap = new Dictionary<string, AlbumInfo>();

        /// <summary>
        /// アルバム一覧
        /// </summary>
        public ConcurrentNotifiableCollection<AlbumInfo> Albums { get; } = new ConcurrentNotifiableCollection<AlbumInfo>();

        /// <summary>
        /// アルバム登録時のイベント
        /// </summary>
        public event EventHandler<AlbumInfo> Registered;

        /// <summary>
        /// アルバム削除時のイベント
        /// </summary>
        public event EventHandler<AlbumInfo> Removed;

        /// <summary>
        /// AlbumMangaerを生成する。
        /// </summary>
        /// <param name="database">DB情報</param>
        /// <param name="artwork">アートワーク</param>
        public AlbumManager(Database database, ArtworkManager artwork)
        {
            this._database = database ?? throw new InvalidOperationException();

            this.Albums = new ConcurrentNotifiableCollection<AlbumInfo>();
            this.Artwork = artwork;

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        /// <summary>
        /// アルバムIDを生成する。
        /// </summary>
        /// <returns>新規アルバムID</returns>
        public int GenerateId() => ++this._albumLatestIdx;

        /// <summary>
        /// アルバム情報を追加する。
        /// </summary>
        /// <param name="albumInfo">アルバム情報</param>
        private void AddImpl(AlbumInfo albumInfo)
        {
            this._albumIdMap.Add(albumInfo.Id, albumInfo);
            this._albumKeyMap.Add(albumInfo.Key, albumInfo);

            this.Albums.Add(albumInfo);

            this.Registered?.Invoke(this, albumInfo);
        }

        /// <summary>
        /// アルバム情報を登録する。
        /// </summary>
        /// <param name="albumInfo">アルバム情報</param>
        /// <param name="artworkData">アートワーク</param>
        public void Add(AlbumInfo albumInfo, byte[] artworkData)
        {
            // データベースに登録する

            byte[]? artwork = null;
            if (artworkData?.Length > 0)
            {
                artwork = ImageUtil.ShrinkImageData(artworkData, ImageUtil.AlbumArtworkMaxSize);
            }

            var dbContext = this._database.Context;
            dbContext.Albums.Insert(new AlbumDataModel
            {
                Id = albumInfo.Id,
                Key = albumInfo.Key,
                ArtworkId = albumInfo.ArtworkId,
                Name = albumInfo.Name,
                Artist = albumInfo.Artist,
                IsCompilation = albumInfo.IsCompilation,
                CreatedAt = albumInfo.RegisteredAt,
                UpdatedAt = albumInfo.UpdatedAt,
            });

            if (artwork?.Length > 0)
            {
                this.Artwork.Add(albumInfo, artwork);
            }

            this.AddImpl(albumInfo);
        }

        /// <summary>
        /// アルバムキーからアルバム情報を取得する。
        /// </summary>
        /// <param name="albumKey"></param>
        /// <param name="albumInfo"></param>
        /// <returns></returns>
        internal bool TryGetFromKey(string albumKey, out AlbumInfo albumInfo)
        {
            return this._albumKeyMap.TryGetValue(albumKey, out albumInfo);
        }

        /// <summary>
        /// トラック情報からアルバム情報を取得する。アルバム情報が存在しない場合はトラック情報から抽出して登録する。
        /// </summary>
        /// <param name="track">トラック情報</param>
        /// <returns>アルバム情報</returns>
        public AlbumInfo GetOrAddAlbum(Track track)
        {
            var albumKey = track.GetAlbumKey();

            if (this._albumKeyMap.TryGetValue(albumKey, out var albumInfo))
            {
                return albumInfo;
            }

            byte[] artwork = track.GetArtworkData();
            string artworkId;

            if (artwork?.Length > 0)
            {
                artworkId = Guid.NewGuid().ToString("D");
            }
            else
            {
                artworkId = null;
                artwork = null;
            }

            albumInfo = new AlbumInfo(this.GenerateId(), albumKey, track, artworkId);

            this.Add(albumInfo, artwork);

            return albumInfo;
        }

        /// <summary>
        /// アルバムIDからアルバム情報を取得する。
        /// </summary>
        /// <param name="albumId">アルバムID</param>
        /// <returns>アルバム情報</returns>
        public AlbumInfo FromId(int albumId)
        {
            return this.Albums.Single(album => album.Id == albumId);
        }

        /// <summary>
        /// データベースからアルバム情報をロードする。
        /// </summary>
        public void Load()
        {
            if (this.Albums.Count > 0)
            {
                // アルバムの読み込みは一度のみ。
                throw new InvalidOperationException();
            }

            var dbContext = this._database.Context;
            var albums = dbContext.Albums.ToArray();
            var artworksByAlbumId = dbContext.AlbumArtworks.ToDictionary(aw => aw.AlbumId);

            var registeredAlbums = new List<AlbumInfo>(albums.Length);
            foreach (var album in albums)
            {
                var artwork = artworksByAlbumId.TryGetValue(album.Id, out var aw) ? aw : default;
                var albumInfo = new AlbumInfo(album, artwork);

                this._albumIdMap.Add(albumInfo.Id, albumInfo);
                this._albumKeyMap.Add(albumInfo.Key, albumInfo);
                registeredAlbums.Add(albumInfo);
            }

            this.Albums.AddRange(registeredAlbums);

            if (this.Albums.Count > 0)
            {
                this._albumLatestIdx = this.Albums.Max(a => a.Id);
            }
        }

        /// <summary>
        /// リソースを破棄する。
        /// </summary>
        public void Dispose()
        {
        }
    }
}
