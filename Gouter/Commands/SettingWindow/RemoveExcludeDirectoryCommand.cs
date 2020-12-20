using Gouter.ViewModels;

namespace Gouter.Commands.SettingWindow
{
    internal class RemoveExcludeDirectoryCommand : Command<string>
    {
        private readonly SettingWindowViewModel _viewModel;

        public RemoveExcludeDirectoryCommand(SettingWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(string parameter)
        {
            return this._viewModel.ExcludeDirectories.Contains(parameter);
        }

        public override void Execute(string parameter)
        {
            this._viewModel.ExcludeDirectories.Remove(parameter);
        }
    }
}
