using System.Runtime.Serialization;

namespace Gouter;

/// <summary>
/// 音声の出力方法
/// </summary>
public enum BackendType : ushort
{
    /// <summary>
    /// DirectSound
    /// </summary>
    [EnumMember(Value = "directsound")]
    DirectSound = 0,

    /// <summary>
    /// WASAPI
    /// </summary>
    [EnumMember(Value = "wasapi")]
    Wasapi = 1,

    /// <summary>
    /// ASIO
    /// </summary>
    [EnumMember(Value = "asio")]
    ASIO,
}
