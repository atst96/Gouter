namespace Gouter.Managers
{
    /// <summary>
    /// トラックの登録状況
    /// </summary>
    internal class TrackRegisterProgress
    {
        /// <summary>
        /// 登録状況
        /// </summary>
        public TrackRegisterState State { get; set; }

        /// <summary>
        /// 現在値
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 全数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// <see cref="TrackRegisterProgress"/>を生成する
        /// </summary>
        /// <param name="state">状況</param>
        public TrackRegisterProgress(TrackRegisterState state)
        {
            this.State = state;
        }

        /// <summary>
        /// <see cref="TrackRegisterProgress"/>を生成する
        /// </summary>
        /// <param name="state">状況</param>
        /// <param name="current">現在値</param>
        /// <param name="total">合計数</param>
        public TrackRegisterProgress(TrackRegisterState state, int current, int total)
        {
            this.State = state;
            this.Current = current;
            this.Total = total;
        }
    }
}
