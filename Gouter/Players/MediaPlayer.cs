using System;
using System.Collections.Generic;
using System.Text;
using CSCore.CoreAudioAPI;
using Gouter.Managers;

namespace Gouter.Players
{
    /// <summary>
    /// メディア再生管理を行うクラス
    /// </summary>
    internal class MediaPlayer : NotificationObject
    {
        /// <summary>
        /// メディア管理クラス
        /// </summary>
        public MediaManager MediaManager { get; }

        private readonly SoundPlayer _player = new SoundPlayer();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mediaManager">メディア管理クラス</param>
        public MediaPlayer(MediaManager mediaManager) : base()
        {
            this.MediaManager = mediaManager;

            var soundOut = new CSCore.SoundOut.WasapiOut(false, AudioClientShareMode.Shared, 1)
            {
                Device = App.Instance.SoundDeviceListener.SystemDefault.GetDevice(),
            };

            this._player.SetSoundDevice(soundOut);
        }

        private bool _isPlaying;

        /// <summary>
        /// 再生中かどうかのフラグ
        /// </summary>
        public bool IsPlaying
        {
            get => this._isPlaying;
            private set => this.SetProperty(ref this._isPlaying, value);
        }

        // TODO: SoundPlayerの再生処理をMediaPlayerに移植する
        /// <summary>
        /// 内部プレーヤ
        /// </summary>
        [Obsolete]
        public SoundPlayer InternalPlayer => this._player;
    }
}
