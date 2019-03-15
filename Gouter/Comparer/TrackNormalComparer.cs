using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal class TrackNormalComparer : IComparer<TrackInfo>
    {
        public static readonly TrackNormalComparer Instance = new TrackNormalComparer();

        public int Compare(TrackInfo x, TrackInfo y)
        {
            if (x.DiskNumber != y.DiskNumber)
            {
                return x.DiskNumber.CompareTo(y.DiskNumber);
            }

            if (x.TrackNumber != y.TrackNumber)
            {
                return x.TrackNumber.CompareTo(y.TrackNumber);
            }

            return StringComparer.CurrentCultureIgnoreCase.Compare(x.Title, y.Title);
        }
    }
}
