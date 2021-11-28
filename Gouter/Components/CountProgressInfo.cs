namespace Gouter
{
    internal class CountProgressInfo
    {
        public CountProgressInfo(int current, int maxCount)
        {
            this.Current = current;
            this.MaxCount = maxCount;
        }

        public int Current { get; }
        public int MaxCount { get; }
    }
}
