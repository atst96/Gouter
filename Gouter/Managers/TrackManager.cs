using ATL;
using Gouter.Data;
using Gouter.DataModels;
using Gouter.Extensions;
using Gouter.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
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

        /// <summary>
        /// トラックIDを生成する
        /// </summary>
        /// <returns>新しいトラックID</returns>
        public int GenerateId() => ++this._latestTrackIdx;

        /// <summary>
        /// トラック情報を登録する。
        /// </summary>
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

        /// <summary>
        /// トラック情報を登録する。
        /// </summary>
        /// <param name="trackInfo">トラック情報</param>
        /// <returns>トラック情報</returns>
        public TrackInfo Add(TrackInfo trackInfo)
        {
            var dbContext = this._database.Context;
            dbContext.Tracks.Insert(new TrackDataModel
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
            });

            this.AddImpl(trackInfo);

            return trackInfo;
        }

        /// <summary>
        /// トラック情報を登録する。
        /// </summary>
        /// <param name="trackInfo">トラック情報</param>
        private void AddImpl(IEnumerable<TrackInfo> tracks)
        {
            foreach (var track in tracks)
            {
                if (this._trackIdMap.ContainsKey(track.Id))
                {
                    throw new InvalidOperationException("トラックIDが重複しています。");
                }

                this._trackIdMap.Add(track.Id, track);
            }

            this.Tracks.AddRange(tracks);
        }

        /// <summary>
        /// トラック情報を登録する。
        /// </summary>
        /// <param name="trackInfo">トラック情報</param>
        /// <returns>トラック情報</returns>
        public void Add(IEnumerable<TrackInfo> tracks)
        {
            var tracksData = tracks.Select(track => new TrackDataModel
            {
                Id = track.Id,
                AlbumId = track.AlbumInfo.Id,
                Path = track.Path,
                Duration = (int)track.Duration.TotalMilliseconds,
                Disk = track.DiskNumber,
                Track = track.TrackNumber,
                Year = track.Year,
                AlbumArtist = track.AlbumArtist,
                Title = track.Title,
                Artist = track.Artist,
                Genre = track.Genre,
                CreatedAt = track.RegisteredAt,
                UpdatedAt = track.UpdatedAt,
            });

            var dbContext = this._database.Context;
            dbContext.Tracks.InsertBulk(tracksData);

            this.AddImpl(tracks);
        }

        /// <summary>
        /// 未登録の楽曲情報を取得する。
        /// </summary>
        /// <param name="musicDirectories">音楽ファイルのディレクトリ</param>
        /// <param name="excludeDirectories">除外するディレクトリ</param>
        /// <param name="excludePaths"></param>
        /// <returns>楽曲情報リスト</returns>
        public IReadOnlyList<Track> GetUnregisteredTracks(
            IReadOnlyCollection<string> musicDirectories,
            IReadOnlyCollection<string> excludeDirectories,
            IReadOnlyCollection<string> excludeFilePaths)
        {
            // 登録済みファイル
            var registeredFilePaths = this.Tracks.Select(t => t.Path);

            var findDirs = PathUtil.ExcludeSubDirectories(musicDirectories);
            var excludeDirs = PathUtil.ExcludeSubDirectories(excludeDirectories);
            var excludePaths = excludeFilePaths ?? Array.Empty<string>();

            // 未登録ファイルを列挙する
            var unregisteredFiles = findDirs
                .SelectMany(path => PathUtil.GetFiles(path, true))
                .Except(excludeFilePaths ?? Array.Empty<string>())
                .Except(registeredFilePaths)
                .AsParallel()
                .Where(path =>
                        PathUtil.IsSupportedMediaExtension(path)
                        && !PathUtil.IsContains(path, excludeDirs))
                .ToList();

            var tracks = new List<Track>(unregisteredFiles.Count);

            foreach (var path in unregisteredFiles)
            {
                try
                {
                    tracks.Add(new Track(path));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"読み込み失敗: {path}, {ex}");
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

            var dbContext = this._database.Context;
            var results = dbContext.Tracks;

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
