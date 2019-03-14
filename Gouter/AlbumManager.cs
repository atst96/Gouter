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

namespace Gouter
{
    internal class AlbumManager
    {
        public readonly static StringComparer AlbumNameComparer = StringComparer.CurrentCultureIgnoreCase;

        private readonly Dictionary<int, AlbumInfo> _albumsIdImpl = new Dictionary<int, AlbumInfo>();
        private readonly Dictionary<string, AlbumInfo> _albumsKeyImpl = new Dictionary<string, AlbumInfo>(AlbumNameComparer);

        public ConcurrentNotifiableCollection<AlbumInfo> Albums { get; } = new ConcurrentNotifiableCollection<AlbumInfo>();

        public AlbumManager()
        {
            this.Albums = new ConcurrentNotifiableCollection<AlbumInfo>();

            BindingOperations.EnableCollectionSynchronization(this._albumsKeyImpl, new object());
            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        private static volatile int _albumLatestIdx = -1;

        public static void SetAlbumIndex(int id)
        {
            if (_albumLatestIdx >= 0)
            {
                throw new NotSupportedException();
            }

            _albumLatestIdx = id;
        }

        public static int GenerateId()
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

        public void Add(AlbumInfo albumInfo)
        {
            if (_albumsKeyImpl.ContainsKey(albumInfo.Key))
            {
                throw new NotSupportedException();
            }

            _albumsKeyImpl.Add(albumInfo.Key, albumInfo);

            this.Albums.Add(albumInfo);
        }

        public void Register(AlbumInfo albumInfo)
        {
            this.Add(albumInfo);
            

            using (var cmd = App.SqlConnection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO albums (id, key, name, artist, is_compilation, artwork) VALUES (@id,@key,@name,@artist,@compilation,@artwork)";

                var @params = cmd.Parameters;

                @params.AddWithValue("id", albumInfo.Id);
                @params.AddWithValue("key", albumInfo.Key);
                @params.AddWithValue("name", albumInfo.Name);
                @params.AddWithValue("artist", albumInfo.Artist);
                @params.AddWithValue("compilation", albumInfo.IsCompilation);
                @params.AddWithValue("artwork", albumInfo.ArtworkStream?.ToArray());

                cmd.Prepare();

                cmd.ExecuteNonQuery();
            }
        }

        public AlbumInfo GetOrAddAlbum(Track track)
        {
            var albumKey = GetAlbumKey(track);

            if (this._albumsKeyImpl.TryGetValue(albumKey, out var albumInfo))
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
    }
}
