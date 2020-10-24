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

        /// <summary>
        /// 再生失敗時
        /// </summary>
        void OnPlayerFailed(Exception ex);

        /// <summary>
        /// トラックの再生終了時
        /// </summary>
        void OnTrackFinished();
    }
}
