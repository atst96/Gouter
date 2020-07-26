using CSCore.SoundOut;
using Gouter.Commands.MainWindow;
using Gouter.Extensions;
using Gouter.Managers;
using Gouter.Players;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Gouter.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, IMediaPlayerObserver
    {
        private readonly Random _rand = new Random();

        private static readonly App _app = App.Instance;

        public MediaManager MediaManager { get; } = _app.MediaManager;

        public MediaPlayer Player { get; } = _app.MediaPlayer;

        public PlaylistManager Playlists => this.MediaManager.Playlists;

        public SortedNotifiableCollectionWrapper<AlbumPlaylist> Albums { get; }

        private AlbumPlaylist _selectedAlbumPlaylist;
        public AlbumPlaylist SelectedAlbumPlaylist
        {
            get => this._selectedAlbumPlaylist;
            set => this.SetProperty(ref this._selectedAlbumPlaylist, value);
        }

        public MainWindowViewModel() : base()
        {
            this.Albums = new SortedNotifiableCollectionWrapper<AlbumPlaylist>(this.Playlists.Albums, AlbumComparer.Instance);

            this.Player.Subscribe(this);

            var timerInterval = TimeSpan.FromSeconds(0.1d);
            this._timer = new DispatcherTimer(timerInterval, DispatcherPriority.Render, this.OnTimerTick, Dispatcher.CurrentDispatcher);
            this._timer.Tick += this.OnTimerTick;

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        private Command _initializeCommand;
        public Command InitializeCommand => this._initializeCommand ?? (this._initializeCommand = new InitializeCommand(this));

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

        public StandardProgressReceiver LoadProgress { get; } = new StandardProgressReceiver();

        private string _status;
        public string Status
        {
            get => this._status;
            set => this.SetProperty(ref this._status, value);
        }

        private Command<TrackInfo> _trackListDoubleClickCommand;
        public Command<TrackInfo> TrackListDoubleClickCommand => this._trackListDoubleClickCommand ??= new TrackListDoubleClickCommand(this);

        private TrackInfo _selectedTrack;
        public TrackInfo SelectedTrack
        {
            get => this._selectedTrack;
            set => this.SetProperty(ref this._selectedTrack, value);
        }

        private Command _playCommand;
        public Command PlayCommand => this._playCommand ??= new PlayCommand(this);

        private Command _pauseCommand;
        public Command PauseCommand => this._pauseCommand ??= new PauseCommand(this);

        private Command _previousTrackCommand;
        public Command PreviousTrackCommand => this._previousTrackCommand ??= new PreviousTrackCommand(this);

        private Command _nextTrackCommand;
        public Command NextTrackCommand => this._nextTrackCommand ??= new NextTrackCommand(this);

        private Command _onCloseCommand;
        public Command OnCloseCommand => this._onCloseCommand ??= new OnCloseCommand(this);

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

        public async void Play()
        {
            this.IsPlayRequired = true;

            try
            {
                await this.Player.Play();
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // pass
            }
        }

        public async void Play(TrackInfo track, IPlaylist playlist)
        {
            this.IsPlayRequired = true;
            try
            {
                await this.Player.SwitchTrack(track, playlist).ConfigureAwait(false);
                await this.Player.Play().ConfigureAwait(false);
            }
            catch (Exception ez) when (ez is TaskCanceledException || ez is OperationCanceledException)
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

        void IMediaPlayerObserver.OnPlayStateChanged(PlayState state)
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
    }
}
