using System;
using System.Collections.Generic;
using System.Text;

namespace Gouter
{
    /// <summary>
    /// サウンドプレーヤに通知Interface
    /// </summary>
    internal interface ISoundPlayerObserver : ISubscribableObject
    {
        void OnPlayStateChanged(PlayState staet);
    }
}
