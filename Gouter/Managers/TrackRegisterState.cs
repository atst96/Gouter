namespace Gouter.Managers
{
    /// <summary>
    /// トラック情報登録状況
    /// </summary>
    public enum TrackRegisterState
    {
        /// <summary>
        /// 検索中
        /// </summary>
        Collecting,

        /// <summary>
        /// 登録中
        /// </summary>
        InProgress,

        /// <summary>
        /// 完了
        /// </summary>
        Complete,

        /// <summary>
        /// 登録情報なし
        /// </summary>
        NotFound,
    }
}
