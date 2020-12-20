using Gouter.ViewModels;

namespace Gouter.Commands.SettingWindow
{
    internal class RemoveMusicDirectoryCommand : Command<string>
    {
        private readonly SettingWindowViewModel _viewModel;

        public RemoveMusicDirectoryCommand(SettingWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(string parameter)
        {
            return this._viewModel.MusicDirectories.Contains(parameter);
        }

        public override void Execute(string parameter)
        {
            this._viewModel.MusicDirectories.Remove(parameter);
        }
    }
}
