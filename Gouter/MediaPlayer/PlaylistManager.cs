using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal class PlaylistManager
    {
        public PlaylistManager()
        {
        }

        public NotifiableCollection<AlbumPlaylist> Albums { get; } = new NotifiableCollection<AlbumPlaylist>();
    }
}
