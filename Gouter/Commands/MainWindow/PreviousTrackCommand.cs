using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

        public override async void Execute(object parameter)
        {
            try
            {
                await this._viewModel.Player.PlayPrevious();
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                // pass
            }
        }
    }
}
