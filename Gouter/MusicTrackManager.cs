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
