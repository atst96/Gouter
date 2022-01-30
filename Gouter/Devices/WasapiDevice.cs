using System;
using Gouter.Managers;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Gouter.Devices;

/// <summary>
/// WASAPI出力デバイス
/// </summary>
internal class WasapiDevice : SoundDevice
{
    /// <summary>
    /// デバイス情報
    /// </summary>
    public WasapiDeviceInfo Info { get; }

    /// <summary>
    /// オーディオデバイス
    /// </summary>
    private MMDevice _device;
    private AudioClientShareMode _shareMode;
    private bool _useEventAsync;
    private int _latency;
    private WasapiOut _audioRender;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="device"></param>
    public WasapiDevice(WasapiDeviceInfo device, AudioClientShareMode shareMode, bool useEventAsync, int latency)
    {
        this.Info = device;
        this._device = GetDevice(device);
        this._shareMode = shareMode;
        this._useEventAsync = useEventAsync;
        this._latency = latency;
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
    private WasapiOut UpdateAudioRender()
    {
        this.ReleaseRender();

        var newRender = this._audioRender = new WasapiOut(
            this._device, this._shareMode, this._useEventAsync, this._latency);

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

        this._device = null;
    }

    /// <summary>
    /// デバイスを取得する
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static MMDevice GetDevice(WasapiDeviceInfo deviceInfo) => ThreadManager.DeviceDispatcher.Invoke(() =>
      {
          using var devices = new MMDeviceEnumerator();

          return deviceInfo.Id is null
              ? devices.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
              : devices.GetDevice(deviceInfo.Id);
      });
}
