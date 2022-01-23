using System;
using NAudio.CoreAudioApi;

namespace Gouter;

/// <summary>
/// サウンドデバイス情報
/// </summary>
internal class SoundDeviceInfo : NotificationObject
{
    private MMDevice _device;

    /// <summary>
    /// デバイスID
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// システムのデフォルトデバイスか否か
    /// </summary>
    public bool IsDefaultDevice { get; } = false;

    /// <summary>
    /// 表示名
    /// </summary>
    public string Name => this.IsDefaultDevice
        ? $"システム既定 ({this._device.FriendlyName})"
        : this._device.FriendlyName;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="device">デバイス情報</param>
    public SoundDeviceInfo(MMDevice device)
        : this(device, false)
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="device">デバイス情報</param>
    /// <param name="isDefaultDevice">システムのデフォルトデバイスか否か</param>
    public SoundDeviceInfo(MMDevice device, bool isDefaultDevice = false)
    {
        this.IsDefaultDevice = isDefaultDevice;
        this.Id = isDefaultDevice ? null : device.ID;
        this.Update(device);
    }

    /// <summary>
    /// デバイス情報を更新する。
    /// </summary>
    /// <param name="device">デバイス情報</param>
    internal void Update(MMDevice device)
    {
        if (!this.IsDefaultDevice && device.ID != this.Id)
        {
            // 異なるデバイスIDの場合
            throw new InvalidOperationException();
        }

        this._device = device;
        this.RaisePropertyChanged(nameof(this.Name));
    }

    /// <summary>
    /// デバイス情報を取得する。
    /// </summary>
    /// <returns>デバイス情報</returns>
    public MMDevice GetDevice() => this._device;
}
