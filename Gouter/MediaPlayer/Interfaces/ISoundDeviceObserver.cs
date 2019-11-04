using System;
using System.Collections.Generic;
using System.Text;

namespace Gouter
{
    /// <summary>
    /// サウンドデバイスの通知Interface
    /// </summary>
    internal interface ISoundDeviceObserver : ISubscribableObject
    {
        void OnDefaultDeviceChanged(SoundDeviceInfo deviceInfo);
        void OnDeviceAdded(SoundDeviceInfo deviceInfo);
        void OnDeviceRemoved(SoundDeviceInfo deviceInfo);
        void OnDeviceStateChanged(SoundDeviceInfo deviceInfo);
        void OnPropertyValueChanged(SoundDeviceInfo deviceInfo);
    }
}
