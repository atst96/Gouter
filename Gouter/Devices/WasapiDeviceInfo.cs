using System.Collections.Generic;
using System.Linq;
using Gouter.Managers;
using NAudio.CoreAudioApi;

namespace Gouter.Devices;

/// <summary>
/// WASAPIデバイス情報
/// </summary>
internal class WasapiDeviceInfo : NotificationObject, IAudioDeviceInfo
{
    /// <summary>
    /// デバイスID
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// デバイス名
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// デフォルトフラグ
    /// </summary>
    public bool IsSytemDefault { get; private set; }

    /// <summary>
    /// WASAPIデバイス情報(システムデフォルト)
    /// </summary>
    private WasapiDeviceInfo()
    {
        this.Id = null;
        this.Name = "システム既定";
        this.IsSytemDefault = true;
    }

    /// <summary>
    /// WASAPIデバイス情報
    /// </summary>
    /// <param name="device">デバイス情報</param>
    /// <param name="isSystemDefault">システムのデフォルトデバイスかどうかのフラグ</param>
    private WasapiDeviceInfo(MMDevice device)
    {
        this.Id = device.ID;
        this.Name = device.FriendlyName;
        this.IsSytemDefault = false;
    }

    /// <summary>
    /// デバイス情報を列挙する
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyList<WasapiDeviceInfo> EnumerateDevices() => ThreadManager.DeviceDispatcher.Invoke(() =>
    {
        using var enumerator = new MMDeviceEnumerator();
        var nativeDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        var devices = new List<WasapiDeviceInfo>(nativeDevices.Count + 1)
        {
            new WasapiDeviceInfo(),
        };

        devices.AddRange(nativeDevices.Select(d => new WasapiDeviceInfo(d)));

        return devices;
    });
}
