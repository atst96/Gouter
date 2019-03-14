using ATL;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    [MessagePackObject]
    internal class TrackInfo : NotificationObject
    {
        public TrackInfo(Track track)
        {
            this.Id = MusicTrackManager.GenerateId();
            this.Path = track.Path;
            this.Duration = TimeSpan.FromMilliseconds(track.DurationMs);
            this.DiskNumber = track.DiscNumber;
            this.TrackNumber = track.TrackNumber;
            this.Year = track.Year;
            this.AlbumArtist = track.AlbumArtist;
            this.Title = track.Title;
            this.Artist = track.Artist;
            this.Genre = track.Genre;
            this.AlbumInfo = App.AlbumManager.GetOrAddAlbum(track);
            this.AlbumInfo.Tracks.Add(this);
        }

        public TrackInfo(int id, int albumId, string path, int duration, int disk, int track, int year, string albumArtist, string title, string artist, string genre)
        {
            this.Id = id;
            this.Path = path;
            this.Duration = TimeSpan.FromMilliseconds(duration);
            this.DiskNumber = disk;
            this.TrackNumber = track;
            this.Year = year;
            this.AlbumArtist = albumArtist;
            this.Title = title;
            this.Artist = artist;
            this.Genre = genre;

            this.AlbumInfo = App.AlbumManager.FromId(albumId);
            this.AlbumInfo.Tracks.Add(this);
        }

        public AlbumInfo AlbumInfo { get; private set; }

        public int Id { get; private set; }

        public string Path { get; private set; }

        public TimeSpan Duration { get; private set; }

        public int DiskNumber { get; private set; }

        public int TrackNumber { get; private set; }

        public int Year { get; private set; }

        public string AlbumArtist { get; private set; }

        public string Title { get; private set; }

        public string Artist { get; private set; }

        public string Genre { get; private set; }
    }
}
