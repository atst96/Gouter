using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using Gouter.Data;

namespace Gouter.Managers
{
    /// <summary>
    /// メディアの管理を行うクラス
    /// </summary>
    internal class MediaManager : IDisposable
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
        /// アートワーク
        /// </summary>
        public ArtworkManager Artwork { get; private set; }

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
        public MediaManager(string artworkPath)
        {
            this._database = new Database();

            this.Tracks = new TrackManager(this._database);
            this.Artwork = new ArtworkManager(artworkPath);
            this.Albums = new AlbumManager(this._database, this.Artwork);
            this.Playlists = new PlaylistManager(this._database, this.Albums);
        }

        /// <summary>
        /// メディア管理クラスを生成する。
        /// </summary>
        /// <param name="libraryPath"></param>
        /// <returns></returns>
        public static MediaManager CreateMediaManager(string libraryPath, string artworkPath)
        {
            var manager = new MediaManager(artworkPath);
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

        /// <summary>
        /// トラックを登録する
        /// </summary>
        /// <param name="track">トラック情報</param>
        public void RegisterTrack(Track track)
        {
            int newTrackId = this.Tracks.GenerateId();
            var albumInfo = this.Albums.GetOrAddAlbum(track);

            var trackInfo = new TrackInfo(newTrackId, track, albumInfo);
            this.Tracks.Add(trackInfo);
        }

        /// <summary>トラックを一括登録する</summary>
        /// <param name="allTracks">トラック</param>
        /// <param name="progress"></param>
        public void RegisterTracks(IEnumerable<Track> allTracks, IProgress<TrackInsertProgress> progress = null)
        {
            int count = 0;
            int maxCount = allTracks.Count();

            using var transaction = this._database.BeginTransaction();

            try
            {
                var tracksByAlbumKey = allTracks
                    .AsParallel()
                    .GroupBy(t => t.GetAlbumKey());

                foreach (var albumTracks in tracksByAlbumKey)
                {
                    var albumKey = albumTracks.Key;

                    AlbumInfo albumInfo;

                    if (!this.Albums.TryGetFromKey(albumKey, out albumInfo))
                    {
                        // 新規トラックを登録する

                        // 1トラック目のアルバム
                        var firstTrack = albumTracks
                            .Aggregate((left, right) => TrackComparer.Instance.Compare(left, right) < 0 ? left : right);

                        albumInfo = this.Albums.GetOrAddAlbum(firstTrack);
                    }

                    var tracks = albumTracks.Select(t => new TrackInfo(this.Tracks.GenerateId(), t, albumInfo)).ToImmutableList();
                    this.Tracks.Add(tracks);
                    albumInfo.Playlist.Tracks.AddRange(tracks);

                    count += tracks.Count;
                    progress?.Report(new TrackInsertProgress
                    {
                        CurrentCount = count,
                        MaxCount = maxCount,
                        Track = null,
                    });
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Flush()
        {
            this._database.Flush();
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
