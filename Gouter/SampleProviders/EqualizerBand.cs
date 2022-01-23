namespace Gouter.SampleProviders
{
    /// <summary>
    /// イコライザーのバンド
    /// </summary>
    internal class EqualizerBand
    {
        /// <summary>
        /// バンド幅
        /// </summary>
        public float BandWidth { get; set; }

        /// <summary>
        /// 周波数
        /// </summary>
        public float Frequency { get; set; }

        /// <summary>
        /// ゲイン
        /// </summary>
        public float Gain { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="bandWidth">バンド幅</param>
        /// <param name="frequency">周波数</param>
        /// <param name="gain">ゲイン</param>
        public EqualizerBand(float bandWidth, float frequency, float gain)
        {
            this.BandWidth = bandWidth;
            this.Frequency = frequency;
            this.Gain = gain;
        }
    }
}
