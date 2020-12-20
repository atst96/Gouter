using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class PauseCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;

        public PauseCommand(MainWindowViewModel viewModel) : base()
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            this._viewModel.IsPlayRequired = false;
            this._viewModel.Player.Pause();
        }
    }
}
