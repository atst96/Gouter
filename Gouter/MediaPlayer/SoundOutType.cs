using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Gouter
{
    /// <summary>
    /// 音声の出力方法
    /// </summary>
    internal enum SoundOutType : ushort
    {
        [EnumMember(Value = "directsound")]
        /// <summary>DirectSound</summary>
        DirectSound = 0,

        [EnumMember(Value = "wasapi_shared")]
        /// <summary>WASAPI (共有)</summary>
        Wasapi_Shraed = 1,

        [EnumMember(Value = "wasapi_exlusive")]
        /// <summary>WASAPI (専有)</summary>
        Wasapi_Exclusive = 2,
    }
}
