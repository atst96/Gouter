using System;
using NAudio.Wave;

namespace Gouter.AudioFilters
{
    /// <summary>
    /// FadeInOutSampleProviderにフェード終了イベントを追加したもの
    /// https://github.com/naudio/NAudio/blob/fb35ce8367f30b8bc5ea84e7d2529e172cf4c381/NAudio.Core/Wave/SampleProviders/FadeInOutSampleProvider.cs
    /// 
    /// </summary>
    public class FadeInOutSampleProvider : ISampleProvider
    {
        public enum FadeState
        {
            Silence,
            FadingIn,
            FullVolume,
            FadingOut,
        }

        private readonly object lockObject = new object();
        private readonly ISampleProvider source;
        private int fadeSamplePosition;
        private int fadeSampleCount;
        private FadeState fadeState;

        /// <summary>
        /// Creates a new FadeInOutSampleProvider
        /// </summary>
        /// <param name="source">The source stream with the audio to be faded in or out</param>
        /// <param name="initiallySilent">If true, we start faded out</param>
        public FadeInOutSampleProvider(ISampleProvider source, bool initiallySilent = false)
        {
            this.source = source;
            this.fadeState = initiallySilent ? FadeState.Silence : FadeState.FullVolume;
        }

        /// <summary>
        /// Requests that a fade-in begins (will start on the next call to Read)
        /// </summary>
        /// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
        public void BeginFadeIn(double fadeDurationInMilliseconds)
        {
            lock (this.lockObject)
            {
                this.fadeSamplePosition = 0;
                this.fadeSampleCount = (int)(fadeDurationInMilliseconds * this.source.WaveFormat.SampleRate / 1000);
                this.fadeState = FadeState.FadingIn;
            }
        }

        /// <summary>
        /// Requests that a fade-out begins (will start on the next call to Read)
        /// </summary>
        /// <param name="fadeDurationInMilliseconds">Duration of fade in milliseconds</param>
        public void BeginFadeOut(double fadeDurationInMilliseconds)
        {
            lock (this.lockObject)
            {
                this.fadeSamplePosition = 0;
                this.fadeSampleCount = (int)(fadeDurationInMilliseconds * this.source.WaveFormat.SampleRate / 1000);
                this.fadeState = FadeState.FadingOut;
            }
        }

        /// <summary>
        /// Reads samples from this sample provider
        /// </summary>
        /// <param name="buffer">Buffer to read into</param>
        /// <param name="offset">Offset within buffer to write to</param>
        /// <param name="count">Number of samples desired</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int sourceSamplesRead = this.source.Read(buffer, offset, count);
            lock (this.lockObject)
            {
                if (this.fadeState == FadeState.FadingIn)
                {
                    this.FadeIn(buffer, offset, sourceSamplesRead);
                }
                else if (this.fadeState == FadeState.FadingOut)
                {
                    this.FadeOut(buffer, offset, sourceSamplesRead);
                }
                else if (this.fadeState == FadeState.Silence)
                {
                    ClearBuffer(buffer, offset, count);
                }
            }
            return sourceSamplesRead;
        }

        private static void ClearBuffer(float[] buffer, int offset, int count)
        {
            for (int n = 0; n < count; n++)
            {
                buffer[n + offset] = 0;
            }
        }

        private void FadeOut(float[] buffer, int offset, int sourceSamplesRead)
        {
            int sample = 0;
            while (sample < sourceSamplesRead)
            {
                float multiplier = 1.0f - (fadeSamplePosition / (float)fadeSampleCount);
                for (int ch = 0; ch < source.WaveFormat.Channels; ch++)
                {
                    buffer[offset + sample++] *= multiplier;
                }
                this.fadeSamplePosition++;
                if (this.fadeSamplePosition > fadeSampleCount)
                {
                    fadeState = FadeState.Silence;
                    // clear out the end
                    ClearBuffer(buffer, sample + offset, sourceSamplesRead - sample);
                    this.FadingFinished?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }

        private void FadeIn(float[] buffer, int offset, int sourceSamplesRead)
        {
            int sample = 0;
            while (sample < sourceSamplesRead)
            {
                float multiplier = this.fadeSamplePosition / (float)this.fadeSampleCount;
                for (int ch = 0; ch < this.source.WaveFormat.Channels; ch++)
                {
                    buffer[offset + sample++] *= multiplier;
                }
                this.fadeSamplePosition++;
                if (this.fadeSamplePosition > this.fadeSampleCount)
                {
                    this.fadeState = FadeState.FullVolume;
                    // no need to multiply any more
                    this.FadingFinished?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }

        /// <summary>
        /// WaveFormat of this SampleProvider
        /// </summary>
        public WaveFormat WaveFormat => this.source.WaveFormat;

        /// <summary>
        /// Event for fading finished
        /// </summary>
        public event EventHandler FadingFinished;
    }
}
