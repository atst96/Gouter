using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;

namespace Gouter
{
    internal class SoundPlayer : NotificationObject, IDisposable
    {
        private readonly DispatcherTimer _timer;
        private IWaveSource _soundSource;
        private ISoundOut _soundOut;

        public event EventHandler<EventArgs> TrackPlayingEnded;

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => this._isPlaying;
            private set => this.SetProperty(ref this._isPlaying, value);
        }

        private PlayState _state = PlayState.Stop;
        public PlayState State
        {
            get => this._state;
            private set
            {
                if (this.SetProperty(ref this._state, value))
                {
                    this.IsPlaying = value == PlayState.Play;
                }
            }
        }

        private double _currentTime;
        public double CurrentTime
        {
            get => this._currentTime;
            set
            {
                if (this.SetProperty(ref this._currentTime, value) && value >= 0 && value < this.Duration)
                {
                    this._soundSource?.SetPosition(TimeSpan.FromMilliseconds(value));
                }
            }
        }

        private double _duration;
        public double Duration
        {
            get => this._duration;
            private set => this.SetProperty(ref this._duration, value);
        }

        private float _volume = 1.0f;
        public float Volumne
        {
            get => this._volume;
            set
            {
                if (this.SetProperty(ref this._volume, value))
                {
                    if (this._soundOut != null)
                    {
                        this._soundOut.Volume = value;
                    }
                }
            }
        }

        public SoundPlayer()
        {
            this._timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100),
            };

            this._timer.Tick += this.OnTimerTicked;
        }

        private void OnTimerTicked(object sender, EventArgs e)
        {
            if (this._soundOut == null)
            {
                return;
            }

            this.OnCurrentTimeUpdated();

            if (this._soundOut.PlaybackState == PlaybackState.Stopped)
            {
                this.StopTimer();
            }
        }

        private TrackInfo _currentTrack;
        public TrackInfo CurrentTrack
        {
            get => this._currentTrack;
            private set => this.SetProperty(ref this._currentTrack, value);
        }

        public void SetTrack(TrackInfo trackInfo)
        {
            this.Stop();

            this.InitializeSoundDevice();

            var previousTrack = this.CurrentTrack;

            //if (object.ReferenceEquals(previousTrack, trackInfo))
            //{
            //    this._soundOut?.Resume();
            //    this._soundSource?.SetPosition(TimeSpan.Zero);

            //    return;
            //}

            if (previousTrack != null)
            {
                previousTrack.SetPlayState(false);
            }

            this.CurrentTrack = trackInfo;

            this._soundSource = CodecFactory.Instance.GetCodec(trackInfo.Path);
            this.CurrentTime = this._soundSource.GetPosition().TotalMilliseconds;
            this.Duration = this._soundSource.GetLength().TotalMilliseconds;

            if (this._soundOut.PlaybackState != PlaybackState.Stopped)
            {
                this._soundOut.Stop();
            }

            this._soundOut.Initialize(this._soundSource);
        }

        private void InitializeSoundDevice()
        {
            if (this._soundOut != null)
            {
                return;
            }

            this._soundOut = new WasapiOut
            {
                Device = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Render, Role.Multimedia),
                Latency = 100,
            };

            this._soundOut.Stopped += this.OnPlayerStopped;
        }

        public async void Play()
        {
            if (this.CurrentTrack == null || this.State == PlayState.Play)
            {
                return;
            }

            this.InitializeSoundDevice();

            await this.WaitPlayerStop();

            this._soundOut.Volume = this.Volumne;
            this.StartTimer();
            this.State = PlayState.Play;
            this._isPlayerStopped = false;
            this._soundOut.Play();

            this.CurrentTrack.SetPlayState(true);
        }

        public void Play(TrackInfo trackInfo)
        {
            this.SetTrack(trackInfo);

            this.Play();
        }

        public async Task WaitPlayerStop()
        {
            while (!this._isPlayerStopped)
            {
                await Task.Delay(20).ConfigureAwait(false);
            }
        }

        private bool _isPlayerStopped = true;

        private void OnPlayerStopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (this._isPlayerStopped)
            {
                return;
            }

            bool isEndOfTrack = this.State == PlayState.Play;

            this.StopTimer();

            this.State = PlayState.Stop;

            this.CurrentTrack?.SetPlayState(false);

            this._isPlayerStopped = true;

            if (isEndOfTrack)
            {
                this.TrackPlayingEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Pause()
        {
            this._isPlayerStopped = true;
            this.State = PlayState.Pause;
            this.StopTimer();
            this._soundOut?.Pause();
        }

        public void Stop()
        {
            if (this.State == PlayState.Stop)
            {
                return;
            }

            this.CurrentTrack?.SetPlayState(false);

            if (this._soundOut != null)
            {
                this._soundOut.Stop();
            }

            this.State = PlayState.Stop;
            this.StopTimer();

            if (this._soundSource != null)
            {
                this._soundSource.Position = 0;
            }
        }

        public void Dispose()
        {
            this.Stop();
            this._soundSource?.Dispose();
            this._soundOut?.Dispose();
        }

        private void OnCurrentTimeUpdated()
        {
            if (this._soundSource == null)
            {
                return;
            }

            double timeMs = this._soundSource.GetPosition().TotalMilliseconds;

            this.SetProperty(ref this._currentTime, timeMs, nameof(SoundPlayer.CurrentTime));
        }

        private void StartTimer()
        {
            this._timer.Start();
        }

        private void StopTimer()
        {
            this._timer.Stop();
        }
    }
}
