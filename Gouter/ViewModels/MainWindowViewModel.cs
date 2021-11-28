using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using Gouter.Managers;
using Gouter.Messaging;
using Gouter.Players;
using Livet.Messaging;

namespace Gouter.ViewModels
{
    /// <summary>
    /// <see cref="Views.MainWindow"/>のViewModel
    /// </summary>
    internal sealed class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private static readonly App _app = App.Instance;

        /// <summary>
        /// スレッド
        /// </summary>
        private Dispatcher _dispatcher = _app.Dispatcher;

        /// <summary>
        /// メッセンジャー
        /// </summary>
        public InteractionMessenger Messenger { get; } = new();

        public MediaManager MediaManager { get; } = _app.MediaManager;

        public PlaylistPlayer Player { get; } = _app.MediaPlayer;

        public PlaylistManager Playlists => this.MediaManager.Playlists;

        public SortedNotifiableCollectionWrapper<AlbumPlaylist> Albums { get; }

        private AlbumPlaylist _selectedAlbumPlaylist;
        public AlbumPlaylist SelectedAlbumPlaylist
        {
            get => this._selectedAlbumPlaylist;
            set
            {
                if (this.SetProperty(ref this._selectedAlbumPlaylist, value))
                {
                    this.AlbumTracks = this._dispatcher.Invoke(() => value != null ? new AlbumTrackViewModel(value) : null);
                }
            }
        }

        private double _verticalOffset;
        public double VerticalOffset
        {
            get => this._verticalOffset;
            set => this.SetProperty(ref this._verticalOffset, value);
        }

        public MainWindowViewModel() : base()
        {
            _app.SettingSaving += this.OnSettingSaving;

            this.Albums = new SortedNotifiableCollectionWrapper<AlbumPlaylist>(this.Playlists.Albums, AlbumComparer.Instance);

            var player = this.Player;
            player.PlayStateChanged += this.OnPlayStateChanged;

            var timerInterval = TimeSpan.FromSeconds(0.1d);
            this._timer = new DispatcherTimer(timerInterval, DispatcherPriority.Render, this.OnTimerTick, Dispatcher.CurrentDispatcher);
            this._timer.Tick += this.OnTimerTick;

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        private void OnSettingSaving(object? sender, ApplicationSetting e)
        {
            e.AlbumListScrollPosition = this.VerticalOffset;
        }

        private AlbumTrackViewModel _albumTracks;

        public AlbumTrackViewModel AlbumTracks
        {
            get => this._albumTracks;
            set => this.SetProperty(ref this._albumTracks, value);
        }

        private bool _isInitialized = false;

        /// <summary>
        /// 初期化時
        /// </summary>
        public void OnInitialized()
        {
            if (this._isInitialized)
            {
                return;
            }

            this._isInitialized = true;

            this.MediaManager.Loaded += this.OnMediaManagerLoaded;
            this.MediaManager.TrackRegisterStateChanged += this.OnTrackRegisterStateChanged;

            App.Instance.OnMainViewReady();
        }

        /// <summary>
        /// <see cref="MediaManager"/>読み込み時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMediaManagerLoaded(object sender, EventArgs e)
        {
            this.MediaManager.Loaded -= this.OnMediaManagerLoaded;

            // プレーヤ状態を復元する（暫定）
            var player = this.Player;
            var settings = App.Instance.Setting;

            IPlaylist lastPlaylist = null;
            if (settings.LastPlaylistId != null)
            {
                int lastPlaylistId = (int)settings.LastPlaylistId;
                lastPlaylist = player.MediaManager.Playlists.Albums
                    .FirstOrDefault(a => a.Album.Id == lastPlaylistId);
            }

            if (settings.LastTrackId != null)
            {
                int lastTrackId = (int)settings.LastTrackId;

                var track = player.MediaManager.Tracks.Tracks
                    .FirstOrDefault(t => t.Id == lastTrackId);
                if (track != null)
                {
                    if (lastPlaylist != null)
                    {
                        player.SwitchTrack(track, lastPlaylist);
                        this.SelectedAlbumPlaylist = player.Playlist as AlbumPlaylist;
                    }
                    else
                    {
                        player.SwitchTrack(track);
                    }
                }
            }

            this.VerticalOffset = settings.AlbumListScrollPosition;
        }

        private TaskDialogPage? _registerProgressDialog;

        /// <summary>
        /// トラック情報登録状況の変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackRegisterStateChanged(object sender, TrackRegisterProgress e) => this._dispatcher.InvokeAsync(() =>
        {
            switch (e.State)
            {
                case TrackRegisterState.Collecting:
                    // 新規トラック情報収集中
                    this.OnTrackCollecting(e);
                    break;

                case TrackRegisterState.InProgress:
                    // トラック情報登録中
                    this.OnTrackRegisterInProgress(e);
                    break;

                case TrackRegisterState.NotFound:
                case TrackRegisterState.Complete:
                    // トラック情報収集完了
                    this.OnTrackRegisterFinished(e);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        /// <summary>
        /// トラック情報の登録処理
        /// </summary>
        /// <param name="e"></param>
        private void OnTrackCollecting(TrackRegisterProgress e)
        {
            var instruction = "楽曲情報の登録";
            var message = "新しい楽曲情報を検索しています...";

            var taskDialog = this._registerProgressDialog = new()
            {
                Caption = App.Name,
                Heading = instruction,
                Text = message,
                ProgressBar = new()
                {
                    State = TaskDialogProgressBarState.Marquee,
                    Maximum = default
                },
                Buttons = new TaskDialogButtonCollection
                {
                    new TaskDialogButton(TaskDialogButton.Close.Text, false, false)
                },
            };

            this.Status = message;
            this.Messenger.Raise(new TaskDialogMessage("ShowTrackRegisterDialog", taskDialog));
        }

        /// <summary>
        /// トラック情報登録中
        /// </summary>
        /// <param name="e"></param>
        private void OnTrackRegisterInProgress(TrackRegisterProgress e)
        {
            var taskDialog = this._registerProgressDialog;
            var progress = taskDialog?.ProgressBar;

            if (progress == null)
            {
                return;
            }

            if (progress.State == TaskDialogProgressBarState.Marquee)
            {
                progress.State = TaskDialogProgressBarState.Normal;
                progress.Maximum = e.Total;

                taskDialog.Text = $"{e.Total}件の楽曲が見つかりました。楽曲情報をライブラリに登録しています...";
            }

            progress.Value = e.Current;
        }

        /// <summary>
        /// トラック情報登録完了
        /// </summary>
        /// <param name="e"></param>
        private void OnTrackRegisterFinished(TrackRegisterProgress e)
        {
            ref var taskDialog = ref this._registerProgressDialog;
            if (taskDialog is null)
            {
                return;
            }

            // 登録データがあればステータスに件数を表示する
            this.Status = e.State == TrackRegisterState.Complete
                ? $"{e.Total}件の楽曲を追加しました"
                : null;
            taskDialog.BoundDialog?.Close();

            this._registerProgressDialog = null;
        }

        private IPlaylist _currentPlaylist;
        public IPlaylist CurrentPlaylist
        {
            get => this._currentPlaylist;
            set => this.SetProperty(ref this._currentPlaylist, value);
        }

        private IPlaylist _selectedAlbum;
        public IPlaylist SelectedAlbum
        {
            get => this._selectedAlbum;
            set
            {
                if (this.SetProperty(ref this._selectedAlbum, value) && value != null)
                {
                    this.CurrentPlaylist = value;
                }
            }
        }

        private string _status;
        public string Status
        {
            get => this._status;
            set => this.SetProperty(ref this._status, value);
        }

        private TrackInfo _selectedTrack;
        public TrackInfo SelectedTrack
        {
            get => this._selectedTrack;
            set => this.SetProperty(ref this._selectedTrack, value);
        }

        private IPlaylist _playingPlaylist;
        public IPlaylist PlayingPlaylist
        {
            get => this._playingPlaylist;
            set => this.SetProperty(ref this._playingPlaylist, value);
        }

        private readonly LinkedList<TrackInfo> _playHistory = new LinkedList<TrackInfo>();

        public const int MaxHistoryCount = 50;

        public bool IsPlayRequired { get; set; }

        private bool _isOpenAlbumPlaylistTrackList = false;
        public bool IsOpenAlbumPlaylistTrackList
        {
            get => this._isOpenAlbumPlaylistTrackList;
            set => this.SetProperty(ref this._isOpenAlbumPlaylistTrackList, value);
        }

        private double _currentTime = 0.0d;
        private double _duration = 0.0d;

        /// <summary>
        /// 楽曲の現在位置
        /// </summary>
        public double CurrentTime
        {
            get => this._currentTime;
            set => this.SetProperty(ref this._currentTime, value);
        }

        /// <summary>
        /// 楽曲の長さ(尺)
        /// </summary>
        public double Duration
        {
            get => this._duration;
            set => this.SetProperty(ref this._duration, value);
        }

        private readonly DispatcherTimer _timer;

        private void OnPlayStateChanged(object sender, PlayState state)
        {
            switch (state)
            {
                case PlayState.Play:
                    this.Duration = this.Player.GetDuration().TotalMilliseconds;
                    this._timer.Start();
                    this.UpdateTime();
                    break;

                case PlayState.Pause:
                case PlayState.Stop:
                    this._timer.Start();
                    this.UpdateTime();
                    break;
            }
        }

        private Command _playCommand;
        private Command _previousTrackCommand;
        private Command _nextTrackCommand;

        /// <summary>
        /// 再生コマンド
        /// </summary>
        public Command PlayCommand => this._playCommand ??= this.Commands.Create(() =>
        {
            if (this.Player.IsPlaying)
            {
                this.IsPlayRequired = false;
                this.Player.Pause();
            }
            else
            {
                this.IsPlayRequired = true;
                this.Player.Play();
            }
        });

        /// <summary>
        /// 次トラックへの変更コマンド
        /// </summary>
        public Command NextTrackCommand => this._nextTrackCommand
            ??= this.Commands.Create(() => this.Player.PlayNext());

        /// <summary>
        /// 前トラックへの変更コマンド
        /// </summary>
        public Command PreviousTrackCommand => this._previousTrackCommand
            ??= this.Commands.Create(() => this.Player.PlayPrevious());

        /// <summary>
        /// ミュート状態を取得または設定する
        /// </summary>
        public bool IsMuted
        {
            get => this.Player.IsMuted;
            set => this.Player.IsMuted = value;
        }

        public bool IsSeeking { get; set; }

        private void UpdateTime()
        {
            if (!this.IsSeeking && this.Player.Track != null)
            {
                this.CurrentTime = this.Player.GetPosition().TotalMilliseconds;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            this.UpdateTime();
        }

        /// <summary>
        /// ループモードの表示名
        /// </summary>
        public IReadOnlyDictionary<LoopMode, string> LoopModes { get; } = new Dictionary<LoopMode, string>
        {
            [LoopMode.None] = "リピートなし",
            [LoopMode.SingleTrack] = "1曲のみリピート",
            [LoopMode.Playlist] = "プレイリストをリピート",
        };

        /// <summary>
        /// シャッフルモードの表示名
        /// </summary>
        public IReadOnlyDictionary<ShuffleMode, string> ShuffleModes { get; } = new Dictionary<ShuffleMode, string>
        {
            [ShuffleMode.None] = "シャッフルなし",
            [ShuffleMode.Random] = "プレイリストをシャッフル",
        };

        /// <summary>
        /// ループモード
        /// </summary>
        public LoopMode LoopMode
        {
            get => this.Player.Options.LoopMode;
            set
            {
                this.Player.Options.LoopMode = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// シャッフルモード
        /// </summary>
        public ShuffleMode ShuffleMode
        {
            get => this.Player.Options.ShuffleMode;
            set
            {
                this.Player.Options.ShuffleMode = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// ウィンドウを閉じた場合
        /// </summary>
        public void OnClosed()
        {
        }

        /// <summary>
        /// 破棄時
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Player.PlayStateChanged -= this.OnPlayStateChanged;
            this.MediaManager.TrackRegisterStateChanged += this.OnTrackRegisterStateChanged;
        }
    }
}
