using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class CloseAlbumPlaylistTrackListCommand : Command
    {
        public MainWindowViewModel _viewModel;

        public CloseAlbumPlaylistTrackListCommand(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            this._viewModel.IsOpenAlbumPlaylistTrackList = false;
        }
    }
}
