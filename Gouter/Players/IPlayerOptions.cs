using System;

namespace Gouter.Players
{
    /// <summary>
    /// プレーヤ設定のインタフェース
    /// </summary>
    internal interface IPlayerOptions
    {
        /// <summary>
        /// ループモードを設定する。
        /// </summary>
        public LoopMode LoopMode { get; }

        /// <summary>
        /// シャッフルモードを設定する。
        /// </summary>
        public ShuffleMode ShuffleMode { get; }

        /// <summary>
        /// シャッフル再生時の次トラック選択で現在のトラックを回避するかどうかを取得する。
        /// </summary>
        public bool IsShuffleAvoidCurrentTrack { get; }

        /// <summary>
        /// フェードイン／アウトを有効にするか取得する。
        /// </summary>
        public bool IsEnableFadeInOut { get; }

        /// <summary>
        /// フェードイン・アウトに要する時間を取得する。
        /// </summary>
        public TimeSpan FadeInOutDuration { get; }
    }
}
