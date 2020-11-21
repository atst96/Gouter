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
        private readonly AlbumTrackViewModel _viewModel;

        public TrackListDoubleClickCommand(AlbumTrackViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(TrackInfo parameter)
        {
            return parameter != null;
        }

        public override void Execute(TrackInfo parameter)
        {
            var player = App.Instance.MediaPlayer;

            player.Play(parameter, this._viewModel.Playlist);
        }
    }
}
