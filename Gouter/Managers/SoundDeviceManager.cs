using System;
using System.Collections.Generic;
using System.Linq;
using Gouter.Devices;

namespace Gouter.Managers;

/// <summary>
/// サウンドデバイス情報
/// </summary>
internal class SoundDeviceManager : NotificationObject
{
    private IReadOnlyList<DirectSoundDeviceInfo> _directSoundDevices;
    private IReadOnlyList<WasapiDeviceInfo> _wasapiDevices;
    private List<AsioDeviceInfo> _asioDevices;

    /// <summary>
    /// サウンドデバイス情報
    /// </summary>
    public SoundDeviceManager()
    {
    }

    /// <summary>
    /// DirectSoundのデバイス情報を取得する
    /// </summary>
    public IReadOnlyList<DirectSoundDeviceInfo> DirectSoundDevices
        => this._directSoundDevices ??= DirectSoundDeviceInfo.EnumerateDevices();

    /// <summary>
    /// DirectSoundデバイス情報を取得する
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public DirectSoundDeviceInfo GetDirectSoundDevice(Guid guid)
        => this.DirectSoundDevices.Single(i => i.Guid == guid);

    /// <summary>
    /// WASAPIデバイス情報を取得する
    /// </summary>
    public IReadOnlyList<WasapiDeviceInfo> WasapiDevices
        => this._wasapiDevices ??= WasapiDeviceInfo.EnumerateDevices();

    /// <summary>
    /// WASAPIデバイス情報を取得する
    /// </summary>
    /// <param name="deviceId">デバイスID</param>
    public WasapiDeviceInfo GetWasapiDevice(string deviceId)
        => this.WasapiDevices.Single(i => i.Id == deviceId);

    /// <summary>
    /// ASIOデバイス情報を取得する
    /// </summary>
    public IReadOnlyList<AsioDeviceInfo> AsioDevices
        => this._asioDevices ??= AsioDeviceInfo.EnumerateDevices();


    /// <summary>
    /// ASIOデバイス情報を取得する
    /// </summary>
    /// <param name="driverId">デバイスID</param>
    public AsioDeviceInfo GetAsioDevice(string driverId)
        => this.AsioDevices.Single(i => i.Id == driverId);

}
