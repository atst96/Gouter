using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class TrackListDoubleClickCommand : Command<TrackInfo>
    {
        private readonly MainWindowViewModel _viewModel;

        public TrackListDoubleClickCommand(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(TrackInfo parameter)
        {
            return parameter != null;
        }

        public override void Execute(TrackInfo parameter)
        {
            this._viewModel.Play(parameter, this._viewModel.SelectedAlbumPlaylist);
        }
    }
}
