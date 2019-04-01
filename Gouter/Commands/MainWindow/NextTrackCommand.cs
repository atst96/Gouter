using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class NextTrackCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;

        public NextTrackCommand(MainWindowViewModel viewModel) : base()
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            this._viewModel.SelectNextTrack();

            if (this._viewModel.IsPlayRequired)
            {
                this._viewModel.Player.Play();
            }
        }
    }
}
