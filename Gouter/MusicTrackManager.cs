using ATL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gouter
{
    internal class MusicTrackManager
    {
        private ConcurrentDictionary<int, TrackInfo> _trackImpl;
        public ConcurrentNotifiableCollection<TrackInfo> Tracks { get; }

        public MusicTrackManager()
        {
            this._trackImpl = new ConcurrentDictionary<int, TrackInfo>();
            this.Tracks = new ConcurrentNotifiableCollection<TrackInfo>();

            BindingOperations.EnableCollectionSynchronization(this.Tracks, new object());
        }

        private static string[] GetFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        }

        private static volatile int _latestTrackIdx = -1;

        public static int GetMaxTrackIndex()
        {
            return _latestTrackIdx;
        }

        public static void SetTrackIndex(int index)
        {
            if (_latestTrackIdx >= 0)
            {
                throw new NotSupportedException();
            }

            _latestTrackIdx = index;
        }

        public static int GenerateId()
        {
            return ++_latestTrackIdx;
        }

        public readonly static HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".wav", ".mp3", ".acc", ".m4a", ".flac", ".ogg"
        };

        public static bool IsSupportedExtension(string path)
        {
            var extension = Path.GetExtension(path);

            return SupportedExtensions.Contains(extension);
        }

        internal TrackInfo Register(Track track)
        {
            var trackInfo = new TrackInfo(track);

            this.Tracks.Add(trackInfo);

            using (var cmd = App.SqlConnection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO tracks (id, album_id, path, duration, disk, track, year, album_artist, title, artist, genre) VALUES (@id, @album_id, @path, @duration, @disk, @track, @year, @album_artist, @title, @artist, @genre)";

                var @params = cmd.Parameters;
                @params.AddWithValue("id", trackInfo.Id);
                @params.AddWithValue("album_id", trackInfo.AlbumInfo.Id);
                @params.AddWithValue("path", trackInfo.Path);
                @params.AddWithValue("duration", (int)trackInfo.Duration.TotalMilliseconds);
                @params.AddWithValue("disk", trackInfo.DiskNumber);
                @params.AddWithValue("track", trackInfo.TrackNumber);
                @params.AddWithValue("year", trackInfo.Year);
                @params.AddWithValue("album_artist", trackInfo.AlbumArtist);
                @params.AddWithValue("title", trackInfo.Title);
                @params.AddWithValue("artist", trackInfo.Artist);
                @params.AddWithValue("genre", trackInfo.Genre);

                cmd.Prepare();

                cmd.ExecuteNonQuery();
            }

            return trackInfo;
        }

        public static IList<string> FindNewFiles(IEnumerable<string> findDirectories)
        {
            var registeredFiles = new HashSet<string>(App.TrackManager.Tracks.Select(t => t.Path));

            return findDirectories
                .SelectMany(dir => GetFiles(dir))
                .AsParallel()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(path => IsSupportedExtension(path) && !registeredFiles.Contains(path))
                .ToList();
        }

        public static IList<Track> GetTracks(IList<string> files, IProgress<int> progress = null)
        {
            var tracks = new List<Track>(files.Count);

            int count = 0;

            foreach (var path in files.AsParallel())
            {
                try
                {
                    tracks.Add(new Track(path));
                }
                catch { /* pass */ }

                progress?.Report(++count);
            }

            return tracks;
        }
    }
}
