using ATL;
using Gouter.Extensions;
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
        private volatile int _latestTrackIdx = -1;

        private IDictionary<int, TrackInfo> _trackIdMap;
        public ConcurrentNotifiableCollection<TrackInfo> Tracks { get; }

        public MusicTrackManager()
        {
            this._trackIdMap = new Dictionary<int, TrackInfo>();
            this.Tracks = new ConcurrentNotifiableCollection<TrackInfo>();

            BindingOperations.EnableCollectionSynchronization(this.Tracks, new object());
        }

        private static string[] GetFiles(string directory)
        {
            return Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        }

        public int GenerateId()
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

        private void AddImpl(TrackInfo trackInfo)
        {
            if (this._trackIdMap.ContainsKey(trackInfo.Id))
            {
                throw new InvalidOperationException("トラックIDが重複しています。");
            }

            this._trackIdMap.Add(trackInfo.Id, trackInfo);
            this.Tracks.Add(trackInfo);
        }

        public TrackInfo Register(Track track)
        {
            var trackInfo = new TrackInfo(track);

            this.AddImpl(trackInfo);

            var values = new Query
            {
                ["id"] = trackInfo.Id,
                ["album_id"] = trackInfo.AlbumInfo.Id,
                ["path"] = trackInfo.Path,
                ["duration"] = (int)trackInfo.Duration.TotalMilliseconds,
                ["disk"] = trackInfo.DiskNumber,
                ["track"] = trackInfo.TrackNumber,
                ["year"] = trackInfo.Year,
                ["album_artist"] = trackInfo.AlbumArtist,
                ["title"] = trackInfo.Title,
                ["artist"] = trackInfo.Artist,
                ["genre"] = trackInfo.Genre,
            };

            Database.Insert(Database.TableNames.Tracks, values);

            return trackInfo;
        }

        public static IList<string> FindNewFiles(IEnumerable<string> findDirectories)
        {
            var registeredFiles = new HashSet<string>(App.TrackManager.Tracks.Select(t => t.Path));

            return findDirectories
                .SelectMany(dir => GetFiles(dir))
                .AsParallel()
                .Distinct(StringComparer.Ordinal)
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

        public void LoadDatabase()
        {
            if (this.Tracks.Count > 0)
            {
                throw new InvalidOperationException();
            }

            foreach (var row in Database.Select(Database.TableNames.Tracks))
            {
                int id = row.Get<int>(0);
                int albumId = row.Get<int>(1);
                var path = row.Get<string>(2);
                int duration = row.Get<int>(3);
                int disk = row.Get<int>(4);
                int track = row.Get<int>(5);
                int year = row.Get<int>(6);
                var albumArtist = row.Get<string>(7);
                var title = row.Get<string>(8);
                var artist = row.Get<string>(9);
                var genre = row.Get<string>(10);

                var trackInfo = new TrackInfo(id, albumId, path, duration, disk, track, year, albumArtist, title, artist, genre);
                this.AddImpl(trackInfo);
            }

            if (this.Tracks.Count > 0)
            {
                this._latestTrackIdx = this.Tracks.Max(t => t.Id);
            }
        }
    }
}
