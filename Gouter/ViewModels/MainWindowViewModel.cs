﻿using Gouter.Commands.MainWindow;
using Gouter.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gouter.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private readonly Random _rand = new Random();

        public SortedNotifiableCollectionWrapper<AlbumInfo> Albums { get; }

        public SoundPlayer Player { get; }

        public MainWindowViewModel() : base()
        {
            this.Albums = new SortedNotifiableCollectionWrapper<AlbumInfo>(App.AlbumManager.Albums, AlbumComparer.Instance);
            this.Player = new SoundPlayer();

            this.Player.TrackPlayingEnded += this.OnPlayTrackEnded;

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        private void OnPlayTrackEnded(object sender, EventArgs e)
        {
            if (this.IsPlayRequired)
            {
                if (this.IsLoop)
                {
                    this.SelectNextTrack();
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

        private Command _initializeCommand;
        public Command InitializeCommand => this._initializeCommand ?? (this._initializeCommand = new InitializeCommand(this));

        private IPlaylist _currentPlaylist;
        public IPlaylist CurrentPlaylist
        {
            get => this._currentPlaylist;
            set => this.SetProperty(ref this._currentPlaylist, value);
        }

        private AlbumInfo _selectedAlbum;
        public AlbumInfo SelectedAlbum
        {
            get => this._selectedAlbum;
            set
            {
                if (this.SetProperty(ref this._selectedAlbum, value))
                {
                    this.CurrentPlaylist = new AlbumPlaylist(value);
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

        private Command _trackListDoubleClickCommand;
        public Command TrackListDoubleClickCommand => this._trackListDoubleClickCommand ?? (this._trackListDoubleClickCommand = new TrackListDoubleClickCommand(this));

        private TrackInfo _selectedTrack;
        public TrackInfo SelectedTrack
        {
            get => this._selectedTrack;
            set => this.SetProperty(ref this._selectedTrack, value);
        }

        private Command _playCommand;
        public Command PlayCommand => this._playCommand ?? (this._playCommand = new PlayCommand(this));

        private Command _pauseCommand;
        public Command PauseCommand => this._pauseCommand ?? (this._pauseCommand = new PauseCommand(this));

        private Command _previousTrackCommand;
        public Command PreviousTrackCommand => this._previousTrackCommand ?? (this._previousTrackCommand = new PreviousTrackCommand(this));

        private Command _nextTrackCommand;
        public Command NextTrackCommand => this._nextTrackCommand ?? (this._nextTrackCommand = new NextTrackCommand(this));

        private Command _onCloseCommand;
        public Command OnCloseCommand => this._onCloseCommand ?? (this._onCloseCommand = new OnCloseCommand(this));

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

        private readonly LinkedList<TrackInfo> _trackHistory = new LinkedList<TrackInfo>();

        public const int MaxHistoryCount = 50;

        public void AddHistory(TrackInfo nextTrack)
        {
            var history = this._trackHistory;

            var currentTrack = this.Player.CurrentTrack;

            if (history.Count == 0 || !object.ReferenceEquals(history.Last.Value, currentTrack))
            {
                history.AddLast(nextTrack);
            }

            if (history.Count > MaxHistoryCount)
            {
                history.RemoveFirst();
            }
        }

        public bool IsPlayRequired { get; set; }

        public void Play()
        {
            this.IsPlayRequired = true;
            this.Player.Play();
        }

        public void Play(TrackInfo track)
        {
            this.IsPlayRequired = true;
            this.Player.Play(track);
            this.AddHistory(track);
        }

        public void SelectPreviousTrack()
        {
            if (this.PlayingPlaylist == null)
            {
                return;
            }

            var playlist = this.PlayingPlaylist;
            var tracks = playlist.Tracks;

            var history = this._trackHistory;

            if (history.Count > 1)
            {
                history.RemoveLast();
                var previousTrack = history.Last.Value;
                this.Player.SetTrack(previousTrack);
            }
        }

        public void SelectNextTrack()
        {
            if (this.PlayingPlaylist == null)
            {
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
                var currentTrack = this.Player.CurrentTrack;
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
                this.Player.SetTrack(nextTrack);
                this.AddHistory(nextTrack);
            }
        }
    }
}
