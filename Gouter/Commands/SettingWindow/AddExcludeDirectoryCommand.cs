using Gouter.ViewModels;
using System;
using System.Linq;

namespace Gouter.Commands.SettingWindow
{
    internal class AddExcludeDirectoryCommand : Command
    {
        private readonly SettingWindowViewModel _viewModel;

        public AddExcludeDirectoryCommand(SettingWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            var directoryList = this._viewModel.ExcludeDirectories;

            var path = this._viewModel.DialogService.SelectDirectory("検索から除外するディレクトリを洗濯");

            if (!string.IsNullOrEmpty(path) && !directoryList.Contains(path, StringComparer.CurrentCultureIgnoreCase))
            {
                directoryList.Add(path);
            }
        }
    }
}
