using ATL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal class TrackInfo : NotificationObject
    {
        private readonly static MusicTrackManager TrackManager = App.TrackManager;
        private readonly static AlbumManager AlbumManager = App.AlbumManager;

        public TrackInfo(Track track)
        {
            this.Id = TrackManager.GenerateId();
            this.Path = track.Path;
            this.Duration = TimeSpan.FromMilliseconds(track.DurationMs);
            this.DiskNumber = track.DiscNumber;
            this.TrackNumber = track.TrackNumber;
            this.Year = track.Year;
            this.AlbumArtist = track.AlbumArtist;
            this.Title = track.Title;
            this.Artist = track.Artist;
            this.Genre = track.Genre;
            this.AlbumInfo = AlbumManager.GetOrAddAlbum(track);
            this.AlbumInfo.Playlist.Tracks.Add(this);
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

            this.AlbumInfo = AlbumManager.FromId(albumId);
            this.AlbumInfo.Playlist.Tracks.Add(this);
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

        public bool IsPlaying { get; private set; }

        public void SetPlayState(bool isPlaying)
        {
            if (this.IsPlaying!=isPlaying)
            {
                this.IsPlaying = isPlaying;
                this.RaisePropertyChanged(nameof(this.IsPlaying));
            }
        }
    }
}
