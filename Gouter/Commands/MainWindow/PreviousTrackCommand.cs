using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class PreviousTrackCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;

        public PreviousTrackCommand(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            this._viewModel.SkipToPreviousTrack();

            if (this._viewModel.IsPlayRequired)
            {
                this._viewModel.Player.Play();
            }
        }
    }
}
