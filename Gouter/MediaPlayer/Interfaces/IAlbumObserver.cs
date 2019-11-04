using System;
using System.Collections.Generic;
using System.Text;

namespace Gouter
{
    internal interface IAlbumObserver : ISubscribableObject
    {
        void OnRegistered(AlbumInfo albumInfo);
        void OnRemoved(AlbumInfo albumInfo);
    }
}
