using System;
using System.Runtime.CompilerServices;
using Gouter.Managers;
using NAudio.Wave;

namespace Gouter
{
    /// <summary>
    /// 音声出力デバイス
    /// </summary>
    internal abstract class AudioDevice : IDisposable
    {
        /// <summary>
        /// 再生停止イベント
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// 再生状態
        /// </summary>
        public abstract PlaybackState PlaybackState { get; }

        /// <summary>
        /// オーディオソースを設定する
        /// </summary>
        /// <param name="waveProvider">オーディオソース</param>
        public abstract void SetAudioSource(ISampleProvider waveProvider);

        /// <summary>
        /// インスタンスを破棄する
        /// </summary>
        protected abstract void Dispose();

        /// <summary>
        /// IDisposable実装
        /// </summary>
        void IDisposable.Dispose() => this.Dispose();

        /// <summary>
        /// RaisePlaybackStoppedイベントを発火させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RaisePlaybackStopped(object sender, StoppedEventArgs eventArgs)
        {
            this.PlaybackStopped?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 再生
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// 一時停止
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// 停止
        /// </summary>
        public abstract void Stop();

        protected static void Invoke(Action action)
        {
            ThreadManager.DeviceDispatcher.Invoke(action);
        }

        protected static T Invoke<T>(Func<T> action)
        {
            return ThreadManager.DeviceDispatcher.Invoke(action);
        }
    }
}
