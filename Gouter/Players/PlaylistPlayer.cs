using System;
using System.Collections.Generic;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using Gouter.Extensions;
using Gouter.Managers;

namespace Gouter.Players
{
    /// <summary>
    /// メディア再生管理を行うクラス
    /// </summary>
    internal class PlaylistPlayer : NotificationObject, IDisposable
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// メディア管理クラス
        /// </summary>
        public MediaManager MediaManager { get; }

        /// <summary>
        /// オーディオ出力先
        /// </summary>
        private ISoundOut _audioRenderer;

        /// <summary>
        /// プレーや設定の内部変数
        /// </summary>
        private IPlayerOptions _options;

        /// <summary>
        /// プレーヤ設定
        /// </summary>
        public IPlayerOptions Options
        {
            get => this._options;
            set
            {
                this._options = value ?? throw new ArgumentNullException(nameof(this.Options));

                if (this._player != null)
                {
                    this._player.Options = value;
                }
            }
        }

        /// <summary>
        /// 再生時のボリューム
        /// </summary>
        public float Volume
        {
            get => this._player.Volume;
            set
            {
                this._player.Volume = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 再生履歴の最大数を取得または設定する。
        /// </summary>
        public int MaxHistoryCount { get; set; } = 50;

        // 現在の再生履歴の位置
        private LinkedListNode<TrackInfo> _currentHistoryNode;

        // 再生履歴リスト
        private LinkedList<TrackInfo> _playHistory = new LinkedList<TrackInfo>();

        private bool _isDisposed;

        private TrackInfo _track;

        /// <summary>
        /// 再生状態の変更時に呼び出されるイベントハンドラ
        /// </summary>
        public event EventHandler<PlayState> PlayStateChanged;

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
        private readonly SoundFilePlayer _player;

        /// <summary>
        /// 再生状態
        /// </summary>
        public PlayState State { get; private set; } = PlayState.Stop;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mediaManager">メディア管理クラス</param>
        public PlaylistPlayer(MediaManager mediaManager, IPlayerOptions options)
            : base()
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));

            var player = new SoundFilePlayer(options);
            player.PlayStateChanged += this.OnPlayStateChanged;
            player.TrackFinished += this.OnTrackFinished;
            player.PlayFailed += this.OnPlayerFailed;

            this._player = player;

            this.MediaManager = mediaManager;

            this._audioRenderer = GetTempAudioRenderer();
            player.SetSoundDevice(this._audioRenderer);
        }

        /// <summary>
        /// 仮の出力デバイスを取得する。
        /// </summary>
        /// <returns></returns>
        private static ISoundOut GetTempAudioRenderer()
            => new WasapiOut(false, AudioClientShareMode.Shared, 100)
            {
                Device = App.Instance.SoundDeviceListener.SystemDefault.GetDevice(),
            };

        /// <summary>
        /// トラックを切り替える。
        /// </summary>
        /// <param name="track"></param>
        /// <param name="isClearHistory"></param>
        /// <returns></returns>
        public void SwitchTrack(TrackInfo track, bool isClearHistory = true, bool isUpdateHistory = true)
            => this.SwitchTrack(track, null, isClearHistory, isUpdateHistory);

        /// <summary>
        /// 再生トラックを切り替える。
        /// </summary>
        /// <param name="track"></param>
        /// <param name="nextPlaylist"></param>
        /// <param name="isClearHistory"></param>
        /// <returns></returns>
        public void SwitchTrack(TrackInfo track, IPlaylist nextPlaylist, bool isClearHistory = true, bool isUpdateHistory = true)
        {
            bool isTrackChanged = this.Track != track;
            if (!isTrackChanged)
            {
                this._player.Seek(TimeSpan.Zero);
            }
            else
            {
                this.Track = track;
                if (isClearHistory)
                {
                    this._playHistory.Clear();
                }

                if (isUpdateHistory)
                {
                    this.AddPlayHistory(track);
                }
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

            if (isTrackChanged)
            {
                this._player.ChangeSource(track);
            }
        }

        /// <summary>
        /// 再生を行う
        /// </summary>
        public void Play()
        {
            this._player.Play();
        }

        public void Play(TrackInfo track, IPlaylist nextPlaylist = null, bool isClearHistory = true, bool isUpdateHistory = true)
        {
            this.SwitchTrack(track, nextPlaylist, isClearHistory, isUpdateHistory);
            this.Play();
        }

        /// <summary>
        /// 前のトラックを再生する。
        /// </summary>
        /// <returns></returns>
        public void PlayPrevious()
        {
            var playlist = this._playlist;
            if (playlist == null)
            {
                return;
            }

            var previousNode = this._currentHistoryNode?.Previous;

            // TODO: 巻き戻し時間の閾値を設定データに持てるようにする
            var rewindDuration = TimeSpan.FromSeconds(3.0);

            if (this.GetPosition() > rewindDuration || previousNode?.Value == default)
            {
                // 再生履歴なし or 再生位置が指定時間以上
                this.Seek(TimeSpan.Zero);
                this.Play();
            }
            else
            {
                this._currentHistoryNode = previousNode;
                this.Play(previousNode.Value, isClearHistory: false, isUpdateHistory: false);
            }

            this.Play();
        }

        /// <summary>
        /// 次のトラックを再生する。
        /// </summary>
        /// <returns></returns>
        public void PlayNext()
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

            this.Play(nextTrack, isClearHistory: false);
        }

        /// <summary>
        /// 次のトラックを選択する。
        /// </summary>
        /// <param name="currentTrack">現在のトラック</param>
        /// <param name="playlist">プレイリスト</param>
        /// <returns></returns>
        public TrackInfo GetNextTrack(TrackInfo currentTrack, IPlaylist playlist)
        {
            var options = this.Options;
            if (options == null || this.Options.LoopMode == LoopMode.SingleTrack)
            {
                return currentTrack;
            }

            var tracks = playlist.Tracks;
            var shuffleMode = options.ShuffleMode;

            int currentTrackIndex = tracks.IndexOf(currentTrack);

            switch (shuffleMode)
            {
                case ShuffleMode.None:
                    {
                        // シャッフルモードでない場合
                        // 次のトラックを選択する
                        int nextTrackIndex = currentTrackIndex + 1;
                        if (nextTrackIndex >= tracks.Count)
                        {
                            // トラックのインデックスが範囲外であれば最初のトラックに戻る
                            nextTrackIndex = 0;
                        }

                        return tracks[nextTrackIndex];
                    }

                case ShuffleMode.Random:
                    {
                        // ランダムシャッフルの場合

                        int tracksCount = tracks.Count;

                        if (tracksCount <= 1)
                        {
                            // 1トラック登録されている場合
                            return currentTrack;
                        }

                        if (tracksCount == 2 && options.IsShuffleAvoidCurrentTrack)
                        {
                            // 2トラック登録されている & 同一トラック回避の場合
                            return tracks[1 - currentTrackIndex];
                        }

                        int nextTrackIndex;
                        do
                        {
                            nextTrackIndex = random.Next(0, tracks.Count);
                        }
                        while (nextTrackIndex == currentTrackIndex && options.IsShuffleAvoidCurrentTrack);

                        return tracks[nextTrackIndex];
                    }

                default:
                    throw new NotImplementedException();
            }
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
        /// 再生履歴を整理する。
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
        /// 再生を一時停止する。
        /// </summary>
        public void Pause() => this._player.Pause();

        /// <summary>
        /// 一時停止中かどうかを取得する。
        /// </summary>
        public bool IsPausing => this.State == PlayState.Pause;

        /// <summary>
        /// 再生を停止する。
        /// </summary>
        public void Stop()
        {
            this._player.Stop();
        }

        /// <summary>
        /// 停止中かどうかを取得する。
        /// </summary>
        public bool IsStopping => this.State == PlayState.Stop;

        /// <summary>
        /// 再生位置を取得する
        /// </summary>
        /// <returns>再生位置</returns>
        public TimeSpan GetPosition()
        {
            return this._player.GetPosition();
        }

        /// <summary>
        /// 楽曲の長さ(尺)を取得する。
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetDuration()
        {
            return this._player.GetDuration();
        }

        /// <summary>
        /// 再生位置を設定する
        /// </summary>
        /// <param name="position">再生位置</param>
        public void Seek(TimeSpan position)
        {
            this._player.Seek(position);
        }

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
                var player = this._player;
                player.PlayStateChanged -= this.OnPlayStateChanged;
                player.TrackFinished -= this.OnTrackFinished;
                player.PlayFailed -= this.OnPlayerFailed;
                player.Dispose();
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
        private void OnPlayStateChanged(object sender, PlayState state)
        {
            // トラック変更中はステータスを変更しない
            this.State = state;
            this.RaisePropertyChanged(nameof(this.IsPlaying));
            this.RaisePropertyChanged(nameof(this.IsPausing));
            this.RaisePropertyChanged(nameof(this.IsStopping));

            this.PlayStateChanged?.Invoke(this, state);
        }

        /// <summary>
        /// 再生失敗時
        /// </summary>
        /// <param name="ex"></param>
        private void OnPlayerFailed(object sender, Exception ex)
        {
        }

        /// <summary>
        /// 現在トラックの再生完了時
        /// </summary>
        private void OnTrackFinished(object sender, EventArgs e)
        {
            var options = this.Options;
            if (options.LoopMode == LoopMode.None)
            {
                // ループしない場合
                return;
            }

            this.PlayNext();
        }
    }
}
