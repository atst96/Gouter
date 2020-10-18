using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

        private readonly object @_lockObject = new object();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        public readonly IList<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();

        // 音源
        private volatile bool _isSoundSourceInitialized;
        private ISampleSource _inputSource;
        private FadeInOut _fadeInOut;
        private Equalizer _equalizer;
        private IWaveSource _outputSource;
        private float _volume = 0.5f;

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
        public TimeSpan? FadeInOutDuration { get; set; } = TimeSpan.FromMilliseconds(200);

        internal PlaybackState? _playerState => this._soundDevice?.PlaybackState;

        /// <summary>
        /// 再生状態を取得する。
        /// </summary>
        public PlayState State { get; private set; } = PlayState.Stop;

        /// <summary>
        /// 現在再生中のファイルパス
        /// </summary>
        private string _currentAudioSource;

        /// <summary>
        /// 次に再生するファイルパス
        /// </summary>
        private string _nextAudioSource;

        /// <summary>
        /// 停止後に次のファイルを再生するかどうかのフラグ
        /// </summary>
        private bool _isStopToNextSource = false;

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
        /// サウンドデバイスの再生処理が停止した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSoundDevicePlayStopped(object sender, PlaybackStoppedEventArgs e)
        {
            this._isStopRequested = false;
            this._isPlaying = false;
            this.OnStateChanged(PlayState.Stop);

            if (this._isSourceChanging)
            {

                this.LoadSoundSource(this._nextAudioSource);
            }

            if (this._isStopToNextSource)
            {
                this.PlayInternal();
            }
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

        private void ChangeSourceImpl(string path)
        {
            var device = this._soundDevice;
            var state = this.State;

            if (device.PlaybackState != PlaybackState.Stopped)
            {
                // 再生中の場合
                if (!this._isStopRequested)
                {
                    this.StopInternal();
                }

                this.ChangeSoundSource(path);
            }
            else
            {
                // 停止中の場合
                this.ChangeSoundSource(path);
                this.LoadSoundSource(path);
            }
        }

        private bool _isPlaying = false;
        private bool _isSourceChanging = false;

        /// <summary>
        /// 再生処理を停止ルル。
        /// </summary>
        private void StopInternal() => this.StopInternal(true);

        /// <summary>
        /// 再生処理を停止する。
        /// </summary>
        /// <param name="isFadeOutEnable">true指定時は停止時、<see cref="FadeInOutDuration"/>の長さだけフェードアウトする</param>
        /// <param name="cancelNext">次ファイルの再生をキャンセルする</param>
        private void StopInternal(bool isFadeOutEnable, bool cancelNext = false)
        {
            if (this._isStopRequested)
            {
                return;
            }

            if (cancelNext)
            {
                this._isStopToNextSource = false;
            }

            var device = this._soundDevice;
            if (device == null || (device?.PlaybackState == PlaybackState.Stopped && this.State == PlayState.Stop))
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
                this._soundDevice?.Stop();
            }
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
        /// 再生位置を取得する
        /// </summary>
        /// <returns>再生位置</returns>
        public TimeSpan GetPosition()
        {
            try
            {
                if (this._isSoundSourceInitialized)
                {
                    return this._inputSource.GetPosition();
                }
            }
            catch
            {
                // pass
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// 再生位置を設定する
        /// </summary>
        /// <param name="position">再生位置</param>
        public void SetPosition(TimeSpan position)
            => this._inputSource?.SetPosition(position);

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
        /// 楽曲の長さ(尺)を取得する。
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetDuration()
            => this._isSoundSourceInitialized ? this._inputSource.GetLength() : TimeSpan.Zero;

        /// <summary>
        /// リソース解放を行う
        /// </summary>
        public void Dispose()
        {
            this.ReleaseAudioSources();
            this._observers.DescribeAll(this);
            this._inputSource?.Dispose();
        }

        /// <summary>
        /// 再生状態の設定と通知を行う
        /// </summary>
        /// <param name="state">再生状態</param>
        private void OnStateChanged(PlayState state) => this._dispatcher.Invoke(() =>
        {
            lock (this.@_lockObject)
            {
                this.State = state;
                this._observers.NotifyAll(observer => observer.OnPlayStateChanged(state));
            }
        });

        /// <summary>
        /// 音源の初期化処理を行う
        /// </summary>
        /// <param name="path">音声ファイルのフルパス</param>
        private void ChangeSoundSource(string path)
        {
            lock (this._lockObject)
            {
                this._isSourceChanging = true;
                this._nextAudioSource = path;
            }
        }

        /// <summary>
        /// 再生処理を開始する。
        /// </summary>
        public void Play() => this.PlayInternal();

        /// <summary>
        /// 再生を開始する
        /// </summary>
        private void PlayInternal()
        {
            var state = this.State;

            if (state == PlayState.Play)
            {
                return;
            }

            if (this._isStopRequested && this._isSourceChanging)
            {
                // トラック変更中(再生停止要求中&トラック変更済み)の場合
                // 次トラックの再生を有効にする
                this._isStopToNextSource = true;
                return;
            }

            if (state == PlayState.Pause)
            {
                // 一時停止中の場合
                // フェードインで再生を開始する
                if (this.GetIsFadeInEnable())
                {
                    this.FadeIn();
                }

                this._soundDevice.Play();
                this.OnStateChanged(PlayState.Play);
                return;
            }

            // デバイスと再生音源が設定されていない場合は操作を受け付けない
            if (this._soundDevice == null || this._inputSource == null)
            {
                throw new InvalidOperationException();
            }

            // 再生処理を開始する
            this._isPlaying = true;
            this._soundDevice.Play();
            this.OnStateChanged(PlayState.Play);
        }

        /// <summary>
        /// 再生トラックを変更する。
        /// </summary>
        /// <param name="track"></param>
        public void ChangeSource(TrackInfo track)
        {
            bool isPlaying = this._isPlaying && !this._isStopRequested;
            this._isStopToNextSource = isPlaying || this._isStopToNextSource;

            if (isPlaying)
            {
                this.ChangeSourceImpl(track.Path);
                return;
            }

            this.ChangeSourceImpl(track.Path);

            if (this.State == PlayState.Stop)
            {
                this.PlayInternal();
            }
            else
            {
                this.StopInternal();
            }
        }

        /// <summary>
        /// 再生トラックを指定してトラックを変更する。
        /// </summary>
        /// <param name="track"></param>
        public void Play(TrackInfo track)
        {
            if (this._isPlaying && !this._isStopRequested)
            {
                // 再生中の場合
                this._isStopToNextSource = true;
                this.ChangeSourceImpl(track.Path);
                return;
            }

            this.ChangeSourceImpl(track.Path);
            this.PlayInternal();
        }

        public void Pause() => this.Pause(true);

        /// <summary>
        /// 一時停止する。
        /// </summary>
        public void Pause(bool isFadeOutEnable)
        {
            this._isStopToNextSource = false;
            var state = this.State;

            if (state != PlayState.Play)
            {
                return;
            }

            // 再生状態を更新
            this.OnStateChanged(PlayState.Pause);

            if (this._inputSource == null)
            {
                return;
            }

            if (isFadeOutEnable && this.GetIsFadeOutEnable())
            {
                this.FadeOut(PlayState.Pause);
            }
            else
            {
                this._soundDevice.Pause();
            }
        }

        /// <summary>
        /// 停止する。
        /// </summary>
        public void Stop() => this.Stop(true);

        /// <summary>
        /// 停止する。
        /// </summary>
        /// <param name="isFadeOutEnable"></param>
        public void Stop(bool isFadeOutEnable)
            => this.StopInternal(isFadeOutEnable, true);

        /// <summary>
        /// フェードインする。
        /// </summary>
        private void FadeIn(PlayState? afterState = null)
        {
            this._afterFadeState = afterState;
            var fadeInDuration = this.FadeInOutDuration ?? TimeSpan.Zero;
            var fadeStrategy = this._fadeInOut.FadeStrategy;

            fadeStrategy.StartFading(null, FadeMaxVolume, fadeInDuration);
        }

        /// <summary>
        /// フェードアウトする。
        /// </summary>
        private void FadeOut(PlayState? afterState = null)
        {
            this._afterFadeState = afterState;
            var fadeOutDuration = this.FadeInOutDuration ?? TimeSpan.Zero;
            var fadeStrategy = this._fadeInOut.FadeStrategy;

            fadeStrategy.StartFading(null, FadeMinVolume, fadeOutDuration);
        }

        /// <summary>
        /// フェードイン／アウト終了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFadeInOutFinished(object sender, EventArgs e) => this._dispatcher.InvokeAsync(() =>
        {
            switch (this._afterFadeState)
            {
                case PlayState.Pause:
                    this._soundDevice?.Pause();
                    break;

                case PlayState.Stop:
                    this._soundDevice?.Stop();
                    break;
            }
        });

        /// <summary>
        /// ファイルパスからオーディオソースを読み込む。
        /// </summary>
        /// <param name="path"></param>
        private void LoadSoundSource(string path)
        {
            this._isSoundSourceInitialized = false;
            this._isSourceChanging = false;

            this.ReleaseAudioSources();

            var fadeStrategy = new LinearFadeStrategy();
            fadeStrategy.FadingFinished += this.OnFadeInOutFinished;

            this._inputSource = CodecFactory.Instance.GetCodec(path).ToSampleSource();
            this._equalizer = Equalizer.Create10BandEqualizer(this._inputSource);
            this._fadeInOut = new FadeInOut(this._equalizer)
            {
                FadeStrategy = fadeStrategy,
            };
            this._outputSource = this._fadeInOut.ToWaveSource();

            var device = this._soundDevice;
            device.Initialize(this._outputSource);
            device.Volume = this._volume;

            this._currentAudioSource = this._nextAudioSource;
            this._nextAudioSource = null;

            this._isSoundSourceInitialized = true;
        }
    }
}
