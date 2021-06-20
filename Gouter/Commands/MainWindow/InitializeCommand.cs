using Gouter.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Gouter.Commands.MainWindow
{
    internal class InitializeCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;
        private bool _isCalled = false;

        public InitializeCommand(MainWindowViewModel viewModel) : base()
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter) => !this._isCalled;

        public override void Execute(object parameter)
        {
            this._isCalled = true;

            this._viewModel.MediaManager.Loaded += this.OnMediaManagerLoaded;

            App.Instance.OnMainViewReady();
        }

        private void OnMediaManagerLoaded(object sender, EventArgs e)
        {
            this._viewModel.MediaManager.Loaded -= this.OnMediaManagerLoaded;

            // プレーヤ状態を復元する（暫定）
            var player = this._viewModel.Player;
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
                        this._viewModel.SelectedAlbumPlaylist = player.Playlist as AlbumPlaylist;
                    }
                    else
                    {
                        player.SwitchTrack(track);
                    }
                }
            }
        }
    }
}
