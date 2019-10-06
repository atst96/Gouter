using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class SelectAlbumPlaylistCommand : Command<AlbumPlaylist>
    {
        private MainWindowViewModel _viewModel;

        public SelectAlbumPlaylistCommand(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(AlbumPlaylist parameter)
        {
            return parameter != null;
        }

        public override void Execute(AlbumPlaylist parameter)
        {
            this._viewModel.SelectedAlbumPlaylist = parameter;
            this._viewModel.IsOpenAlbumPlaylistTrackList = true;
        }
    }
}
