using Gouter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Commands.SettingWindow
{
    internal class AddMusicDirectoryCommand : Command
    {
        private readonly SettingWindowViewModel _viewModel;

        public AddMusicDirectoryCommand(SettingWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            var directoryList = this._viewModel.MusicDirectories;

            var path = this._viewModel.DialogService.SelectDirectory("音楽フォルダを選択");

            if (!string.IsNullOrEmpty(path) && !directoryList.Contains(path, StringComparer.CurrentCultureIgnoreCase))
            {
                directoryList.Add(path);
            }
        }
    }
}
