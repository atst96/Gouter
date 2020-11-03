using Gouter.Managers;
using Gouter.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
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
                await mediaManager.LoadLibrary().ConfigureAwait(false);

                var progress = this._viewModel.LoadProgress;

                this._viewModel.Status = "楽曲フォルダから新しい楽曲を検索しています...";

                var newTracks = trackManager.GetUnregisteredTracks(setting.MusicDirectories, setting.ExcludeDirectories);

                if (newTracks.Count <= 0)
                {
                    this._viewModel.Status = null;
                    return;
                }

                this._viewModel.Status = $"{newTracks.Count}件の楽曲が見つかりました。楽曲情報をライブラリに登録しています...";

                progress.Reset(newTracks.Count);

                mediaManager.RegisterTracks(newTracks, progress);

                this._viewModel.Status = $"{newTracks.Count}件の楽曲が追加されました";
            });

            // プレーヤ状態を復元する（暫定）
            var player = this._viewModel.Player;
            var settings = App.Instance.Setting;

            IPlaylist lastPlaylist = null;
            if (settings.LastPlaylistId != null)
            {
                int lastPlaylistId = (int)settings.LastPlaylistId;
                lastPlaylist = player.MediaManager.Playlists.Albums
                    .FirstOrDefault(a => a.Album.Id == lastPlaylistId);
            }

            if (settings.LastTrackId != null)
            {
                int lastTrackId = (int)settings.LastTrackId;

                var track = player.MediaManager.Tracks.Tracks
                    .FirstOrDefault(t => t.Id == lastTrackId);
                if (track != null)
                {
                    if (lastPlaylist != null)
                    {
                        player.SwitchTrack(track, lastPlaylist);
                        this._viewModel.SelectedAlbumPlaylist = player.Playlist as AlbumPlaylist;
                    }
                    else
                    {
                        player.SwitchTrack(track);
                    }
                }
            }
        }
    }
}
