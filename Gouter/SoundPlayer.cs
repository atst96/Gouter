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

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => this._isPlaying;
            private set => this.SetProperty(ref this._isPlaying, value);
        }

        private PlayState _state;
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
            private set => this.SetProperty(ref this._currentTime, value);
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
                Interval = TimeSpan.FromMilliseconds(200d),
            };
            this._timer.Tick += this.OnTimerTicked;
        }

        private void OnTimerTicked(object sender, EventArgs e)
        {
            this.CurrentTime = this._soundSource.GetTime(this._soundSource.Position).TotalMilliseconds;
        }

        private IPlaylist _playlist;
        public IPlaylist Playlist
        {
            get => this._playlist;
            private set => this.SetProperty(ref this._playlist, value);
        }

        private TrackInfo _currentTrack;
        public TrackInfo CurrentTrack
        {
            get => this._currentTrack;
            private set => this.SetProperty(ref this._currentTrack, value);
        }

        public void ChangePlaylist(IPlaylist playlist)
        {
            this.Stop();

            this.Playlist = playlist;
        }

        public void Play(TrackInfo trackInfo)
        {
            this.PlayItem(trackInfo);
        }

        public void Play()
        {
            this.PlayItem(this.CurrentTrack);
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
            this._soundOut.Stopped += this.OnSoundOutStopped;
        }

        private void PlayItem(TrackInfo trackInfo)
        {
            this.InitializeSoundDevice();

            this.CurrentTrack = trackInfo;

            this._soundSource = CodecFactory.Instance.GetCodec(trackInfo.Path);
            this.CurrentTime = this._soundSource.GetTime(this._soundSource.Position).TotalMilliseconds;
            this.Duration = this._soundSource.GetTime(this._soundSource.Length).TotalMilliseconds;
            if (this._soundOut.PlaybackState != PlaybackState.Stopped)
            {
                this._soundOut.Stop();
            }
            this._soundOut.Initialize(this._soundSource);
            this._soundOut.Volume = this.Volumne;
            this._soundOut.Play();
            this._timer.Start();
            this.State = PlayState.Play;
        }

        private void OnSoundOutStopped(object sender, PlaybackStoppedEventArgs e)
        {
            this._timer.Start();
        }

        public void Pause()
        {
            this.State = PlayState.Pause;
            this._timer.Stop();
            this._soundOut?.Pause();
        }

        public void Stop()
        {
            this.State = PlayState.Stop;
            this._soundOut?.Stop();
            this._timer.Stop();
            if (this._soundSource != null)
            {
                this._soundSource.Position = 0;
            }
        }

        public void Dispose()
        {
            this._soundSource?.Dispose();
            this._soundOut?.Dispose();
        }
    }
}
