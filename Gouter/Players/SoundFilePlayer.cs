using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;

namespace Gouter.Players
{
    /// <summary>
    /// 音声ファイルの再生処理を行うクラス
    /// </summary>
    internal class SoundFilePlayer : IDisposable, ISubscribable<ISoundPlayerObserver>
    {
        // フェード終了後の再生状態
        private PlayState? _afterFadeState = null;

        // 音源
        private ISampleSource _inputSource;
        private FadeInOut _fadeInOut;
        private Equalizer _equalizer;
        private IWaveSource _outputSource;
        private float _volume;

        // サウンドデバイス
        private ISoundOut _soundDevice;

        // フェードの最大／最小ボリューム
        private const float FadeMaxVolume = 1.0f;
        private const float FadeMinVolume = 0.0f;

        // 再生処理停止要求フラグ
        private volatile bool _isStopRequested = false;

        // 再生スレッドのディスパッチャ
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        /// <summary>
        /// フェードイン／アウトにかける時間を取得または設定する。
        /// </summary>
        public TimeSpan? FadeInOutDuration { get; set; }

        /// <summary>
        /// 再生状態を取得する。
        /// </summary>
        public PlayState State { get; private set; } = PlayState.Stop;

        /// <summary>
        /// ボリュームを取得または設定する。
        /// </summary>
        public float Volume
        {
            get => this._volume;
            set
            {
                this._volume = value;

                // 出力デバイスのボリュームを変更
                var device = this._soundDevice;
                if (device != null)
                {
                    device.Volume = value;
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SoundFilePlayer()
        {
        }

        /// <summary>
        /// 再生状態の設定と通知を行う
        /// </summary>
        /// <param name="state">再生状態</param>
        private void OnStateChanged(PlayState state)
        {
            this.State = state;
            this._observers.NotifyAll(observer => observer.OnPlayStateChanged(state));
        }

        /// <summary>
        /// 音源の初期化処理を行う
        /// </summary>
        /// <param name="path">音声ファイルのフルパス</param>
        public void ChangeSoundSource(string path)
        {
            if (this.State != PlayState.Stop)
            {
                throw new InvalidOperationException();
            }

            this.ReleaseAudioSources();

            this._inputSource = GetSoundSource(path).ToSampleSource();
            this._equalizer = Equalizer.Create10BandEqualizer(this._inputSource);
            this._fadeInOut = new FadeInOut(this._equalizer)
            {
                FadeStrategy = new LinearFadeStrategy(),
            };
            this._outputSource = this._fadeInOut.ToWaveSource();

            this._fadeInOut.FadeStrategy.FadingFinished += this.OnFadeInOutFinished;
            this._soundDevice.Initialize(this._outputSource);
        }

        private static IWaveSource GetSoundSource(string path)
            => CodecFactory.Instance.GetCodec(path);

        /// <summary>
        /// デバイスの再生処理停止イベントが呼び出された
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSoundDevicePlayStopped(object sender, PlaybackStoppedEventArgs e)
        {
            this._isStopRequested = false;
            this.OnStateChanged(PlayState.Stop);
        }

        /// <summary>
        /// 出力デバイスを設定する
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
            if (this._inputSource != null)
            {
                this._inputSource.Dispose();
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
        /// 再生を開始する
        /// </summary>
        /// <param name="enableFadeIn">フェ＝ドインを行うかどうかのフラグ</param>
        public async Task Play(bool enableFadeIn = true)
        {
            if (this.State == PlayState.Play)
            {
                return;
            }

            if (this.State == PlayState.Pause)
            {
                if (enableFadeIn && this.GetIsFadeInEnable())
                {
                    this.FadeIn();
                }

                this._soundDevice.Play();
                this.OnStateChanged(PlayState.Play);
                return;
            }

            // デバイスと再生音源が設定されていない場合は操作を受け付けない。
            if (this._soundDevice == null || this._inputSource == null)
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
            this.OnStateChanged(PlayState.Play);
        }

        /// <summary>
        /// 再生を一時停止する
        /// </summary>
        /// <param name="isFadeOutEnable">フェードアウトを行うかどうかのフラグ</param>
        public void Pause(bool isFadeOutEnable = true)
        {
            var playState = this.State;

            if (playState != PlayState.Play)
            {
                return;
            }

            // 再生状態を更新
            this.OnStateChanged(PlayState.Pause);

            if (this._inputSource == null)
            {
                return;
            }

            if (isFadeOutEnable && this.State == PlayState.Pause && this.GetIsFadeOutEnable())
            {
                this.FadeOut(PlayState.Pause);
            }
            else
            {
                this._soundDevice.Pause();
            }
        }

        /// <summary>
        /// 再生を停止する。デバイスの再生処理終了の待機は行わない
        /// </summary>
        /// <param name="isFadeOutEnable">フェードアウトを行うかどうかのフラグ</param>
        public void Stop(bool isFadeOutEnable = true)
        {
            if (this._soundDevice == null || this.State == PlayState.Stop)
            {
                return;
            }

            this._isStopRequested = true;

            if (isFadeOutEnable && this.State == PlayState.Play && this.GetIsFadeOutEnable())
            {
                this.FadeOut(PlayState.Stop);
            }
            else
            {
                this._soundDevice.Stop();
            }
        }

        /// <summary>
        /// 再生を停止し、デバイスの再生処理終了を待機する
        /// </summary>
        /// <returns></returns>
        public async ValueTask StopAndWait()
        {
            if (this._soundDevice == null || this.State == PlayState.Stop)
            {
                return;
            }

            this.Stop();

            await this._dispatcher.InvokeAsync(() => this._soundDevice.WaitForStopped());
        }

        /// <summary>
        /// フェードイン処理が可能かどうかを取得する。
        /// </summary>
        /// <returns></returns>
        private bool GetIsFadeOutEnable()
            => this.FadeInOutDuration > TimeSpan.Zero;

        /// <summary>
        /// フェードアウト処理が可能かどうかを取得する。
        /// </summary>
        /// <returns></returns>
        private bool GetIsFadeInEnable()
            => this.FadeInOutDuration > TimeSpan.Zero;

        /// <summary>
        /// フェードインする。
        /// </summary>
        private void FadeIn(PlayState? afterState = null)
        {
            this._afterFadeState = afterState;
            var fadeInDuration = this.FadeInOutDuration ?? TimeSpan.Zero;

            this._fadeInOut.FadeStrategy.StartFading(null, FadeMaxVolume, fadeInDuration);
        }

        /// <summary>
        /// フェードアウトする。
        /// </summary>
        private void FadeOut(PlayState? afterState = null)
        {
            this._afterFadeState = afterState;
            var fadeOutDuration = this.FadeInOutDuration ?? TimeSpan.Zero;

            this._fadeInOut.FadeStrategy.StartFading(null, FadeMinVolume, fadeOutDuration);
        }

        /// <summary>
        /// フェードイン／アウト終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnFadeInOutFinished(object sender, EventArgs e) => await this._dispatcher.InvokeAsync(() =>
        {
            switch (this._afterFadeState)
            {
                case PlayState.Pause:
                    this._soundDevice.Pause();
                    break;

                case PlayState.Stop:
                    this._soundDevice.Stop();
                    break;
            }
        });

        /// <summary>
        /// 再生位置を取得する
        /// </summary>
        /// <returns>再生位置</returns>
        public TimeSpan GetPosition()
        {
            return this._inputSource.GetPosition();
        }

        /// <summary>
        /// 再生位置を設定する
        /// </summary>
        /// <param name="position">再生位置</param>
        public void SetPosition(TimeSpan position)
        {
            this._inputSource?.SetPosition(position);
        }

        private readonly List<ISoundPlayerObserver> _observers = new List<ISoundPlayerObserver>();

        /// <summary>
        /// 通知オブジェクトを登録する
        /// </summary>
        /// <param name="observer">通知オブジェクト</param>
        public void Subscribe(ISoundPlayerObserver observer)
        {
            if (!this._observers.Contains(observer))
            {
                this._observers.Add(observer);
            }
        }

        /// <summary>
        /// 通知オブジェクトを登録解除する
        /// </summary>
        /// <param name="observer">通知オブジェクト</param>
        public void Describe(ISoundPlayerObserver observer)
        {
            this._observers.Remove(observer);
        }

        /// <summary>
        /// 音源等リソースを解放する。
        /// </summary>
        private void ReleaseAudioSources()
        {
            if (this._outputSource == null)
            {
                return;
            }

            this._outputSource.Dispose();
            this._fadeInOut.FadeStrategy.FadingFinished -= this.OnFadeInOutFinished;
            this._fadeInOut.Dispose();
            this._equalizer.Dispose();
            this._inputSource.Dispose();
        }

        /// <summary>
        /// リソース解放を行う
        /// </summary>
        public void Dispose()
        {
            this.ReleaseAudioSources();
            this._observers.DescribeAll(this);
            this._inputSource?.Dispose();
        }
    }
}
