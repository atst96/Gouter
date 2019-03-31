using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gouter
{
    internal class AlbumPlaylist : IPlaylist
    {
        public AlbumPlaylist(AlbumInfo albumInfo)
        {
            this.Album = albumInfo;
            this.Tracks = new SortedNotifiableCollectionWrapper<TrackInfo>(albumInfo.Tracks, TrackNormalComparer.Instance);

            BindingOperations.EnableCollectionSynchronization(this.Tracks, new object());
        }

        public AlbumInfo Album { get; }

        public SortedNotifiableCollectionWrapper<TrackInfo> Tracks { get; }

        IReadOnlyList<TrackInfo> IPlaylist.Tracks => this.Tracks;
    }
}
