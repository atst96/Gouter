using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal class StandardProgressReceiver : NotificationObject, IProgress<int>
    {
        private int _currentCount;
        public int CurrentCount
        {
            get => this._currentCount;
            private set => this.SetProperty(ref this._currentCount, value);
        }

        public int _maxCount;
        public int MaxCount
        {
            get => this._maxCount;
            private set => this.SetProperty(ref this._maxCount, value);
        }

        public StandardProgressReceiver()
            : this(0, 1)
        {
        }

        public StandardProgressReceiver(int maxCount) : this(0, maxCount)
        {
        }

        public StandardProgressReceiver(int currentCount, int maxCount)
        {
            this._currentCount = currentCount;
            this._maxCount = maxCount;
        }

        public void Reset()
        {
            this.CurrentCount = 0;
        }

        public void Reset(int maxCount)
        {
            this.CurrentCount = 0;
            this.MaxCount = maxCount;
        }

        public void Report(int value)
        {
            this.CurrentCount = value;
        }
    }
}
