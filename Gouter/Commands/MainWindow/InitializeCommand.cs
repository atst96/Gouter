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
                App.AlbumManager.LoadDatabase();
                App.TrackManager.LoadDatabase();

                var progress = this._viewModel.LoadProgress;

                this._viewModel.Status = "楽曲ディレクトリを検索しています...";

                var newFiles = MusicTrackManager.FindNewFiles(setting.MusicDirectories, setting.ExcludeDirectories);

                if (newFiles.Count > 0)
                {
                    this._viewModel.Status = newFiles.Count + "件の新規ファイルを検出しました。楽曲情報を読み込んでいます...";
                    progress.Reset(newFiles.Count);
                    var newTracks = MusicTrackManager.GetTracks(newFiles, progress);

                    this._viewModel.Status = newTracks.Count + "件の楽曲情報をライブラリに登録しています...";

                    progress.Reset(newTracks.Count);

                    App.TrackManager.RegisterAll(newTracks, progress);

                }
            });

            this._viewModel.Status = null;
        }
    }
}
