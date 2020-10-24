/*
 * フェードアウト終了後に音声ノイズが発生するため、下記ソースコードを修正
 * https://github.com/filoe/cscore/blob/af1792ea680743c5172ece4727e2b331012e99de/CSCore/Streams/LinearFadeStrategy.cs
 */
using System;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using CSCore.Streams;

namespace Gouter.AudioFilters
{
    /// <summary>
    ///     Provides a linear fading algorithm.
    /// </summary>
    public class FadeStrategy : IFadeStrategy
    {
        private volatile float _currentVolume = 1;
        private float _startVolume;
        private volatile float _step;
        private float _targetVolume = 1;

        /// <summary>
        ///     Gets the current volume.
        /// </summary>
        public float CurrentVolume => this._currentVolume;

        /// <summary>
        ///     Gets the target volume.
        /// </summary>
        public float TargetVolume => this._targetVolume;

        /// <summary>
        ///     Occurs when the fading process has reached its target volume.
        /// </summary>
        public event EventHandler FadingFinished;

        /// <summary>
        ///     Gets a value which indicates whether the <see cref="FadeStrategy" /> class is fading.
        ///     True means that the <see cref="FadeStrategy" /> class is fading audio data.
        ///     False means that the <see cref="CurrentVolume" /> equals the <see cref="TargetVolume" />.
        /// </summary>
        public bool IsFading { get; private set; }

        /// <summary>
        ///     Gets or sets the sample rate to use.
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        ///     Gets or sets the number of channels.
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        ///     Starts fading <paramref name="from" /> a specified volume <paramref name="to" /> another volume.
        /// </summary>
        /// <param name="from">
        ///     The start volume in the range from 0.0 to 1.0. If no value gets specified, the default volume will be used.
        ///     The default volume is typically 100% or the current volume.
        /// </param>
        /// <param name="to">The target volume in the range from 0.0 to 1.0.</param>
        /// <param name="duration">The duration.</param>
        public void StartFading(float? from, float to, TimeSpan duration)
        {
            this.StartFading(@from, to, Math.Floor(duration.TotalMilliseconds));
        }

        /// <summary>
        ///     Starts fading <paramref name="from" /> a specified volume <paramref name="to" /> another volume.
        /// </summary>
        /// <param name="from">
        ///     The start volume in the range from 0.0 to 1.0. If no value gets specified, the default volume will be used.
        ///     The default volume is typically 100% or the current volume.
        /// </param>
        /// <param name="to">The target volume in the range from 0.0 to 1.0.</param>
        /// <param name="duration">The duration in milliseconds.</param>
        public void StartFading(float? from, float to, double duration)
        {
            if (this.SampleRate <= 0)
            {
                throw new InvalidOperationException("SampleRate property is not set to a valid value.");
            }

            if (this.Channels <= 0)
            {
                throw new InvalidOperationException("Channels property it not set to a valid value.");
            }

            if (to < 0 || to > 1)
            {
                throw new ArgumentOutOfRangeException("to");
            }

            if (this.IsFading)
            {
                this.StopFadingInternal();
            }

            if (!from.HasValue)
            {
                this._startVolume = this.CurrentVolume;
            }
            else
            {
                if (from.Value < 0 || from.Value > 1)
                {
                    throw new ArgumentOutOfRangeException("from");
                }

                this._startVolume = from.Value;
            }

            this._targetVolume = to;
            this._currentVolume = this._startVolume;

            // calculate the step
            var durationInBlocks = (int)(duration / 1000 * this.SampleRate);
            float delta = this._targetVolume - this._startVolume;
            this._step = delta / durationInBlocks;

            this.IsFading = true;
        }

        /// <summary>
        ///     Stops the fading.
        /// </summary>
        public void StopFading()
        {
            this.StopFadingInternal();
            this.OnFadingFinished();
        }

        /// <summary>
        ///     Applies the fading algorithm to the <paramref name="buffer" />.
        /// </summary>
        /// <param name="buffer">Float-array which contains IEEE-Float samples.</param>
        /// <param name="offset">Zero-based offset of the <paramref name="buffer"/>.</param>
        /// <param name="count">The number of samples, the fading algorithm has to be applied on.</param>
        public void ApplyFading(float[] buffer, int offset, int count)
        {
            if (!this.IsFading)
            {
                float currentValue = this._currentVolume;

                if (currentValue == 1.0f)
                {
                    return;
                }

                for (int i = offset; i < count; i++)
                {
                    buffer[i] *= currentValue;
                }

                return;
            }

            if (this.IsFading && this.IsFadingFinished())
            {
                this.FinalizeFading();
                this.ApplyFading(buffer, offset, count);

                return;
            }

            int channels = this.Channels;
            count -= count % channels;

            int sampleIndex = offset;

            float step = this._step;
            float currentVolume = this._currentVolume;

            while ((sampleIndex - offset) < count)
            {
                for (int i = 0; i < channels; ++i)
                {
                    buffer[sampleIndex++] *= currentVolume;
                }

                currentVolume += step;
            }

            this._currentVolume = currentVolume;

            if (this.IsFadingFinished())
            {
                this.FinalizeFading();
                int remaining = count - (sampleIndex - offset);
                if (remaining > 0)
                {
                    this.ApplyFading(buffer, sampleIndex, remaining); //apply the rest
                }
            }
        }

        private bool IsFadingFinished()
        {
            var (currentVolume, targetVolume, step) = (this._currentVolume, this._targetVolume, this._step);

            return (Math.Abs(currentVolume - targetVolume) < Math.Abs(step))
                || (step > 0.0f && currentVolume > targetVolume)
                || (step < 0.0f && currentVolume < targetVolume);
        }

        private void StopFadingInternal()
        {
            this._targetVolume = _currentVolume;
            this.IsFading = false;
        }

        private void FinalizeFading()
        {
            this._currentVolume = _targetVolume;
            this.IsFading = false;
            this.OnFadingFinished();
        }

        private void OnFadingFinished()
        {
            this.FadingFinished?.Invoke(this, EventArgs.Empty);
        }
    }
}
