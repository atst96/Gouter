#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using ATL;
using Gouter.DataModels;
using Gouter.Extensions;

namespace Gouter
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

        /// <summary>アルバム情報を登録する</summary>
        /// <param name="albumInfo"></param>
        public void Add(AlbumInfo albumInfo)
        {
            // データベースに登録する

            var dataModel = new AlbumDataModel
            {
                Id = albumInfo.Id,
                Key = albumInfo.Key,
                Name = albumInfo.Name,
                Artist = albumInfo.Artist,
                IsCompilation = albumInfo.IsCompilation,
                Artwork = albumInfo.ArtworkStream?.ToArray(),
                CreatedAt = albumInfo.RegisteredAt,
                UpdatedAt = albumInfo.UpdatedAt,
            };

            dataModel.Insert(this._database);

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

            albumInfo = new AlbumInfo(this.GenerateId(), albumKey, track);

            this.Add(albumInfo);

            return albumInfo;
        }

        /// <summary>アルバムIDからアルバム情報を取得する</summary>
        /// <param name="albumId">アルバムID</param>
        /// <returns>アルバム情報</returns>
        public AlbumInfo FromId(int albumId)
        {
            return this.Albums.Single(album => album.Id == albumId);
        }

        /// <summary>データベースからアルバム情報をロードする</summary>
        public void LoadDatabase()
        {
            if (this.Albums.Count > 0)
            {
                throw new InvalidOperationException();
            }

            var results = AlbumDataModel.GetAll(this._database);

            foreach (var result in results)
            {
                var albumInfo = new AlbumInfo(result);
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
