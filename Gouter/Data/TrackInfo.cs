using ATL;
using Gouter.DataModels;
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
            this.RegisteredAt = DateTimeOffset.Now;
            this.UpdatedAt = this.RegisteredAt;
        }

        public TrackInfo(TrackDataModel dbItem)
        {
            this.Id = dbItem.Id;
            this.Path = dbItem.Path;
            this.Duration = TimeSpan.FromMilliseconds(dbItem.Duration);
            this.DiskNumber = dbItem.Disk ?? -1;
            this.TrackNumber = dbItem.Track ?? -1;
            this.Year = dbItem.Year ?? -1;
            this.AlbumArtist = dbItem.AlbumArtist;
            this.Title = dbItem.Title;
            this.Artist = dbItem.Artist;
            this.Genre = dbItem.Genre;

            this.AlbumInfo = AlbumManager.FromId(dbItem.AlbumId);
            this.AlbumInfo.Playlist.Tracks.Add(this);

            this.RegisteredAt = dbItem.CreatedAt;
            this.UpdatedAt = dbItem.UpdatedAt;
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
        public DateTimeOffset RegisteredAt { get; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public bool IsPlaying { get; private set; }

        public void SetPlayState(bool isPlaying)
        {
            if (this.IsPlaying != isPlaying)
            {
                this.IsPlaying = isPlaying;
                this.RaisePropertyChanged(nameof(this.IsPlaying));
            }
        }
    }
}
