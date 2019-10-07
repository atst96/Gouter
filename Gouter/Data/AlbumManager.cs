using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using ATL;
using Gouter.DataModels;
using Gouter.Extensions;

namespace Gouter
{
    internal class AlbumManager
    {
        private readonly Database _database;

        public static readonly StringComparer AlbumNameComparer = StringComparer.CurrentCultureIgnoreCase;

        private volatile int _albumLatestIdx = -1;

        private readonly Dictionary<int, AlbumInfo> _albumIdMap = new Dictionary<int, AlbumInfo>();
        private readonly Dictionary<string, AlbumInfo> _albumKeyMap = new Dictionary<string, AlbumInfo>();

        public ConcurrentNotifiableCollection<AlbumInfo> Albums { get; } = new ConcurrentNotifiableCollection<AlbumInfo>();

        public AlbumManager(Database database)
        {
            this._database = database ?? throw new InvalidOperationException();

            this.Albums = new ConcurrentNotifiableCollection<AlbumInfo>();

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        public int GenerateId()
        {
            return ++this._albumLatestIdx;
        }

        public static string GetAlbumKey(Track track)
        {
            string albumName = track.Album;
            string albumArtist = GetAlbumArtist(track, "unknown", "###compilation###");

            return $"--#name={{{albumName}}};\n--#artist={{{albumArtist}}};";
        }

        internal static string GetAlbumArtist(Track track, string unknownValue = "Unknown", string compilationValue = "Various Artists")
        {
            if (track.AdditionalFields.TryGetValue("cpil", out var cpil) && string.Equals(cpil, "1"))
            {
                return compilationValue;
            }

            if (!string.IsNullOrEmpty(track.AlbumArtist))
            {
                return track.AlbumArtist;
            }

            if (string.IsNullOrEmpty(track.Artist))
            {
                return unknownValue;
            }

            return track.Artist;
        }

        private void AddImpl(AlbumInfo albumInfo)
        {
            if (this._albumKeyMap.ContainsKey(albumInfo.Key))
            {
                throw new InvalidOperationException("アルバムキーが重複しています。");
            }

            if (this._albumIdMap.ContainsKey(albumInfo.Id))
            {
                throw new InvalidOperationException("アルバムIDが重複しています。");
            }

            this._albumIdMap.Add(albumInfo.Id, albumInfo);
            this._albumKeyMap.Add(albumInfo.Key, albumInfo);

            this.Albums.Add(albumInfo);
            App.PlaylistManager.Albums.Add(albumInfo.Playlist);
        }

        public void Register(AlbumInfo albumInfo)
        {
            this.AddImpl(albumInfo);

            var dataModel = new AlbumDataModel
            {
                Id = albumInfo.Id,
                Key = albumInfo.Key,
                Name = albumInfo.Name,
                Artist = albumInfo.Artist,
                IsCompilation = albumInfo.IsCompilation,
                Artwork = albumInfo.ArtworkStream?.ToArray(),
                CreatedAt = albumInfo.RegisteredAt,
                UpdatedAt = albumInfo.UpdatedAt,
            };
            dataModel.Insert(this._database);
        }

        public AlbumInfo GetOrAddAlbum(Track track)
        {
            var albumKey = GetAlbumKey(track);

            if (this._albumKeyMap.TryGetValue(albumKey, out var albumInfo))
            {
                return albumInfo;
            }

            albumInfo = new AlbumInfo(albumKey, track);

            this.Register(albumInfo);

            return albumInfo;
        }

        public AlbumInfo FromId(int albumId)
        {
            return this.Albums.First(album => album.Id == albumId);
        }

        public void LoadDatabase()
        {
            if (this.Albums.Count > 0)
            {
                throw new InvalidOperationException();
            }

            var results = AlbumDataModel.GetAll(this._database);

            foreach(var result in results)
            {
                var albumInfo = new AlbumInfo(result);
                this.AddImpl(albumInfo);
            }

            if (this.Albums.Count > 0)
            {
                this._albumLatestIdx = this.Albums.Max(a => a.Id);
            }
        }
    }
}
