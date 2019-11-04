using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    /// <summary>
    /// 再生状態
    /// </summary>
    internal enum PlayState
    {
        /// <summary>再生中</summary>
        Play,

        /// <summary>一時停止中</summary>
        Pause,

        /// <summary>停止中</summary>
        Stop,
    }
}
