using ATL;
using Gouter.DataModels;
using Gouter.Extensions;
using Gouter.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gouter.Managers
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

        /// <summary>トラックIDを生成する</summary>
        /// <returns></returns>
        public int GenerateId()
        {
            return ++this._latestTrackIdx;
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

        /// <summary>
        /// 未登録の楽曲情報を取得する。
        /// </summary>
        /// <param name="musicDirectories">音楽ファイルのディレクトリリスト</param>
        /// <param name="excludeDirectories">除外するディレクトリリスト</param>
        /// <returns>楽曲情報リスト</returns>
        public IReadOnlyList<Track> GetUnregisteredTracks(IReadOnlyCollection<string> musicDirectories, IReadOnlyCollection<string> excludeDirectories)
        {
            var registeredFiles = this.Tracks.Select(t => t.Path).ToImmutableHashSet();

            var findDirs = PathUtil.NormalizeDirectories(musicDirectories);
            var excludeDirs = PathUtil.NormalizeDirectories(excludeDirectories);

            var foundFiles = findDirs
                .SelectMany(path => PathUtil.GetFiles(path, true))
                .AsParallel()
                .Where(path =>
                    PathUtil.IsSupportedMediaExtension(path)
                        && !registeredFiles.Contains(path)
                        && !PathUtil.IsContainsDirectory(path, excludeDirs));

            var tracks = new List<Track>(foundFiles.Count());

            foreach (var path in foundFiles)
            {
                try
                {
                    tracks.Add(new Track(path));
                }
                catch
                {
                    Debug.WriteLine($"読み込み失敗: {path}");
                }
            }

            return tracks;
        }

        /// <summary>データベースから読み込む</summary>
        /// <param name="albumManager"></param>
        public void LoadLibrary(AlbumManager albumManager)
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
