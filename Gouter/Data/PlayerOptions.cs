using System;
using System.Windows.Media.Animation;
using Gouter.Players;

namespace Gouter.Data
{
    internal class PlayerOptions : NotificationObject, IPlayerOptions
    {
        private LoopMode _loopMode;

        /// <summary>
        /// ループモードを取得または設定する。
        /// </summary>
        public LoopMode LoopMode
        {
            get => this._loopMode;
            set => this.SetProperty(ref this._loopMode, value);
        }

        private ShuffleMode _shuffleMode;

        /// <summary>
        /// シャッフルモードを取得または設定する。
        /// </summary>
        public ShuffleMode ShuffleMode
        {
            get => this._shuffleMode;
            set => this.SetProperty(ref this._shuffleMode, value);
        }

        private bool _isShuffleAvoidCurrentTrack;

        /// <summary>
        /// シャッフル再生時の次トラック選択で現在のトラックを回避するかどうかを取得または設定する。
        /// </summary>
        public bool IsShuffleAvoidCurrentTrack
        {
            get => this._isShuffleAvoidCurrentTrack;
            set => this.SetProperty(ref this._isShuffleAvoidCurrentTrack, value);
        }

        private bool _isEnableFadeInOut;

        /// <summary>
        /// フェードイン／アウトを有効にするかを取得または設定する。
        /// </summary>
        public bool IsEnableFadeInOut
        {
            get => this._isEnableFadeInOut;
            set => this.SetProperty(ref this._isEnableFadeInOut, value);
        }

        private TimeSpan _fadeInOutDuration;

        /// <summary>
        /// フェードイン／アウトに要する時間を取得または設定する。
        /// </summary>
        public TimeSpan FadeInOutDuration
        {
            get => this._fadeInOutDuration;
            set => this.SetProperty(ref this._fadeInOutDuration, value);
        }
    }
}
