using ATL;
using Gouter.DataModels;
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
    /// <summary>
    /// トラック情報の管理を行う
    /// </summary>
    internal class TrackManager
    {
        /// <summary>データベース</summary>
        private readonly Database _database;

        /// <summary>トラックの最終ID</summary>
        private volatile int _latestTrackIdx = -1;

        /// <summary>トラックIDとトラック情報が対応したマップ</summary>
        private readonly IDictionary<int, TrackInfo> _trackIdMap;

        /// <summary>トラック一覧</summary>
        public ConcurrentNotifiableCollection<TrackInfo> Tracks { get; }

        /// <summary>TrackManagerを生成する</summary>
        /// <param name="database">データベース</param>
        public TrackManager(Database database)
        {
            this._database = database ?? throw new InvalidOperationException();

            this._trackIdMap = new Dictionary<int, TrackInfo>();
            this.Tracks = new ConcurrentNotifiableCollection<TrackInfo>();

            BindingOperations.EnableCollectionSynchronization(this.Tracks, new object());
        }

        /// <summary>再帰的にファイル一覧を取得する</summary>
        /// <param name="directory">ディレクトリパス</param>
        /// <returns>ファイル一覧</returns>
        private static IEnumerable<string> GetFiles(string directory)
        {
            return Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);
        }

        /// <summary>トラックIDを生成する</summary>
        /// <returns></returns>
        public int GenerateId()
        {
            return ++this._latestTrackIdx;
        }

        /// <summary>検索するファイルの拡張子</summary>
        public static readonly HashSet<string> SupportedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".wav", ".mp3", ".acc", ".m4a", ".flac", ".ogg"
        };

        /// <summary>対応するメディアの拡張子かどうかを判定する</summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>対応メディア</returns>
        public static bool IsSupportedExtension(string path)
        {
            var extension = Path.GetExtension(path);

            return SupportedExtensions.Contains(extension);
        }

        /// <summary>トラック情報を登録する</summary>
        /// <param name="trackInfo">トラック情報</param>
        private void AddImpl(TrackInfo trackInfo)
        {
            if (this._trackIdMap.ContainsKey(trackInfo.Id))
            {
                throw new InvalidOperationException("トラックIDが重複しています。");
            }

            this._trackIdMap.Add(trackInfo.Id, trackInfo);
            this.Tracks.Add(trackInfo);
        }

        /// <summary>トラック情報を登録する</summary>
        /// <param name="trackInfo">トラック情報</param>
        /// <returns>トラック情報</returns>
        public TrackInfo Add(TrackInfo trackInfo)
        {
            var dataModel = new TrackDataModel
            {
                Id = trackInfo.Id,
                AlbumId = trackInfo.AlbumInfo.Id,
                Path = trackInfo.Path,
                Duration = (int)trackInfo.Duration.TotalMilliseconds,
                Disk = trackInfo.DiskNumber,
                Track = trackInfo.TrackNumber,
                Year = trackInfo.Year,
                AlbumArtist = trackInfo.AlbumArtist,
                Title = trackInfo.Title,
                Artist = trackInfo.Artist,
                Genre = trackInfo.Genre,
                CreatedAt = trackInfo.RegisteredAt,
                UpdatedAt = trackInfo.UpdatedAt,
            };
            dataModel.Insert(this._database);

            this.AddImpl(trackInfo);
            
            return trackInfo;
        }

        /// <summary>再帰検索の際にディレクトリが重複しないかを検証する</summary>
        /// <param name="path">検証パス</param>
        /// <param name="directories">ディレクトリ一覧</param>
        /// <returns>ディレクトリの重複有無</returns>
        private static bool IsContainsDirectory(string path, IEnumerable<string> directories)
        {
            return directories.Any(dir => !path.Equals(dir) && path.StartsWith(dir));
        }

        /// <summary>ディレクトリセパレータ</summary>
        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        /// <summary>ディレクトリパスを正規化する</summary>
        /// <param name="paths">ディレクトリパス一覧</param>
        /// <returns>正規化済みディレクトリ一覧</returns>
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

        /// <summary>新規ファイルを列挙する</summary>
        /// <param name="findDirectories"></param>
        /// <param name="excludeDirectories"></param>
        /// <returns></returns>
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

        /// <summary>ファイル一覧からトラック情報を取得する</summary>
        /// <param name="files"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static IList<Track> GetTracks(IList<string> files, IProgress<int> progress = null)
        {
            var tracks = new List<Track>(files.Count);

            int count = 0;

            foreach (var path in files)
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

        /// <summary>データベースから読み込む</summary>
        /// <param name="albumManager"></param>
        public void LoadDatabase(AlbumManager albumManager)
        {
            if (this.Tracks.Count > 0)
            {
                // トラック情報が1件でも登録されている場合は操作を受け付けない
                throw new InvalidOperationException();
            }

            var results = TrackDataModel.GetAll(this._database);

            foreach (var result in results)
            {
                var trackInfo = new TrackInfo(result, albumManager.FromId(result.AlbumId));
                this.AddImpl(trackInfo);
            }

            if (this.Tracks.Count > 0)
            {
                this._latestTrackIdx = this.Tracks.Max(t => t.Id);
            }
        }
    }
}
