using System;
using System.Collections.Generic;
using System.Text;

namespace Gouter
{
    internal interface ISoundPlayerObserver : ISubscribableObject
    {
        void OnPlayStateChanged(PlayState staet);
    }
}
