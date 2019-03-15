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
using Gouter.Extensions;

namespace Gouter
{
    internal class AlbumManager
    {
        public readonly static StringComparer AlbumNameComparer = StringComparer.CurrentCultureIgnoreCase;

        private volatile int _albumLatestIdx = -1;

        private readonly Dictionary<int, AlbumInfo> _albumIdMap = new Dictionary<int, AlbumInfo>();
        private readonly Dictionary<string, AlbumInfo> _albumKeyMap = new Dictionary<string, AlbumInfo>();

        public ConcurrentNotifiableCollection<AlbumInfo> Albums { get; } = new ConcurrentNotifiableCollection<AlbumInfo>();

        public AlbumManager()
        {
            this.Albums = new ConcurrentNotifiableCollection<AlbumInfo>();

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        public int GenerateId()
        {
            return ++_albumLatestIdx;
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
        }

        public void Register(AlbumInfo albumInfo)
        {
            this.AddImpl(albumInfo);

            using (var cmd = Database.CreateCommand())
            {
                var values = new Query
                {
                    ["id"] = albumInfo.Id,
                    ["key"] = albumInfo.Key,
                    ["name"] = albumInfo.Name,
                    ["artist"] = albumInfo.Artist,
                    ["is_compilation"] = albumInfo.IsCompilation,
                    ["artwork"] = albumInfo.ArtworkStream?.ToArray(),
                };

                Database.Insert(Database.TableNames.Albums, values);
            }
        }

        public AlbumInfo GetOrAddAlbum(Track track)
        {
            AlbumInfo albumInfo;

            var albumKey = GetAlbumKey(track);

            if (this._albumKeyMap.TryGetValue(albumKey, out albumInfo))
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

            foreach (var row in Database.Select(Database.TableNames.Albums))
            {
                int id = row.Get<int>(0);
                var key = row.Get<string>(1);
                var name = row.Get<string>(2);
                var artist = row.Get<string>(3);
                bool isCompilation = row.Get<bool>(4);
                byte[] artwork = row.GetOrDefault<byte[]>(5);

                var albumInfo = new AlbumInfo(id, key, name, artist, isCompilation, artwork);
                this.AddImpl(albumInfo);
            }

            if (this.Albums.Count > 0)
            {
                this._albumLatestIdx = this.Albums.Max(a => a.Id);
            }
        }
    }
}
