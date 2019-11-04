using System;
using System.Collections.Generic;
using System.Text;

namespace Gouter
{
    /// <summary>
    /// アルバムの通知Interface
    /// </summary>
    internal interface IAlbumObserver : ISubscribableObject
    {
        void OnRegistered(AlbumInfo albumInfo);
        void OnRemoved(AlbumInfo albumInfo);
    }
}
