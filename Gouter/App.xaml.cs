using Gouter.Data;
using Gouter.Extensions;
using Gouter.Managers;
using Gouter.Players;
using Gouter.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Gouter
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    internal partial class App : Application
    {
        internal const string Name = "Gouter";

        internal const string Version = "0.0.0.0";

        /// <summary>
        /// メディア管理
        /// </summary>
        internal MediaManager MediaManager { get; private set; }

        /// <summary>
        /// プレイリスト再生クラス
        /// </summary>
        internal PlaylistPlayer MediaPlayer { get; private set; }

        /// <summary>
        /// プレーヤ設定
        /// </summary>
        internal PlayerOptions PlayerOptions { get; private set; }

        /// <summary>
        /// サウンドデバイスのリスナー
        /// </summary>
        internal SoundDeviceListener SoundDeviceListener { get; } = new SoundDeviceListener();

        /// <summary>
        /// アセンブリ情報
        /// </summary>
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        /// <summary>
        /// Appインスタンス
        /// </summary>
        internal static App Instance { get; private set; }

        /// <summary>
        /// アプリケーション設定
        /// </summary>
        internal ApplicationSetting Setting { get; private set; }

        /// <summary>
        /// 終了時に設定ファイルを保存するかどうかのフラグ。
        /// </summary>
        public bool IsRequireSaveSettings { get; private set; } = true;

        /// <summary>
        /// アプリケーション起動時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            App.Instance = (App)Current;

            base.OnStartup(e);

            try
            {
                this.LoadSettings().Wait();
            }
            catch (Exception ex)
            {
                TaskDialog.Show(ex.GetMessage(), null, App.Name);
                this.ForceShutdown();
                return;
            }

            if (this.Setting.MusicDirectories.Count == 0)
            {
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                new Views.SettingWindow().ShowDialog();
            }

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            var libraryPath = this.GetLocalFilePath(Config.LibraryFileName);
            var artworkPath = this.GetLocalFilePath("Artworks");
            this.MediaManager = MediaManager.CreateMediaManager(libraryPath, artworkPath);

            // TODO: プレーヤ設定を設定データから生成できるようにする
            var options = new PlayerOptions
            {
                LoopMode = LoopMode.Playlist,
                ShuffleMode = ShuffleMode.None,
                IsShuffleAvoidCurrentTrack = true,
                IsEnableFadeInOut = true,
                FadeInOutDuration = TimeSpan.FromMilliseconds(200d),
            };

            this.PlayerOptions = options;

            this.MediaPlayer = new PlaylistPlayer(this.MediaManager, options);
        }

        /// <summary>
        /// アプリケーション終了時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            if (this.IsRequireSaveSettings)
            {
                this.SaveSettings().Wait();
            }

            using (var player = this.MediaPlayer)
            {
                player.Stop();
            }

            this.MediaManager.Dispose();

            base.OnExit(e);
        }

        /// <summary>
        /// 設定ファイルを読み込む。
        /// </summary>
        /// <returns></returns>
        private async Task LoadSettings()
        {
            var settingFilePath = this.GetLocalFilePath(Config.SettingFileName);

            this.Setting = await MessagePackUtil.DeserializeFile<ApplicationSetting>(settingFilePath)
                .ConfigureAwait(false);

            if (this.Setting == null)
            {
                this.Setting = new ApplicationSetting();
            }
        }

        /// <summary>
        /// 設定ファイルを保存する。
        /// </summary>
        /// <returns></returns>
        private Task SaveSettings()
        {
            var setting = this.Setting;

            var player = this.MediaPlayer;
            var manager = player.MediaManager;

            var settingFilePath = this.GetLocalFilePath(Config.SettingFileName);

            var lastTrack = player.Track;
            if (lastTrack != null)
            {
                setting.LastTrackId = lastTrack.Id;
            }

            var lastPlaylist = player.Playlist as AlbumPlaylist;
            if (lastPlaylist != null)
            {
                setting.LastPlaylistId = lastPlaylist.Album.Id;
            }

            return MessagePackUtil.SerializeFile(this.Setting, settingFilePath);
        }

        /// <summary>
        /// 現在のディレクトリを取得する。
        /// </summary>
        /// <returns>現在のディレクトリのパス</returns>
        public string GetAssemlyDirectory()
            => Path.GetDirectoryName(this._assembly.Location);

        /// <summary>
        /// カレントディレクトリのファイルパスを取得する。
        /// </summary>
        /// <param name="relativePath">相対ファイル名</param>
        /// <returns>ファイルの絶対パス</returns>
        public string GetLocalFilePath(string relativePath)
            => Path.Combine(this.GetAssemlyDirectory(), relativePath);

        /// <summary>
        /// アプリケーションを強制終了する。
        /// </summary>
        public void ForceShutdown()
        {
            this.IsRequireSaveSettings = false;
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.Shutdown();
        }

        /// <summary>
        /// メインビューの準備完了時
        /// </summary>
        internal void OnMainViewReady()
        {
            var settings = this.Setting;
            var mediaManager = this.MediaManager;
            var trackManager = mediaManager.Tracks;

            // TODO: ファイル検索時の除外パスを指定できるようにする
            var excludeFiles = Array.Empty<string>();

            mediaManager.LoadLibrary()
                .ContinueWith(t =>
                {
                    mediaManager.SearchAndRegisterNewTracks(
                        settings.MusicDirectories,
                        settings.ExcludeDirectories,
                        excludeFiles);
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
