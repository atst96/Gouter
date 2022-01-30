using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Gouter.Managers;
using NAudio.Wave;

namespace Gouter.Devices;

/// <summary>
/// DirectSoundデバイス情報
/// </summary>
internal class DirectSoundDeviceInfo : NotificationObject, IAudioDeviceInfo
{
    /// <summary>
    /// デバイスのGuid
    /// </summary>
    public Guid Guid { get; private set; }

    /// <summary>
    /// デバイス名
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// システム規定のデバイスフラグ
    /// </summary>
    public bool IsSystemDefault { get; private set; }

    /// <summary>
    /// DirectSoundデバイス情報
    /// </summary>
    /// <param name="deviceInfo">デバイス情報</param>
    public DirectSoundDeviceInfo(NAudio.Wave.DirectSoundDeviceInfo deviceInfo)
    {
        this.Guid = deviceInfo.Guid;
        this.IsSystemDefault = (this.Guid == default);
        this.Name = this.IsSystemDefault ? "システム規定" : deviceInfo.Description;
    }

    /// <summary>
    /// デバイス情報を列挙する
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyList<DirectSoundDeviceInfo> EnumerateDevices()
        => ThreadManager.DeviceDispatcher.Invoke(() =>
        {
            return DirectSoundOut.Devices
                .Select(d => new DirectSoundDeviceInfo(d))
                .ToList();
        });
}
