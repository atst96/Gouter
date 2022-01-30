using System.Collections.Generic;
using System.Linq;
using Gouter.Managers;
using NAudio.Wave.Asio;

namespace Gouter.Devices;

/// <summary>
/// ASIOデバイス情報
/// </summary>
internal class AsioDeviceInfo : NotificationObject, IAudioDeviceInfo
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
    /// ASIOデバイス情報
    /// </summary>
    /// <param name="name"></param>
    private AsioDeviceInfo(string name)
    {
        this.Id = name;
        this.Name = name;
    }

    /// <summary>
    /// デバイス情報を列挙する
    /// </summary>
    /// <returns></returns>
    public static List<AsioDeviceInfo> EnumerateDevices()
        => ThreadManager.DeviceDispatcher.Invoke(() =>
        {
            return AsioDriver.GetAsioDriverNames()
                .Select(d => new AsioDeviceInfo(d))
                .ToList();
        });
}
