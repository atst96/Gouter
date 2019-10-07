using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATL;

namespace Gouter
{
    internal class LibraryManager : IDisposable
    {
        private readonly Database _database;
        public string FileName { get; }
        public bool IsInitialized { get; private set; }
        public AlbumManager Albums { get; }
        public TrackManager Tracks { get; }
        public PlaylistManager Playlists { get; }

        public LibraryManager()
        {
            this._database = new Database();

            var db = this._database;
            this.Tracks = new TrackManager(db);
            this.Albums = new AlbumManager(db);
            this.Playlists = new PlaylistManager();
        }

        /// <summary>
        /// 初期処理を行う。
        /// </summary>
        /// <param name="filePath"></param>
        public void Initialize(string filePath)
        {
            if (this.IsInitialized)
            {
                throw new InvalidOperationException();
            }

            this.IsInitialized = true;

            this._database.Connect(filePath);

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

        public Task LoadLibrary() => Task.Run(() =>
        {
            this.Albums.LoadDatabase();
            this.Tracks.LoadDatabase(this.Albums);
        });

        /// <summary>
        /// トラックを登録する。
        /// </summary>
        /// <param name="track">トラック情報</param>
        public void RegisterTrack(Track track)
        {
            var newTrackId = this.Tracks.GenerateId();
            var albumInfo = this.Albums.GetOrAddAlbum(track);

            var trackInfo = new TrackInfo(newTrackId, track, albumInfo);
            this.Tracks.Register(trackInfo);
        }

        /// <summary>
        /// トラックを一括登録する。
        /// </summary>
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
        /// リソースを解放する。
        /// </summary>
        public void Dispose()
        {
            this._database?.Dispose();
        }
    }
}
