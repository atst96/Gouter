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

        private readonly IDictionary<int, TrackInfo> _trackIdMap;
        public ConcurrentNotifiableCollection<TrackInfo> Tracks { get; }

        public MusicTrackManager()
        {
            this._trackIdMap = new Dictionary<int, TrackInfo>();
            this.Tracks = new ConcurrentNotifiableCollection<TrackInfo>();

            BindingOperations.EnableCollectionSynchronization(this.Tracks, new object());
        }

        private static IEnumerable<string> GetFiles(string directory)
        {
            return Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
        }

        public int GenerateId()
        {
            return ++this._latestTrackIdx;
        }

        public static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        public void RegisterAll(IEnumerable<Track> tracks, IProgress<int> progress = null)
        {
            int count = 0;

            using (var transaction = Database.BeginTransaction())
            {
                foreach (var track in tracks)
                {
                    var trackInfo = App.TrackManager.Register(track);
                    progress?.Report(++count);
                }

                transaction.Commit();
            }
        }

        private static bool IsContainsDirectory(string path, IEnumerable<string> directories)
        {
            return directories.Any(dir => !path.Equals(dir) && path.StartsWith(dir));
        }

        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        private static IList<string> NormalizeDirectories(IEnumerable<string> paths)
        {
            var directories = paths
                .Select(path => path.EndsWith(DirectorySeparator) ? path : (path + DirectorySeparator))
                .ToList();

            for (int i = directories.Count - 1; i >= 0; --i)
            {
                if (IsContainsDirectory(directories[i], directories))
                {
                    directories.RemoveAt(i);
                }
            }

            return directories;
        }

        public static IList<string> FindNewFiles(IEnumerable<string> findDirectories, IEnumerable<string> excludeDirectories)
        {
            var finds = NormalizeDirectories(findDirectories);
            var excludes = NormalizeDirectories(excludeDirectories);

            var registeredFiles = new HashSet<string>(App.TrackManager.Tracks.Select(t => t.Path));

            var files = finds
                .SelectMany(path => GetFiles(path))
                .AsParallel()
                .Where(path => IsSupportedExtension(path) && !registeredFiles.Contains(path) && !IsContainsDirectory(path, excludes));

            var list = files.ToList();

            return list;
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
