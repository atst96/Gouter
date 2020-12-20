using System.ComponentModel;
using System.Windows.Data;
using Gouter.Components.Mvvm;

namespace Gouter.ViewModels
{
    /// <summary>
    /// アルバムのトラック情報リスト
    /// </summary>
    internal class AlbumTrackViewModel : NotificationObject
    {
        /// <summary>
        /// アルバム情報
        /// </summary>
        public AlbumPlaylist Playlist { get; }

        /// <summary>
        /// アルバム情報
        /// </summary>
        public AlbumInfo Album { get; }

        /// <summary>
        /// トラックリスト
        /// </summary>
        public NotifiableCollection<TrackInfo> Tracks { get; }

        /// <summary>
        /// </summary>
        public CollectionViewSource TrackViewSource { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playlist"></param>
        public AlbumTrackViewModel(AlbumPlaylist playlist)
        {
            this.Playlist = playlist;
            this.Album = playlist.Album;
            this.Tracks = playlist.Tracks;

            var tracks = this.Tracks;

            var trackViewSource = new CollectionViewSource
            {
                Source = tracks,
            };

            var groupDescriptions = trackViewSource.GroupDescriptions;
            var liveGroupingProperties = trackViewSource.LiveGroupingProperties;

            groupDescriptions.Add(new PropertyGroupDescription(nameof(TrackInfo.DiskNumber)));
            liveGroupingProperties.Add(nameof(TrackInfo.DiskNumber));

            var sortDescriptions = trackViewSource.SortDescriptions;
            var liveSortingDescriptions = trackViewSource.LiveSortingProperties;
            sortDescriptions.Add(new SortDescription(nameof(TrackInfo.DiskNumber), ListSortDirection.Ascending));
            sortDescriptions.Add(new SortDescription(nameof(TrackInfo.TrackNumber), ListSortDirection.Ascending));
            sortDescriptions.Add(new SortDescription(nameof(TrackInfo.Title), ListSortDirection.Ascending));
            liveSortingDescriptions.Add(nameof(TrackInfo.DiskNumber));
            liveSortingDescriptions.Add(nameof(TrackInfo.TrackNumber));
            liveSortingDescriptions.Add(nameof(TrackInfo.Title));

            this.TrackViewSource = trackViewSource;
        }

        private Command<TrackInfo> _trackPlayCommand;

        /// <summary>
        /// トラックのダブルクリック時のコマンド
        /// </summary>
        public Command<TrackInfo> TrackPlayCommand => this._trackPlayCommand
            ??= new DelegateCommand<TrackInfo>(this.OnPlayCommandExecute, track => track != null);

        private void OnPlayCommandExecute(TrackInfo track)
        {
            var player = App.Instance.MediaPlayer;

            player.Play(track, this.Playlist);
        }
    }
}
