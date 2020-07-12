using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATL;

namespace Gouter.Managers
{
    /// <summary>
    /// メディアの管理を行うクラス
    /// </summary>
    internal class MediaManager
    {
        /// <summary>
        /// データベース情報
        /// </summary>
        private Database _database;

        /// <summary>
        /// ライブラリのファイル名
        /// </summary>
        public string LibraryPath { get; private set; }

        /// <summary>
        /// 初期化済みかどうかのフラグ
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// トラック情報管理
        /// </summary>
        public TrackManager Tracks { get; private set; }

        /// <summary>
        /// アルバム情報管理
        /// </summary>
        public AlbumManager Albums { get; private set; }

        /// <summary>
        /// プレイリスト情報管理
        /// </summary>
        public PlaylistManager Playlists { get; private set; }

        /// <summary>
        /// メディア管理クラスを生成する。
        /// </summary>
        /// <param name="libInfo"></param>
        public MediaManager()
        {
            this._database = new Database();

            this.Tracks = new TrackManager(this._database);
            this.Albums = new AlbumManager(this._database);
            this.Playlists = new PlaylistManager(this._database, this.Albums);
        }

        /// <summary>
        /// メディア管理クラスを生成する。
        /// </summary>
        /// <param name="libraryPath"></param>
        /// <returns></returns>
        public static MediaManager CreateMediaManager(string libraryPath)
        {
            var manager = new MediaManager();
            manager.Initialize(libraryPath);

            return manager;
        }

        /// <summary>
        /// 初期化処理を行う。
        /// </summary>
        public void Initialize(string libraryPath)
        {
            if (this.IsInitialized)
            {
                // 初期化処理は1度のみ
                throw new InvalidOperationException();
            }

            this.IsInitialized = true;

            // DB接続と準備処理
            var db = this._database;
            db.Connect(libraryPath);
            db.PrepareTable();
        }

        /// <summary>
        /// ライブラリを読み込む。
        /// </summary>
        /// <returns></returns>
        public Task LoadLibrary() => Task.Run(() =>
        { 
            this.Albums.LoadLibrary();
            this.Tracks.LoadLibrary(this.Albums);
        });

        /// <summary>トラックを登録する</summary>
        /// <param name="track">トラック情報</param>
        public void RegisterTrack(Track track)
        {
            var newTrackId = this.Tracks.GenerateId();
            var albumInfo = this.Albums.GetOrAddAlbum(track);

            var trackInfo = new TrackInfo(newTrackId, track, albumInfo);
            this.Tracks.Add(trackInfo);
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

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            this._database?.Dispose();
        }
    }
}
