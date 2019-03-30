using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class TrackListDoubleClickCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;

        public TrackListDoubleClickCommand(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return this._viewModel.SelectedTrack != null;
        }

        public override void Execute(object parameter)
        {
            var player = this._viewModel.Player;

            player.ChangePlaylist(new AlbumPlaylist(this._viewModel.SelectedAlbum));
            player.Play(this._viewModel.SelectedTrack);
        }
    }
}
