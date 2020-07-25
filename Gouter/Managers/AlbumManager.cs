#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using ATL;
using Commons;
using Dapper.FastCrud;
using Gouter.DataModels;
using Gouter.Extensions;
using Gouter.Utils;

namespace Gouter.Managers
{
    /// <summary>
    /// アルバム管理を行うクラス
    /// </summary>
    internal class AlbumManager : ISubscribable<IAlbumObserver>, IDisposable
    {
        /// <summary>データベース</summary>
        private readonly Database _database;

        /// <summary>アルバム名の比較を行うComparer</summary>
        public static readonly StringComparer AlbumNameComparer = StringComparer.CurrentCultureIgnoreCase;

        /// <summary>アルバムの最終ID</summary>
        private volatile int _albumLatestIdx = -1;

        /// <summary>アルバムIDとアルバム情報が対応したマップ</summary>
        private readonly Dictionary<int, AlbumInfo> _albumIdMap = new Dictionary<int, AlbumInfo>();

        /// <summary>アルバムキーとアルバム情報が対応したマップ</summary>
        private readonly Dictionary<string, AlbumInfo> _albumKeyMap = new Dictionary<string, AlbumInfo>();

        /// <summary>アルバム一覧</summary>
        public ConcurrentNotifiableCollection<AlbumInfo> Albums { get; } = new ConcurrentNotifiableCollection<AlbumInfo>();

        /// <summary>AlbumMangaerを生成する</summary>
        /// <param name="database">DB情報</param>
        public AlbumManager(Database database)
        {
            this._database = database ?? throw new InvalidOperationException();

            this.Albums = new ConcurrentNotifiableCollection<AlbumInfo>();

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        /// <summary>アルバムIDを生成する</summary>
        /// <returns>新規アルバムID</returns>
        public int GenerateId()
        {
            return ++this._albumLatestIdx;
        }

        /// <summary>アルバム情報を追加する</summary>
        /// <param name="albumInfo">アルバム情報</param>
        private void AddImpl(AlbumInfo albumInfo)
        {
            this._albumIdMap.Add(albumInfo.Id, albumInfo);
            this._albumKeyMap.Add(albumInfo.Key, albumInfo);

            this.Albums.Add(albumInfo);
            this._observers.NotifyAll(obsr => obsr.OnRegistered(albumInfo));
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
                using var image = Utils.ImageUtil.ShrinkImageData(artworkData, Utils.ImageUtil.AlbumArtworkMaxSize);
                artwork = image.ToArray();
            }

            var dbContext = this._database.Context;
            dbContext.Albums.Insert(new AlbumDataModel
            {
                Id = albumInfo.Id,
                Key = albumInfo.Key,
                Name = albumInfo.Name,
                Artist = albumInfo.Artist,
                IsCompilation = albumInfo.IsCompilation,
                Artwork = artwork,
                CreatedAt = albumInfo.RegisteredAt,
                UpdatedAt = albumInfo.UpdatedAt,
            });

            if (artwork?.Length > 0)
            {
                dbContext.AlbumArtworks.Insert(new AlbumArtworksDataModel
                {
                    AlbumId = albumInfo.Id,
                    Artwork = artwork,
                    CreatedAt = albumInfo.RegisteredAt,
                    UpdatedAt = albumInfo.UpdatedAt,
                });
            }

            this.AddImpl(albumInfo);
        }

        /// <summary>トラック情報からアルバム情報を取得する。アルバム情報が存在しない場合はトラック情報から抽出して登録する。</summary>
        /// <param name="track">トラック情報</param>
        /// <returns>アルバム情報</returns>
        public AlbumInfo GetOrAddAlbum(Track track)
        {
            var albumKey = track.GenerateAlbumKey();

            if (this._albumKeyMap.TryGetValue(albumKey, out var albumInfo))
            {
                return albumInfo;
            }

            var artwork = track.GetArtworkData();

            albumInfo = new AlbumInfo(this.GenerateId(), albumKey, track, artwork);

            this.Add(albumInfo, artwork);

            return albumInfo;
        }

        /// <summary>アルバムIDからアルバム情報を取得する</summary>
        /// <param name="albumId">アルバムID</param>
        /// <returns>アルバム情報</returns>
        public AlbumInfo FromId(int albumId)
        {
            return this.Albums.Single(album => album.Id == albumId);
        }

        /// <summary>
        /// データベースからアルバム情報をロードする
        /// </summary>
        public void LoadLibrary()
        {
            if (this.Albums.Count > 0)
            {
                // アルバムの読み込みは一度のみ。
                throw new InvalidOperationException();
            }

            var dbContext = this._database.Context;
            var albums = dbContext.Albums;
            var artworksByAlbumId = dbContext.AlbumArtworks.ToDictionary(aw => aw.AlbumId);

            foreach (var album in albums)
            {
                var artwork = artworksByAlbumId.TryGetValue(album.Id, out var aw) ? aw : default;

                var albumInfo = new AlbumInfo(album, artwork);
                this.AddImpl(albumInfo);
            }

            if (this.Albums.Count > 0)
            {
                this._albumLatestIdx = this.Albums.Max(a => a.Id);
            }
        }

        /// <summary>Observer一覧</summary>
        private readonly List<IAlbumObserver> _observers = new List<IAlbumObserver>();

        /// <summary>変更の購読を登録する</summary>
        /// <param name="observer">Observer</param>
        public void Subscribe(IAlbumObserver observer)
        {
            if (!this._observers.Contains(observer))
            {
                this._observers.Add(observer);
            }
        }

        /// <summary>変更の購読を解除する</summary>
        /// <param name="observer">Observer</param>
        public void Describe(IAlbumObserver observer)
        {
            this._observers.Remove(observer);
        }

        /// <summary>リソースを破棄する</summary>
        public void Dispose()
        {
            this._observers.Clear();
        }
    }
}
