using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using Gouter.Managers;

namespace Gouter.Players
{
    /// <summary>
    /// メディア再生管理を行うクラス
    /// </summary>
    internal class MediaPlayer : NotificationObject, IDisposable, ISubscribable<IMediaPlayerObserver>, ISoundPlayerObserver
    {
        /// <summary>
        /// メディア管理クラス
        /// </summary>
        public MediaManager MediaManager { get; }

        /// <summary>
        /// 出力デバイス
        /// </summary>
        private ISoundOut _audioRenderer;

        /// <summary>
        /// オブザーバ
        /// </summary>
        private readonly IList<IMediaPlayerObserver> _observers = new List<IMediaPlayerObserver>();

        private bool _isDisposed;

        private TrackInfo _track;
        /// <summary>
        /// 再生中のトラック情報
        /// </summary>
        public TrackInfo Track
        {
            get => this._track;
            private set => this.SetProperty(ref this._track, value);
        }

        private IPlaylist _playlist;

        /// <summary>
        /// 再生中のプレイリスト情報
        /// </summary>
        public IPlaylist Playlist
        {
            get => this._playlist;
            private set => this.SetProperty(ref this._playlist, value);
        }

        /// <summary>
        /// サウンドプレーヤ
        /// </summary>
        private readonly SoundFilePlayer _player = new SoundFilePlayer();

        /// <summary>
        /// 再生状態
        /// </summary>
        public PlayState State { get; private set; } = PlayState.Stop;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mediaManager">メディア管理クラス</param>
        public MediaPlayer(MediaManager mediaManager) : base()
        {
            this._player.Subscribe(this);
            this.MediaManager = mediaManager;

            this._audioRenderer = GetTempAudioRenderer();
            this._player.SetSoundDevice(this._audioRenderer);
        }

        /// <summary>
        /// 仮の出力デバイスを取得する。
        /// </summary>
        /// <returns></returns>
        private static ISoundOut GetTempAudioRenderer()
            => new WasapiOut(false, AudioClientShareMode.Shared, 1)
            {
                Device = App.Instance.SoundDeviceListener.SystemDefault.GetDevice(),
            };

        public ValueTask ChangeTrack(TrackInfo track) => this.ChangeTrack(track, null);

        public async ValueTask ChangeTrack(TrackInfo track, IPlaylist nextPlaylist)
        {
            if (this.Track != track && this.State != PlayState.Stop)
            {
                // トラックが再生中であれば停止する
                await this._player.StopAndWait().ConfigureAwait(false);
            }

            if (nextPlaylist != null && this.Playlist != nextPlaylist)
            {
                this.Playlist = nextPlaylist;
            }

            var playlist = this.Playlist;

            if (playlist == null || !playlist.Tracks.Contains(track))
            {
                throw new InvalidOperationException();
            }

            if (this.Track != track)
            {
                this.Track = track;
                this._player.ChangeSoundSource(track.Path);
            }
        }
        /// <summary>
        /// 再生を行う
        /// </summary>
        public async void Play()
            => await this._player.Play();

        /// <summary>
        /// 再生中かどうかを取得する。
        /// </summary>
        public bool IsPlaying => this.State == PlayState.Play;

        /// <summary>
        /// 再生を一時停止する
        /// </summary>
        public void Pause()
            => this._player.Pause();

        /// <summary>
        /// 一時停止中かどうかを取得する。
        /// </summary>
        public bool IsPausing => this.State == PlayState.Pause;

        /// <summary>
        /// 再生を停止する。デバイスの再生処理終了の待機は行わない
        /// </summary>
        public void Stop()
            => this._player.Stop();

        /// <summary>
        /// 停止中かどうかを取得する。
        /// </summary>
        public bool IsStopping => this.State == PlayState.Stop;

        /// <summary>
        /// 再生位置を取得する
        /// </summary>
        /// <returns>再生位置</returns>
        public TimeSpan GetPosition()
            => this._player.GetPosition();

        /// <summary>
        /// 再生位置を設定する
        /// </summary>
        /// <param name="position">再生位置</param>
        public void SetPosition(TimeSpan position)
            => this._player.SetPosition(position);

        /// <summary>
        /// 通知オブジェクトの購読を行う。
        /// </summary>
        /// <param name="observer">通知オブジェクト</param>
        public void Subscribe(IMediaPlayerObserver observer)
            => this._observers.Add(observer);

        /// <summary>
        /// 通知オブジェクトの購読解除を行う。
        /// </summary>
        /// <param name="observer">通知オブジェクト</param>
        public void Describe(IMediaPlayerObserver observer)
            => this._observers.Remove(observer);

        /// <summary>
        /// インスタンスを破棄する。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this._player.Dispose();
                this._observers.DescribeAll(this);
            }

            this._isDisposed = true;
        }

        /// <summary>
        /// インスタンスを破棄する。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 内部プレーヤの状態が変化した
        /// </summary>
        /// <param name="state">状態</param>
        void ISoundPlayerObserver.OnPlayStateChanged(PlayState state)
        {
            this.State = state;
            this.RaisePropertyChanged(nameof(this.IsPlaying));
            this.RaisePropertyChanged(nameof(this.IsPausing));
            this.RaisePropertyChanged(nameof(this.IsStopping));
        }
    }
}
