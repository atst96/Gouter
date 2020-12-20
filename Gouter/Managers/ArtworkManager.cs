using System;
using System.Collections.Generic;
using System.IO;

namespace Gouter.Managers
{
    internal class ArtworkManager
    {
        private string _dirPath;

        private object _lockObj = new();
        private Dictionary<string, WeakReference<byte[]>> _artworkReferences = new();

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
            lock (this._lockObj)
            {
                var path = this.GetPath(album);
                var albumId = album.ArtworkId;

                try
                {
                    byte[] data;
                    WeakReference<byte[]> weakReference = null;

                    if (this._artworkReferences.TryGetValue(albumId, out weakReference)
                        && weakReference.TryGetTarget(out data))
                    {
                        return data;
                    }

                    data = File.ReadAllBytes(path);

                    if (weakReference == null)
                    {
                        weakReference = new WeakReference<byte[]>(data);
                        this._artworkReferences.Add(albumId, weakReference);
                    }
                    else
                    {
                        weakReference.SetTarget(data);
                    }

                    return data;
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }
        }

        public Stream GetStream(AlbumInfo album)
        {
            try
            {
                return new MemoryStream(this.GetBytes(album));
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
