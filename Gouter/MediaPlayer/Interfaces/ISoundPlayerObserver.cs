using System;
using System.Collections.Generic;
using System.Text;

namespace Gouter
{
    internal interface ISoundPlayerObserver : ICustomObserverObject
    {
        void OnPlayStateChanged(PlayState staet);
    }
}
