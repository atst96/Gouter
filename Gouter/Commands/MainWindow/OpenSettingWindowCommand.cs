using System;
using System.Collections.Generic;
using System.Text;
using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class OpenSettingWindowCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;

        public OpenSettingWindowCommand(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            this._viewModel.WindowService.OpenSettingWindow();
        }
    }
}
