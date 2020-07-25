using Dapper;
using Dapper.FastCrud;
using Gouter.Components.TypeHandlers;
using Gouter.Extensions;
using Gouter.Managers;
using Gouter.Players;
using Gouter.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
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
        /// メディア再生
        /// </summary>
        internal MediaPlayer MediaPlayer { get; private set; }

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

            this.InitializeDapper();

            var libraryPath = this.GetLocalFilePath(Config.LibraryFileName);
            this.MediaManager = MediaManager.CreateMediaManager(libraryPath);
            this.MediaPlayer = new MediaPlayer(this.MediaManager);
        }

        /// <summary>
        /// アプリケーション終了時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            // this.MediaManager.Close();

            if (this.IsRequireSaveSettings)
            {
                this.SaveSettings().Wait();
            }

            base.OnExit(e);
        }

        /// <summary>
        /// 設定ファイルを読み込む。
        /// </summary>
        /// <returns></returns>
        private async Task LoadSettings()
        {
            var settingFilePath = this.GetLocalFilePath(Config.SettingFileName);

            this.Setting = await MessagePackUtil.DeserializeFile<ApplicationSetting>(settingFilePath).ConfigureAwait(false);

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
            var settingFilePath = this.GetLocalFilePath(Config.SettingFileName);

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
        /// Dapperを初期化する。
        /// </summary>
        private void InitializeDapper()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.SqLite;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(new MemoryStreamTypeHandler());
            SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        }

        /// <summary>
        /// アプリケーションを強制終了する。
        /// </summary>
        public void ForceShutdown()
        {
            this.IsRequireSaveSettings = false;
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.Shutdown();
        }
    }
}
