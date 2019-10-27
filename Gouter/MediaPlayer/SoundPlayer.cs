using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;

namespace Gouter
{
    internal class SoundPlayer : IDisposable
    {
        private IWaveSource _soundSource;
        private ISoundOut _soundDevice;

        public double Duration { get; private set; }
        public PlayState State { get; private set; } = PlayState.Stop;
        public TrackInfo PlayTrack { get; private set; }
        private volatile bool _isStopRequested = false;

        public event EventHandler<PlayerStateEventArgs> PlayerEventChanged;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SoundPlayer()
        {
        }

        /// <summary>
        /// 再生状態の設定と通知を行う。
        /// </summary>
        /// <param name="state">再生状態</param>
        private void OnStateChanged(PlayState state)
        {
            this.State = state;
            this.PlayerEventChanged?.Invoke(this, new PlayerStateEventArgs(state));
        }

        /// <summary>
        /// 音源の初期化処理を行う。
        /// </summary>
        /// <param name="path">音声ファイルのフルパス</param>
        private void SetSoundSource(string path)
        {
            var soundSource = CodecFactory.Instance.GetCodec(path);

            this._soundSource = soundSource;

            this._soundDevice.Initialize(soundSource);
        }

        /// <summary>
        /// デバイスの再生処理停止イベントが呼び出された。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSoundDevicePlayStopped(object sender, PlaybackStoppedEventArgs e)
        {
            this._isStopRequested = false;
            this.OnStateChanged(PlayState.Stop);
        }

        /// <summary>
        /// 出力デバイスを設定する。
        /// </summary>
        /// <param name="soundDevice"></param>
        public void SetSoundDevice(ISoundOut soundDevice)
        {
            // 再生停止状態でなければ操作を受け付けない。
            if (this.State != PlayState.Stop && !this._isStopRequested)
            {
                throw new InvalidOperationException();
            }

            // 再生音源のリソースを解放する。
            if (this._soundSource != null)
            {
                this._soundSource.Dispose();
            }

            // サウンドデバイスのイベント購読を解除する。
            if (this._soundDevice != null)
            {
                this._soundDevice.Stopped -= this.OnSoundDevicePlayStopped;
            }

            // 新規サウンドデバイスを設定する。
            this._soundDevice = soundDevice;
            this._soundDevice.Stopped += this.OnSoundDevicePlayStopped;
        }

        /// <summary>
        /// 再生を開始する。
        /// </summary>
        /// <returns>Task</returns>
        public async Task Play()
        {
            if (this.State == PlayState.Play)
            {
                return;
            }

            // デバイスと再生音源が設定されていない場合は操作を受け付けない。
            if (this._soundDevice == null || this._soundSource == null)
            {
                throw new InvalidOperationException();
            }

            // デバイスの再生停止処理が終了していない場合は待機する。
            if (this._isStopRequested)
            {
                await Task.Run(() => this._soundDevice.WaitForStopped())
                    .ConfigureAwait(false);
            }

            // 再生処理を開始する。
            this._soundDevice.Play();
        }

        /// <summary>
        /// 再生を一時停止する。
        /// </summary>
        public void Pause()
        {
            if (this.State == PlayState.Pause || this.State == PlayState.Stop)
            {
                return;
            }

            if (this._soundSource != null)
            {
                this._soundDevice.Pause();
                this.OnStateChanged(PlayState.Pause);
            }
        }

        /// <summary>
        /// 再生を停止する。デバイスの再生処理終了の待機は行わない。
        /// </summary>
        public void Stop()
        {
            if (this._soundDevice == null || this.State == PlayState.Stop)
            {
                return;
            }

            this._isStopRequested = true;
            this._soundDevice.Stop();
        }

        /// <summary>
        /// 再生を停止し、デバイスの再生処理終了を待機する。
        /// </summary>
        /// <returns></returns>
        public async Task StopWithWaitInternalPlayer()
        {
            if (this._soundDevice == null || this.State == PlayState.Stop)
            {
                return;
            }

            this.Stop();

            await Task.Run(() => this._soundDevice.WaitForStopped())
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 再生トラックを指定する。
        /// </summary>
        /// <param name="track"></param>
        public void SetTrack(TrackInfo track)
        {
            if (this.State != PlayState.Stop)
            {
                throw new InvalidOperationException();
            }

            this.PlayTrack = track;
            this.SetSoundSource(track.Path);
        }

        /// <summary>
        /// 再生位置を取得する。
        /// </summary>
        /// <returns>再生位置</returns>
        public TimeSpan GetPosition()
        {
            return this._soundSource.GetPosition();
        }

        /// <summary>
        /// 再生位置を設定する。
        /// </summary>
        /// <param name="position">再生位置</param>
        public void SetPosition(TimeSpan position)
        {
            this._soundSource?.SetPosition(position);
        }

        /// <summary>
        /// リソース解放を行う。
        /// </summary>
        public void Dispose()
        {
            this._soundSource?.Dispose();
        }
    }
}
