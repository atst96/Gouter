using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Gouter.Managers
{
    internal class ArtworkManager
    {
        private string _dirPath;

        public ArtworkManager(string path)
        {
            this._dirPath = path;

            if (!Directory.Exists(path))
            {
                // ディレクトリがなければ生成する
                Directory.CreateDirectory(path);
            }
        }

        public void Add(AlbumInfo album, byte[] artworks)
        {
            var path = this.GetPath(album);

            using var file = File.Open(path, FileMode.CreateNew);
            file.Write(artworks, 0, artworks.Length);
        }

        public byte[] GetBytes(AlbumInfo album)
        {
            var path = this.GetPath(album);

            try
            {
                return File.ReadAllBytes(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public Stream GetStream(AlbumInfo album)
        {
            var path = this.GetPath(album);

            try
            {
                return File.OpenRead(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private string GetPath(AlbumInfo album)
        {
            var fileName = album.ArtworkId + ".gaw";
            return Path.Combine(this._dirPath, fileName);
        }
    }
}
