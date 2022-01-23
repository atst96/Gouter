using System.Runtime.Serialization;

namespace Gouter;

/// <summary>
/// 音声の出力方法
/// </summary>
internal enum SoundOutType : ushort
{
    /// <summary>
    /// DirectSound
    /// </summary>
    [EnumMember(Value = "directsound")]
    DirectSound = 0,

    /// <summary>
    /// WASAPI (共有)
    /// </summary>
    [EnumMember(Value = "wasapi_shared")]
    Wasapi_Shraed = 1,

    /// <summary>
    /// WASAPI (専有)
    /// </summary>
    [EnumMember(Value = "wasapi_exlusive")]
    Wasapi_Exclusive = 2,

    // TODO: 非対応
    ///// <summary>
    ///// ASIO
    ///// </summary>
    //[EnumMember(Value = "asio")]
    //ASIO,
}
