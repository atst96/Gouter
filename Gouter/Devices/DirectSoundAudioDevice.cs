using NAudio.Wave;

namespace Gouter.Devices;

/// <summary>
/// DirectSound音声出力デバイス
/// </summary>
internal class DirectSoundAudioDevice : AudioDevice
{
    /// <summary>
    /// デバイス情報
    /// </summary>
    public DirectSoundDeviceInfo Info { get; }

    /// <summary>
    /// オーディオレンダラ
    /// </summary>
    private DirectSoundOut _audioRender;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="deviceGuid"></param>
    public DirectSoundAudioDevice(DirectSoundDeviceInfo deviceInfo)
    {
        this.Info = deviceInfo;
    }

    /// <summary>
    /// 再生状態
    /// </summary>
    public override PlaybackState PlaybackState => this._audioRender?.PlaybackState ?? PlaybackState.Stopped;

    /// <summary>
    /// オーディオソースを設定する
    /// </summary>
    /// <param name="waveProvider"></param>
    public override void SetAudioSource(ISampleProvider waveProvider) => Invoke(() =>
    {
        var render = this.UpdateAudioRender();

        render.Init(waveProvider);
    });

    /// <summary>
    /// オーディオレンダラを更新する
    /// </summary>
    /// <returns>新しいオーディオレンダラ</returns>
    private DirectSoundOut UpdateAudioRender()
    {
        this.ReleaseRender();

        var newRender = this._audioRender = new DirectSoundOut(this.Info.Guid);
        newRender.PlaybackStopped += this.RaisePlaybackStopped;

        return newRender;
    }

    /// <summary>
    /// 再生
    /// </summary>
    public override void Play()
    {
        Invoke(() => this._audioRender?.Play());
    }

    /// <summary>
    /// 一時停止
    /// </summary>
    public override void Pause()
    {
        Invoke(() => this._audioRender?.Pause());
    }

    /// <summary>
    /// 停止
    /// </summary>
    public override void Stop()
    {
        Invoke(() => this._audioRender?.Stop());
    }

    /// <summary>
    /// 使用中のレンダラを解放する
    /// </summary>
    private void ReleaseRender()
    {
        var render = this._audioRender;
        if (render is not null)
        {
            render.PlaybackStopped -= this.RaisePlaybackStopped;
            render.Dispose();
        }
    }

    /// <summary>
    /// インスタンスを破棄する
    /// </summary>
    protected override void Dispose()
    {
        if (this._audioRender is not null)
        {
            this._audioRender?.Dispose();
            this._audioRender = null;
        }
    }
}
