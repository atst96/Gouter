using System;
using Gouter.Extensions;
using NAudio.Dsp;
using NAudio.Wave;

namespace Gouter.SampleProviders
{
    /// <summary>
    /// イコライザ
    /// </summary>
    internal class EqualizerSampleProvider : ISampleProvider
    {
        private ISampleProvider _sourceProvider;
        private EqualizerBand[] _bands;
        private BiQuadFilter[,] _filters;

        /// <summary>
        /// イコライザを生成する
        /// </summary>
        /// <param name="sourceProvider"></param>
        /// <param name="bands"></param>
        public EqualizerSampleProvider(ISampleProvider sourceProvider, EqualizerBand[] bands)
        {
            this._sourceProvider = sourceProvider;
            this._bands = bands;
            this._filters = CreateFilters(sourceProvider.WaveFormat, bands);
        }

        /// <summary>
        /// イコライザ用のフィルタを生成する
        /// </summary>
        /// <param name="format">音声フォーマット</param>
        /// <param name="bands">バンド情報</param>
        /// <returns></returns>
        private static BiQuadFilter[,] CreateFilters(WaveFormat format, EqualizerBand[] bands)
        {
            int channels = format.Channels;
            int sampleRate = format.SampleRate;
            var filters = new BiQuadFilter[channels, bands.Length];

            for (int bandIdx = 0; bandIdx < bands.Length; ++bandIdx)
            {
                var band = bands[bandIdx];

                for (int channelIdx = 0; channelIdx < channels; ++channelIdx)
                {
                    filters[channelIdx, bandIdx] = BiQuadFilter.PeakingEQ(sampleRate, band.Frequency, band.BandWidth, band.Gain);
                }
            }

            return filters;
        }

        /// <summary>
        /// イコライザの設定値を変更する
        /// </summary>
        /// <param name="band"></param>
        public void Update(EqualizerBand band)
        {
            int bandIdx = this._bands.IndexOf(band);
            var format = this._sourceProvider.WaveFormat;

            this.Update(bandIdx, format.Channels, format.SampleRate, band);
        }

        /// <summary>
        /// イコライザの設定値を変更する
        /// </summary>
        /// <param name="bandIdx"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        /// <param name="band"></param>
        private void Update(int bandIdx, int channels, int sampleRate, EqualizerBand band)
        {
            for (int channelIdx = 0; channelIdx < channels; ++channels)
            {
                this._filters[channelIdx, bandIdx].SetPeakingEq(sampleRate, band.Frequency, band.BandWidth, band.Gain);
            }
        }

        /// <summary>
        /// 音声フォーマット
        /// </summary>
        public WaveFormat WaveFormat => this._sourceProvider.WaveFormat;

        /// <summary>
        /// 音声処理を行う
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int channels = this.WaveFormat.Channels;
            int samples = this._sourceProvider.Read(buffer, offset, count);

            for (int sampleIdx = 0; sampleIdx < samples; ++sampleIdx)
            {
                int ch = sampleIdx % channels;
                for (int band = 0; band < this._bands.Length; ++band)
                {
                    buffer[offset + sampleIdx] = this._filters[ch, band].Transform(buffer[offset + sampleIdx]);
                }
            }

            return samples;
        }

        /// <summary>
        /// 10バンドのイコライザを生成する
        /// </summary>
        /// <param name="sampleProvider">オーディオソース</param>
        /// <returns></returns>
        public static EqualizerSampleProvider Create10BandEqualizer(ISampleProvider sampleProvider)
        {
            return Create10BandEqualizer(sampleProvider, 18f, 0f);
        }

        /// <summary>
        /// 10バンドのイコライザを生成する
        /// </summary>
        /// <param name="sampleProvider">オーディオソース</param>
        /// <param name="bandWidth">バンド幅</param>
        /// <param name="gain">初期ゲイン</param>
        /// <returns></returns>
        public static EqualizerSampleProvider Create10BandEqualizer(ISampleProvider sampleProvider, float bandWidth, float gain)
        {
            EqualizerBand[] bands =
            {
                new (bandWidth, 32, gain),
                new (bandWidth, 64, gain),
                new (bandWidth, 125, gain),
                new (bandWidth, 250, gain),
                new (bandWidth, 500, gain),
                new (bandWidth, 1000, gain),
                new (bandWidth, 2000, gain),
                new (bandWidth, 4000, gain),
                new (bandWidth, 8000, gain),
                new (bandWidth, 16000, gain),
            };

            return new EqualizerSampleProvider(sampleProvider, bands);
        }
    }
}
