using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using Gouter.SampleProviders;
using NAudio.Wave;

namespace Gouter.Players;

/// <summary>
/// 音声ファイルの再生処理を行うクラス
/// </summary>
internal class SoundFilePlayer : IDisposable
{
    /// <summary>
    /// フェード後に遷移する状態
    /// </summary>
    private PlayState? _afterFadeState;

    /// <summary>
    /// 破棄済みかどうかのフラグ
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// ロック用のオブジェクト
    /// </summary>
    private readonly object @_lockObject = new();

    /// <summary>
    /// 音声ソースが初夏済みかどうかのフラグ
    /// </summary>
    private volatile bool _isSoundSourceInitialized;

    /// <summary>
    /// 音声入力ソース
    /// </summary>
    private AudioFileReader _inputSource;

    /// <summary>
    /// フェードイン／アウト制御
    /// </summary>
    private FadeInOutSampleProvider _fadeInOut;

    /// <summary>
    /// イコライザ
    /// </summary>
    private EqualizerSampleProvider _equalizer;

    /// <summary>
    /// 音声出力ソース
    /// </summary>
    private ISampleProvider _outputSource;

    /// <summary>
    /// ボリューム
    /// </summary>
    private float _volume = 0.5f;

    /// <summary>
    /// 音声出力デバイス
    /// </summary>
    private SoundDevice _soundDevice;

    /// <summary>
    /// フェード時の最大ボリューム
    /// </summary>
    private const float FadeMaxVolume = 1.0f;

    /// <summary>
    /// フェード時の最小ボリューム
    /// </summary>
    private const float FadeMinVolume = 0.0f;

    /// <summary>
    /// 再生処理停止要求フラグ
    /// </summary>
    private volatile bool _isStopRequested = false;

    /// <summary>
    /// 再生スレッドのディスパッチャ
    /// </summary>
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

    /// <summary>
    /// 再生状態を取得する。
    /// </summary>
    public PlayState State { get; private set; } = PlayState.Stop;

    /// <summary>
    /// プレーヤ設定(内部変数)
    /// </summary>
    private PlayerOptions _options = new();

    /// <summary>
    /// プレーヤ設定
    /// </summary>
    public PlayerOptions Options
    {
        get => this._options;
        set => this._options = value ?? throw new ArgumentNullException(nameof(this.Options));
    }

    /// <summary>
    /// ミュート状態
    /// </summary>
    private bool _isMuted;

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
    private bool _isStopToNextSource;

    /// <summary>
    /// 内部プレーヤが再生中かどうかのフラグ
    /// </summary>
    private bool _isPlaying;

    /// <summary>
    /// 次のオーディオソースが指定されているかどうかのフラグ
    /// </summary>
    private bool _hasNextAudioSource;

    /// <summary>
    /// ボリュームを取得または設定する。
    /// </summary>
    public float Volume
    {
        get => this._volume;
        set
        {
            this._volume = value;
            this.UpdateVolume();
        }
    }
    /// <summary>
    /// 再生処理に失敗した場合に呼び出されるイベントハンドラ
    /// </summary>
    public event EventHandler<Exception> PlayFailed;

    /// <summary>
    /// トラック変更時に呼び出されるイベントハンドラ
    /// </summary>
    public event EventHandler TrackFinished;

    /// <summary>
    /// 再生状態の変更時に呼び出されるイベントハンドラ
    /// </summary>
    public event EventHandler<PlayState> PlayStateChanged;

    /// <summary>
    /// ミュート状態を取得または設定する
    /// </summary>
    public bool IsMuted
    {
        get => this._isMuted;
        set
        {
            this._isMuted = value;
            this.UpdateVolume();
        }
    }

    /// <summary>
    /// ボリュームの更新
    /// </summary>
    private void UpdateVolume()
    {
        if (this._soundDevice is null)
        {
            return;
        }

        float volume = 0.0f;

        if (!this._isMuted)
        {
            volume = this._volume;
        }

        this._inputSource.Volume = volume;
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SoundFilePlayer(PlayerOptions options)
    {
        this.Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// サウンドデバイスの再生処理が停止した
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSoundDevicePlayStopped(object sender, StoppedEventArgs e)
    {
        bool isStopRequested = this._isStopRequested;
        this._isStopRequested = false;
        this._isPlaying = false;

        // 再生停止時
        this.OnStateChanged(PlayState.Stop);

        if (!isStopRequested)
        {
            // 現在トラックの再生終了
            this.TrackFinished?.Invoke(this, EventArgs.Empty);
        }

        if (this._hasNextAudioSource)
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
    public void SetSoundDevice([NotNull] SoundDevice soundDevice)
    {
        // 再生停止状態でなければ操作を受け付けない。
        if (this.State != PlayState.Stop && !this._isStopRequested)
        {
            throw new InvalidOperationException();
        }

        // デバイスに変更がなければ処理を行わない
        if (object.ReferenceEquals(this._soundDevice, soundDevice))
        {
            return;
        }

        // 旧デバイス
        var oldDevice = this._soundDevice;

        // 新規サウンドデバイスを設定する。
        soundDevice.PlaybackStopped += this.OnSoundDevicePlayStopped;
        this._soundDevice = soundDevice;

        // 旧デバイスのイベント購読を解除する。
        if (oldDevice != null)
        {
            oldDevice.PlaybackStopped -= this.OnSoundDevicePlayStopped;
        }
    }

    /// <summary>
    /// オーディオソースを変更する。
    /// </summary>
    /// <param name="path"></param>
    private void ChangeAudioSource(string path)
    {
        var device = this._soundDevice;
        if (device.PlaybackState != PlaybackState.Stopped)
        {
            // 再生中の場合

            if (!this._isStopRequested)
            {
                // 再生中の場合、再生を停止する
                this.StopInternal();
            }

            // 内部プレーヤの停止完了時にソースを切り替える
            this.SetNextAudioSource(path);
        }
        else
        {
            // 停止中の場合
            this.SetNextAudioSource(path);

            // オーディオソースを読み込む
            this.LoadSoundSource(path);
        }
    }

    /// <summary>
    /// 再生処理を停止する。
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
            this.StopInternalPlayer();
        }
    }

    /// <summary>
    /// フェードイン処理が可能かどうかを取得する。
    /// </summary>
    /// <returns></returns>
    private bool GetIsFadeOutEnable()
    {
        var options = this.Options;
        return options.IsEnableFadeInOut && options.FadeInOutDuration > TimeSpan.Zero;
    }

    /// <summary>
    /// フェードアウト処理が可能かどうかを取得する。
    /// </summary>
    /// <returns></returns> 
    private bool GetIsFadeInEnable()
    {
        var options = this.Options;
        return options.IsEnableFadeInOut && options.FadeInOutDuration > TimeSpan.Zero;
    }

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
                return this._inputSource.CurrentTime;
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
    public void Seek(TimeSpan position)
    {
        if (this._inputSource != null)
        {
            this._inputSource.CurrentTime = position;
        }
    }

    /// <summary>
    /// 音源等リソースを解放する。
    /// </summary>
    private void ReleaseAudioSources()
    {
        this._outputSource = null;

        if (this._fadeInOut != null)
        {
            this._fadeInOut.FadingFinished -= this.OnFadeInOutFinished;
            this._fadeInOut = null;
        }

        // this._euqalizer = null;
        this.ReleaseInstance(ref this._inputSource);
    }

    /// <summary>
    /// 楽曲の長さ(尺)を取得する。
    /// </summary>
    /// <returns></returns>
    public TimeSpan? GetDuration()
    {
        return this._isSoundSourceInitialized ? this._inputSource.TotalTime : null;
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
            this.PlayStateChanged?.Invoke(this, state);
        }
    });

    /// <summary>
    /// 切り替え先の音源を設定する。
    /// </summary>
    /// <param name="path">音声ファイルのフルパス</param>
    private void SetNextAudioSource(string path)
    {
        this._hasNextAudioSource = true;
        this._nextAudioSource = path;
    }

    /// <summary>
    /// 再生トラックを変更する。
    /// </summary>
    /// <param name="track"></param>
    public void ChangeSource(TrackInfo track)
    {
        this.AssertDisposed();

        bool isPlaying = this._isPlaying && !this._isStopRequested;
        this._isStopToNextSource = isPlaying || this._isStopToNextSource;

        if (isPlaying)
        {
            this.ChangeAudioSource(track.Path);
            return;
        }

        this.ChangeAudioSource(track.Path);

        if (this.State == PlayState.Stop)
        {
            // this.PlayInternal();
        }
        else
        {
            this.StopInternal();
        }
    }

    /// <summary>
    /// 再生を開始する
    /// </summary>
    private void PlayInternal()
    {
        var state = this.State;

        if (this._isStopRequested && this._hasNextAudioSource)
        {
            // トラック変更中(再生停止要求中&トラック変更済み)の場合
            // 次トラックの再生を有効にする
            this._isStopToNextSource = true;
            return;
        }

        if (this._afterFadeState != PlayState.Play || state == PlayState.Pause)
        {
            // 一時停止中の場合
            // フェードインで再生を開始する
            if (this.GetIsFadeInEnable())
            {
                this.FadeIn();
            }

            this.PlayInternalPlayer();
            return;
        }

        // デバイスと再生音源が設定されていない場合は操作を受け付けない
        if (this._soundDevice == null || this._inputSource == null)
        {
            throw new InvalidOperationException();
        }

        // 再生処理を開始する
        this._isPlaying = true;
        this.PlayInternalPlayer();
    }

    /// <summary>
    /// 再生処理を開始する。
    /// </summary>
    public void Play()
    {
        this.AssertDisposed();
        this.PlayInternal();
    }

    /// <summary>
    /// 再生トラックを指定してトラックを変更する。
    /// </summary>
    /// <param name="track"></param>
    public void Play(TrackInfo track)
    {
        this.AssertDisposed();

        if (this._isPlaying && !this._isStopRequested)
        {
            // 再生中の場合
            this._isStopToNextSource = true;
            this.ChangeAudioSource(track.Path);
            return;
        }

        this.ChangeAudioSource(track.Path);
        this.PlayInternal();
    }

    /// <summary>
    /// 再生を停止する。
    /// </summary>
    public void Pause() => this.Pause(true);

    /// <summary>
    /// 一時停止する。
    /// </summary>
    public void Pause(bool isFadeOutEnable)
    {
        this.AssertDisposed();

        this._isStopToNextSource = false;
        var state = this.State;

        if (state != PlayState.Play)
        {
            return;
        }

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
            this.PauseInternalPlayer();
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
    {
        this.AssertDisposed();
        this.StopInternal(isFadeOutEnable, true);
    }

    /// <summary>
    /// フェードインする。
    /// </summary>
    private void FadeIn(PlayState? afterState = null)
    {
        if (this._isDisposed)
        {
            return;
        }

        this._afterFadeState = afterState;
        this._fadeInOut.BeginFadeIn(this.Options.FadeInOutDuration.TotalMilliseconds);
    }

    /// <summary>
    /// フェードアウトする。
    /// </summary>
    private void FadeOut(PlayState? afterState = null)
    {
        if (this._isDisposed)
        {
            return;
        }

        this._afterFadeState = afterState;
        this._fadeInOut.BeginFadeOut(this.Options.FadeInOutDuration.TotalMilliseconds);
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
                this.PauseInternalPlayer();
                break;

            case PlayState.Stop:
                this.StopInternalPlayer();
                break;
        }

        this._afterFadeState = null;
    });

    /// <summary>
    /// ファイルパスからオーディオソースを読み込む。
    /// </summary>
    /// <param name="path"></param>
    private void LoadSoundSource(string path)
    {
        this._isSoundSourceInitialized = false;
        this._currentAudioSource = null;
        this._nextAudioSource = null;
        this._hasNextAudioSource = false;

        this.ReleaseAudioSources();

        try
        {
            ISampleProvider outputSource;

            outputSource = this._inputSource = new AudioFileReader(path);
            outputSource = this._equalizer = EqualizerSampleProvider.Create10BandEqualizer(outputSource);
            outputSource = this._fadeInOut = new FadeInOutSampleProvider(outputSource, initiallySilent: false);
            this._fadeInOut.FadingFinished += this.OnFadeInOutFinished;

            this._outputSource = outputSource;
        }
        catch (Exception ex)
        {
            this.ReleaseAudioSources();
            this.PlayFailed?.Invoke(this, ex);
            throw;
        }

        var device = this._soundDevice;
        device.SetAudioSource(this._outputSource);
        this.UpdateVolume();

        this._currentAudioSource = this._nextAudioSource;
        this._nextAudioSource = null;

        this._isSoundSourceInitialized = true;
    }

    /// <summary>
    /// 内部プレーヤの再生を開始する。
    /// </summary>
    private void PlayInternalPlayer()
    {
        this._soundDevice?.Play();

        // 再生状態を更新
        this._afterFadeState = null;
        this.OnStateChanged(PlayState.Play);
    }

    /// <summary>
    /// 内部プレーヤの再生を一時停止する。
    /// </summary>
    private void PauseInternalPlayer()
    {
        this._soundDevice?.Pause();

        // 再生状態を更新
        this.OnStateChanged(PlayState.Pause);
    }

    /// <summary>
    /// 内部プレーヤの再生を停止する。
    /// </summary>
    private void StopInternalPlayer()
    {
        this._soundDevice?.Stop();

        // MEMO: 停止時の状態通知はイベント内で行う
        // @see: 
    }

    /// <summary>
    /// リソースを解放する。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    private void ReleaseInstance<T>(ref T instance)
        where T : IDisposable
    {
        if (instance == null)
        {
            return;
        }

        instance.Dispose();
        instance = default;
    }

    /// <summary>
    /// リソース解放を行う。
    /// </summary>
    public void Dispose()
    {
        if (this._isDisposed)
        {
            return;
        }

        this._isDisposed = true;

        var device = this._soundDevice;
        if (device != null && device.PlaybackState != PlaybackState.Stopped)
        {
            device.PlaybackStopped -= this.OnSoundDevicePlayStopped;
            device.Stop();
        }

        this.ReleaseAudioSources();
    }

    /// <summary>
    /// インスタンスが破棄済みでないか検査する。
    /// </summary>
    /// <exception cref="InvalidOperationException" />
    private void AssertDisposed()
    {
        if (this._isDisposed)
        {
            throw new InvalidOperationException();
        }
    }
}
