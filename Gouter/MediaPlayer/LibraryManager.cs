using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATL;

namespace Gouter
{
    /// <summary>
    /// ライブラリの管理を行う
    /// </summary>
    internal class LibraryManager : IDisposable
    {
        /// <summary>データベース</summary>
        private readonly Database _database;

        /// <summary>ライブラリのファイル名</summary>
        public string FileName { get; }

        /// <summary>初期化済みか否かのフラグ</summary>
        public bool IsInitialized { get; private set; }

        /// <summary>アルバム情報マネージャ</summary>
        public AlbumManager Albums { get; }

        /// <summary>トラック情報マネージャ</summary>
        public TrackManager Tracks { get; }

        /// <summary>プレイリスト情報マネージャ</summary>
        public PlaylistManager Playlists { get; }

        /// <summary>ライブラリ情報を生成する</summary>
        public LibraryManager()
        {
            this._database = new Database();

            var db = this._database;
            this.Tracks = new TrackManager(db);
            this.Albums = new AlbumManager(db);
            this.Playlists = new PlaylistManager(db, this.Albums);
        }

        /// <summary>初期処理を行う</summary>
        /// <param name="filePath"></param>
        public void Initialize(string filePath)
        {
            if (this.IsInitialized)
            {
                // 初期化処理は一度のみ可能
                throw new InvalidOperationException();
            }

            this.IsInitialized = true;

            // データベースに接続する
            this._database.Connect(filePath);

            // テーブルがなければ作成する

            var queryList = new Dictionary<string, string>
            {
                [Database.TableNames.Albums] = Database.Queries.CreateAlbumsTable,
                [Database.TableNames.Tracks] = Database.Queries.CreateTracksTable,
                [Database.TableNames.AlbumArtworks] = Database.Queries.CreateAlbumArtworksTable,
            };

            var db = this._database;
            var tables = new HashSet<string>(db.EnumerateTableNames());

            foreach (var kvp in queryList)
            {
                if (!tables.Contains(kvp.Key))
                {
                    db.ExecuteNonQuery(kvp.Value);
                }
            }
        }

        /// <summary>ライブラリを非同期で読み込む</summary>
        /// <returns>Task</returns>
        public Task LoadLibrary() => Task.Run(() =>
        {
            this.Albums.LoadDatabase();
            this.Tracks.LoadDatabase(this.Albums);
        });

        /// <summary>トラックを登録する</summary>
        /// <param name="track">トラック情報</param>
        public void RegisterTrack(Track track)
        {
            var newTrackId = this.Tracks.GenerateId();
            var albumInfo = this.Albums.GetOrAddAlbum(track);

            var trackInfo = new TrackInfo(newTrackId, track, albumInfo);
            this.Tracks.Register(trackInfo);
        }

        /// <summary>トラックを一括登録する</summary>
        /// <param name="tracks">トラック</param>
        /// <param name="progress"></param>
        public void RegisterTracks(IEnumerable<Track> tracks, IProgress<int> progress = null)
        {
            int count = 0;

            using var transaction = this._database.BeginTransaction();

            foreach (var track in tracks.AsParallel())
            {
                this.RegisterTrack(track);
                progress?.Report(++count);
            }

            transaction.Commit();
        }

        /// <summary>リソースを解放する</summary>
        public void Dispose()
        {
            this._database?.Dispose();
        }
    }
}
