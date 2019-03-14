using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    [MessagePackObject]
    internal class Library
    {
        public AlbumInfo[] Albums { get; private set; }
        public TrackInfo[] Tracks { get; private set; }
    }
}
