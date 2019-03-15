using Gouter.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Commands.MainWindow
{
    internal class InitializeCommand : Command
    {
        private readonly MainWindowViewModel _viewModel;
        private bool _isCalled = false;

        public InitializeCommand(MainWindowViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter) => !this._isCalled;

        public override async void Execute(object parameter)
        {
            this._isCalled = true;

            var setting = App.Instance.Setting;

            await Task.Run(() =>
            {
                var progress = this._viewModel.LoadProgress;

                var newFiles = MusicTrackManager.FindNewFiles(setting.MusicDirectories);

                if (newFiles.Count > 0)
                {
                    progress.Reset(newFiles.Count);
                    var newTracks = MusicTrackManager.GetTracks(newFiles, progress);

                    progress.Reset(newTracks.Count);
                    int count = 0;

                    using (var transaction = Database.BeginTransaction())
                    {
                        foreach (var track in newTracks)
                        {
                            var trackInfo = App.TrackManager.Register(track);

                            progress.Report(++count);
                        }

                        transaction.Commit();
                    }
                }
            });
        }
    }
}
