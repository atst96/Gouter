using Gouter.ViewModels;

namespace Gouter.Commands.MainWindow
{
    internal class PlayCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;

        public PlayCommand(MainWindowViewModel viewModel) : base()
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            if (this._viewModel.Player.IsPlaying)
            {
                this._viewModel.IsPlayRequired = false;
                this._viewModel.Player.Pause();
            }
            else
            {
                this._viewModel.Play();
            }
        }
    }
}
