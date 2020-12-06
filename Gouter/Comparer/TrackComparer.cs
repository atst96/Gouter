using System;
using System.Collections.Generic;
using ATL;

namespace Gouter
{
    /// <summary>
    /// トラック情報の比較クラス
    /// </summary>
    internal class TrackComparer : IComparer<Track>
    {
        /// <summary>インスタンス</summary>
        public static IComparer<Track> Instance { get; } = new TrackComparer();

        /// <summary>トラック情報の比較を行う</summary>
        /// <param name="x">左辺</param>
        /// <param name="y">右辺</param>
        /// <returns>比較結果</returns>
        public int Compare(Track x, Track y)
        {
            // ■ 比較優先度
            // 1. ディスク番号
            // 2. トラック番号
            // 3. トラック名

            if (x.DiscNumber != y.DiscNumber)
            {
                // ディスク番号が異なる場合、ディスク番号で比較する
                return x.DiscNumber.CompareTo(y.DiscNumber);
            }

            if (x.TrackNumber != y.TrackNumber)
            {
                // トラック番号が異なる場合、トラック番号で比較する
                return x.TrackNumber.CompareTo(y.TrackNumber);
            }

            // トラック名で比較する
            return StringComparer.CurrentCultureIgnoreCase.Compare(x.Title, y.Title);
        }
    }
}
