using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundOut;
using Gouter.Extensions;
using Gouter.Managers;

namespace Gouter.Players
{
    /// <summary>
    /// メディア再生管理を行うクラス
    /// </summary>
    internal class MediaPlayer : NotificationObject, IDisposable, ISubscribable<IMediaPlayerObserver>, ISoundPlayerObserver
    {
        private volatile bool _isTrackChanging = false;
        private static Random _rand = new Random();

        /// <summary>
        /// メディア管理クラス
        /// </summary>
        public MediaManager MediaManager { get; }

        /// <summary>
        /// オーディオ出力先
        /// </summary>
        private ISoundOut _audioRenderer;

        /// <summary>
        /// ループモード
        /// </summary>
        public LoopMode LoopMode { get; set; } = LoopMode.Playlist;

        /// <summary>
        /// シャッフルモード
        /// </summary>
        public ShuffleMode ShuffleMode { get; set; } = ShuffleMode.Random;

        /// <summary>
        /// 再生履歴の最大数を取得または設定する。
        /// </summary>
        public int MaxHistoryCount { get; set; } = 50;

        // 現在の再生履歴の位置
        private LinkedListNode<TrackInfo> _currentHistoryNode;

        // 再生履歴リスト
        private LinkedList<TrackInfo> _playHistory = new LinkedList<TrackInfo>();

        // オブザーバ
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

        /// <summary>
        /// トラックを切り替える。
        /// </summary>
        /// <param name="track"></param>
        /// <param name="isClearHistory"></param>
        /// <returns></returns>
        public ValueTask SwitchTrack(TrackInfo track, bool isClearHistory = true, bool isUpdateHistory = true)
            => this.SwitchTrack(track, null, isClearHistory, isUpdateHistory);

        /// <summary>
        /// トラックを切り替える。
        /// </summary>
        /// <param name="track"></param>
        /// <param name="nextPlaylist"></param>
        /// <param name="isClearHistory"></param>
        /// <returns></returns>
        public async ValueTask SwitchTrack(TrackInfo track, IPlaylist nextPlaylist, bool isClearHistory = true, bool isUpdateHistory = true)
        {
            if (isClearHistory)
            {
                this._playHistory.Clear();
            }

            if (isUpdateHistory)
            {
                this.AddPlayHistory(track);
            }

            if (this.Track != track && this.State != PlayState.Stop)
            {
                // 再生中であれば停止する
                this._isTrackChanging = true;

                await this._player.StopAndWait().ConfigureAwait(false);

                this._isTrackChanging = false;
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
        public ValueTask Play() => this._player.Play();

        /// <summary>
        /// 前のトラックを再生する。
        /// </summary>
        /// <returns></returns>
        public async ValueTask PlayPrevious()
        {
            var playlist = this._playlist;
            if (playlist == null)
            {
                return;
            }

            var previousNode = this._currentHistoryNode?.Previous;

            var temp = TimeSpan.FromSeconds(3.0);

            if (this.GetPosition() > temp || previousNode?.Value == default)
            {
                // 再生履歴なし or 再生位置が指定時間以上
                this.SetPosition(TimeSpan.Zero);
            }
            else
            {
                await this.SwitchTrack(previousNode.Value, false, false).ConfigureAwait(false);
                this._currentHistoryNode = previousNode;
            }

            await this.Play().ConfigureAwait(false);
        }

        /// <summary>
        /// 次のトラックを再生する。
        /// </summary>
        /// <returns></returns>
        public async ValueTask PlayNext()
        {
            var playlist = this._playlist;
            if (playlist == null)
            {
                return;
            }

            TrackInfo nextTrack;

            // 再生履歴に次のトラックがある場合
            var nextHistoryNode = this._currentHistoryNode.Next;
            if (nextHistoryNode != null)
            {
                nextTrack = nextHistoryNode.Value;
            }
            else
            {
                nextTrack = this.GetNextTrack(this._currentHistoryNode.Value, playlist);
            }

            await this.SwitchTrack(nextTrack, false).ConfigureAwait(false);
            await this.Play().ConfigureAwait(false);
        }

        /// <summary>
        /// 次のトラックを選択する。
        /// </summary>
        /// <param name="currentTrack">現在のトラック</param>
        /// <param name="playlist">プレイリスト</param>
        /// <returns></returns>
        public TrackInfo GetNextTrack(TrackInfo currentTrack, IPlaylist playlist)
        {
            if (this.LoopMode == LoopMode.SingleTrack)
            {
                return currentTrack;
            }

            var tracks = playlist.Tracks;

            if (this.ShuffleMode == ShuffleMode.None)
            {
                int trackIndex = tracks.IndexOf(currentTrack);
                int index = trackIndex < tracks.Count - 1 ? trackIndex + 1 : 0;

                return tracks[index];
            }
            else if (this.ShuffleMode == ShuffleMode.Random)
            {
                int index = _rand.Next(0, tracks.Count - 1);

                return tracks[index];
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// トラックを再生履歴に追加する。
        /// </summary>
        /// <param name="nextTrack">次のトラック</param>
        public void AddPlayHistory(TrackInfo nextTrack)
        {
            var history = this._playHistory;
            var currentNode = this._currentHistoryNode;

            if (history.Count == 0)
            {
                // 再生履歴がない場合
                this._currentHistoryNode = history.AddLast(nextTrack);
            }
            else if (currentNode.Value != nextTrack)
            {
                // 再生履歴あり
                this._currentHistoryNode = history.AddAfter(currentNode, nextTrack);

                history.RemoveAfterAll(this._currentHistoryNode.Next);
            }

            this.OrganizeHistory();
        }

        /// <summary>
        /// 再生履歴を整理する
        /// </summary>
        private void OrganizeHistory()
        {
            var history = this._playHistory;

            // 履歴件数がMaxCountを超える場合は先頭の履歴を削除
            while (history.Count > 0 && history.Count > this.MaxHistoryCount)
            {
                history.RemoveFirst();
            }
        }

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
        {
            if (!this._observers.Contains(observer))
            {
                this._observers.Add(observer);
            }
        }

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

            if (state == PlayState.Stop)
            {
                // スキップ処理を見直す
                if (!this._isTrackChanging)
                {
                    if (this.LoopMode == LoopMode.None)
                    {
                        return;
                    }

                    this.PlayNext();
                }
            }
        }
    }
}
