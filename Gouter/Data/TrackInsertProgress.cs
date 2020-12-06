namespace Gouter.Data
{
    /// <summary>
    /// トラック追加の進捗状況
    /// </summary>
    internal record TrackInsertProgress
    {
        /// <summary>
        /// トラック情報
        /// </summary>
        public TrackInfo Track { get; init; }

        /// <summary>
        /// 現在の要素
        /// </summary>
        public int CurrentCount { get; init; }

        /// <summary>
        /// すべての要素数
        /// </summary>
        public int MaxCount { get; init; }
    }
}
