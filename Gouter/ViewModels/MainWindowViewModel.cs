using CSCore.SoundOut;
using Gouter.Commands.MainWindow;
using Gouter.Extensions;
using Gouter.Managers;
using Gouter.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Gouter.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, ISoundPlayerObserver
    {
        private readonly Random _rand = new Random();

        private static readonly App _app = App.Instance;

        public MediaManager MediaManager { get; } = _app.MediaManager;

        public MediaPlayer NewPlayer { get; } = _app.MediaPlayer;

        public PlaylistManager Playlists => this.MediaManager.Playlists;

        public SortedNotifiableCollectionWrapper<AlbumPlaylist> Albums { get; }

        public SoundPlayer Player => this.NewPlayer.InternalPlayer;

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

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        void ISoundPlayerObserver.OnPlayStateChanged(PlayState state)
        {
            if (state == PlayState.Stop)
            {
                if (this.IsPlayRequired)
                {
                    if (this.IsLoop)
                    {
                        this.SkipToNextTrack();
                        this.Player.Play();
                    }
                    else
                    {
                        this.IsPlayRequired = false;
                        if (this.PauseCommand.CanExecute(null))
                        {
                            this.PauseCommand.Execute(null);
                        }
                    }
                }
            }
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

        private double _volume = 1.0;
        public double Volume
        {
            get => this._volume;
            set => this.SetProperty(ref this._volume, value);
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

        public void Play()
        {
            this.IsPlayRequired = true;
            this.Player.Play();
        }

        public void Play(TrackInfo track)
        {
            this.IsPlayRequired = true;
            this.Player.SetTrack(track);
            this.Player.Play();
            this.AddHistory(track);
        }

        private LinkedListNode<TrackInfo> _currentNode;

        public void SkipToPreviousTrack()
        {
            if (this.PlayingPlaylist == null)
            {
                return;
            }

            var player = this.Player;
            var previousNode = this._currentNode?.Previous;

            if (player.GetPosition().TotalMilliseconds > 3000.0 || previousNode == null)
            {
                player.SetPosition(TimeSpan.Zero);
                return;
            }

            player.SetTrack(previousNode.Value);
            this._currentNode = previousNode;
        }

        public void SkipToNextTrack()
        {
            if (this.PlayingPlaylist == null)
            {
                return;
            }

            var player = this.Player;

            var nextNode = this._currentNode?.Next;
            if (nextNode != null)
            {
                player.SetTrack(nextNode.Value);
                this._currentNode = nextNode;

                return;
            }

            var playlist = this.PlayingPlaylist;
            var tracks = playlist.Tracks;

            TrackInfo nextTrack = default;

            if (this.IsShuffle)
            {
                int nextTrackIdx = this._rand.Next(0, tracks.Count - 1);

                nextTrack = tracks[nextTrackIdx];
            }
            else
            {
                var currentTrack = player.PlayTrack;
                var currentTrackIdx = tracks.IndexOf(currentTrack);

                if (currentTrackIdx >= 0)
                {
                    int nextTrackIdx = tracks.Count - 1 <= currentTrackIdx
                        ? 0
                        : currentTrackIdx + 1;

                    nextTrack = tracks[nextTrackIdx];
                }
            }

            if (nextTrack != default)
            {
                player.SetTrack(nextTrack);
                this.AddHistory(nextTrack);
            }
        }

        private bool _isOpenAlbumPlaylistTrackList = false;
        public bool IsOpenAlbumPlaylistTrackList
        {
            get => this._isOpenAlbumPlaylistTrackList;
            set => this.SetProperty(ref this._isOpenAlbumPlaylistTrackList, value);
        }

        public void AddHistory(TrackInfo nextTrack)
        {
            var history = this._playHistory;
            var currentNode = this._currentNode;

            if (currentNode == null)
            {
                this._currentNode = history.AddLast(nextTrack);
            }
            else if (!object.ReferenceEquals(this._currentNode.Value, nextTrack))
            {
                this._currentNode = history.AddAfter(currentNode, nextTrack);

                history.RemoveAfterAll(this._currentNode.Next);
            }

            //if (history.Count == 0 || !object.ReferenceEquals(history.Last.Value, currentTrack))
            //{
            //    history.AddLast(nextTrack);
            //}

            //if (history.Count > MaxHistoryCount)
            //{
            //    history.RemoveFirst();
            //}
        }

        public void SwitchPlaylist(IPlaylist album, TrackInfo track)
        {
            this.PlayingPlaylist = album;
            this.AddHistory(track);
        }

        private Command _openSettingWindowCommand;
        public Command OpenSettingWindowCommand => this._openSettingWindowCommand ??= (new OpenSettingWindowCommand(this));
    }
}
