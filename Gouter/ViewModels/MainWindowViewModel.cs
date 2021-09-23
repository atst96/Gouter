using Gouter.Commands.MainWindow;
using Gouter.Components.Mvvm;
using Gouter.Managers;
using Gouter.Players;
using Livet.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using Gouter.Messaging;

namespace Gouter.ViewModels
{
    internal sealed class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly Random _rand = new Random();

        private static readonly App _app = App.Instance;

        /// <summary>
        /// スレッド
        /// </summary>
        private Dispatcher _thread = _app.Dispatcher;

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
                    this.AlbumTracks = this._thread.Invoke(() => value != null ? new AlbumTrackViewModel(value) : null);
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
            _app.SettingSaving += OnSettingSaving;

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

        private TaskDialogPage _taskDialog;
        private TaskDialogButton _taskDialogButton;

        /// <summary>
        /// トラック情報登録状況の変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackRegisterStateChanged(object sender, TrackRegisterProgress e) =>
            this._thread.InvokeAsync(() =>
            {
                var taskDialog = this._taskDialog;
                var button = this._taskDialogButton;

                switch (e.State)
                {
                    case TrackRegisterState.Finding:
                        button = this._taskDialogButton = new(TaskDialogButton.Close.Text, false, false);
                        taskDialog = this._taskDialog = new()
                        {
                            Caption = App.Name,
                            Heading = "楽曲の登録",
                            Text = "楽曲情報を検索しています...",
                            ProgressBar = new()
                            {
                                State = TaskDialogProgressBarState.Marquee,
                                Maximum = default
                            },
                            Buttons = new TaskDialogButtonCollection { button },
                        };

                        this.Status = "楽曲情報を検索しています...";
                        this.Messenger.Raise(new TaskDialogMessage("ShowTrackRegisterDialog", taskDialog));

                        break;

                    case TrackRegisterState.NotFound:
                        this.Status = null;
                        taskDialog.BoundDialog?.Close();
                        this._taskDialogButton = null;
                        this._taskDialog = null;
                        break;

                    case TrackRegisterState.Registering:
                        var progress = taskDialog.ProgressBar;
                        if (progress.Maximum == default)
                        {
                            progress.State = TaskDialogProgressBarState.Normal;
                            progress.Maximum = e.Total;

                            taskDialog.Text = $"{e.Total}件の楽曲が見つかりました。楽曲情報をライブラリに登録しています...";
                        }

                        progress.Value = e.Current;
                        break;

                    case TrackRegisterState.Complete:
                        button.Enabled = true;
                        taskDialog.BoundDialog?.Close();
                        this.Status = $"{e.Total}件の楽曲を追加しました";
                        this._taskDialogButton = null;
                        this._taskDialog = null;
                        break;
                }
            });

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

        private Command _playCommand;
        public Command PlayCommand => this._playCommand ??= new PlayCommand(this);

        private Command _previousTrackCommand;
        public Command PreviousTrackCommand => this._previousTrackCommand ??= new PreviousTrackCommand(this);

        private Command _nextTrackCommand;
        public Command NextTrackCommand => this._nextTrackCommand ??= new NextTrackCommand(this);

        /// <summary>
        /// ウィンドウを閉じた場合
        /// </summary>
        public void OnClosed()
        {
        }

        private Command<AlbumPlaylist> _selectAlbumPlaylistCommand;
        public Command<AlbumPlaylist> SelectAlbumPlaylistCommand => this._selectAlbumPlaylistCommand ??= new SelectAlbumPlaylistCommand(this);

        private Command _closeAlbumPlaylistTrackListCommand;
        public Command CloseAlbumPlaylistTrackListCommand => this._closeAlbumPlaylistTrackListCommand ??= new CloseAlbumPlaylistTrackListCommand(this);

        private IPlaylist _playingPlaylist;
        public IPlaylist PlayingPlaylist
        {
            get => this._playingPlaylist;
            set => this.SetProperty(ref this._playingPlaylist, value);
        }

        private bool _isLoop = true;
        public bool IsLoop
        {
            get => this._isLoop;
            set => this.SetProperty(ref this._isLoop, value);
        }

        private bool _isShuffle;
        public bool IsShuffle
        {
            get => this._isShuffle;
            set => this.SetProperty(ref this._isShuffle, value);
        }

        private readonly LinkedList<TrackInfo> _playHistory = new LinkedList<TrackInfo>();

        public const int MaxHistoryCount = 50;

        public bool IsPlayRequired { get; set; }

        public void Play()
        {
            this.IsPlayRequired = true;

            try
            {
                this.Player.Play();
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // pass
            }
        }

        public void Play(TrackInfo track, IPlaylist playlist)
        {
            this.IsPlayRequired = true;
            try
            {
                this.Player.Play(track, playlist);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // pass
            }
        }

        private bool _isOpenAlbumPlaylistTrackList = false;
        public bool IsOpenAlbumPlaylistTrackList
        {
            get => this._isOpenAlbumPlaylistTrackList;
            set => this.SetProperty(ref this._isOpenAlbumPlaylistTrackList, value);
        }

        private Command _openSettingWindowCommand;
        public Command OpenSettingWindowCommand => this._openSettingWindowCommand ??= (new OpenSettingWindowCommand(this));

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
        /// 破棄時
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Player.PlayStateChanged -= this.OnPlayStateChanged;
            this.MediaManager.TrackRegisterStateChanged += this.OnTrackRegisterStateChanged;
        }
    }
}
