using Gouter.Managers;
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

        public InitializeCommand(MainWindowViewModel viewModel) : base()
        {
            this._viewModel = viewModel;
        }

        public override bool CanExecute(object parameter) => !this._isCalled;

        public override async void Execute(object parameter)
        {
            this._isCalled = true;

            var setting = App.Instance.Setting;

            var mediaManager = App.Instance.MediaManager;
            var trackManager = mediaManager.Tracks;

            await Task.Run(async () =>
            {
                await mediaManager.LoadLibrary();

                var progress = this._viewModel.LoadProgress;

                this._viewModel.Status = "楽曲ディレクトリを検索しています..."; 

                var registeredFiles = new HashSet<string>(trackManager.Tracks.Select(t => t.Path));
                var newFiles = TrackManager.FindNewFiles(registeredFiles, setting.MusicDirectories, setting.ExcludeDirectories);

                if (newFiles.Count > 0)
                {
                    this._viewModel.Status = newFiles.Count + "件の新規ファイルを検出しました。楽曲情報を読み込んでいます...";
                    progress.Reset(newFiles.Count);

                    var newTracks = TrackManager.GetTracks(newFiles, progress);

                    this._viewModel.Status = newTracks.Count + "件の楽曲情報をライブラリに登録しています...";

                    progress.Reset(newTracks.Count);

                    mediaManager.RegisterTracks(newTracks, progress);

                }
            });

            this._viewModel.Status = null;
        }
    }
}
