using ATL;
using Gouter.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    /// <summary>
    /// トラック情報
    /// </summary>
    internal class TrackInfo : NotificationObject
    {
        /// <summary>トラック情報を生成する</summary>
        /// <param name="trackId">トラックID</param>
        /// <param name="track">トラック情報</param>
        /// <param name="albumInfo">アルバム情報</param>
        public TrackInfo(int trackId, Track track, AlbumInfo albumInfo)
        {
            this.Id = trackId;
            this.Path = track.Path;
            this.Duration = TimeSpan.FromMilliseconds(track.DurationMs);
            this.DiskNumber = track.DiscNumber;
            this.TrackNumber = track.TrackNumber;
            this.Year = track.Year;
            this.AlbumArtist = track.AlbumArtist;
            this.Title = track.Title;
            this.Artist = track.Artist;
            this.Genre = track.Genre;
            this.AlbumInfo = albumInfo;
            this.AlbumInfo.Playlist.Tracks.Add(this);
            this.RegisteredAt = DateTimeOffset.Now;
            this.UpdatedAt = this.RegisteredAt;
        }

        /// <summary>データベースからトラック情報を生成する</summary>
        /// <param name="dbItem">トラック情報</param>
        /// <param name="albumInfo">アルバム情報</param>
        public TrackInfo(TrackDataModel dbItem, AlbumInfo albumInfo)
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

            this.AlbumInfo = albumInfo;
            this.AlbumInfo.Playlist.Tracks.Add(this);

            this.RegisteredAt = dbItem.CreatedAt;
            this.UpdatedAt = dbItem.UpdatedAt;
        }

        /// <summary>アルバム情報</summary>
        public AlbumInfo AlbumInfo { get; private set; }

        /// <summary>トラックID</summary>
        public int Id { get; private set; }

        /// <summary>ファイルパス</summary>
        public string Path { get; private set; }

        /// <summary>尺</summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>ディスク番号</summary>
        public int DiskNumber { get; private set; }

        /// <summary>トラック番号</summary>
        public int TrackNumber { get; private set; }

        /// <summary>発売年</summary>
        public int Year { get; private set; }

        /// <summary>アルバムアーティスト</summary>
        public string AlbumArtist { get; private set; }

        /// <summary>タイトル</summary>
        public string Title { get; private set; }

        /// <summary>アーティスト名</summary>
        public string Artist { get; private set; }

        /// <summary>ジャンル</summary>
        public string Genre { get; private set; }

        /// <summary>トラック情報の登録日</summary>
        public DateTimeOffset RegisteredAt { get; }
        
        /// <summary>トラック情報の更新日</summary>
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>再生中かどうかを取得する</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>トラックの再生状態を設定する</summary>
        /// <param name="isPlaying">再生状態</param>
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
