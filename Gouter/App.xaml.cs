using Dapper;
using Gouter.Components.TypeHandlers;
using Gouter.Extensions;
using Gouter.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
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

        internal static MediaPlayer MediaPlayer { get; } = new MediaPlayer();

        [Obsolete]
        internal static LibraryManager LibraryManager => MediaPlayer.Library;
        [Obsolete]
        internal static PlaylistManager PlaylistManager => LibraryManager.Playlists;
        [Obsolete]
        internal static AlbumManager AlbumManager => LibraryManager.Albums;
        [Obsolete]
        internal static TrackManager TrackManager => LibraryManager.Tracks;

        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        internal static App Instance { get; private set; }

        internal ApplicationSetting Setting { get; private set; }

        public bool IsRequireSaveSettings { get; private set; } = true;

        protected override void OnStartup(StartupEventArgs e)
        {
            App.Instance = (App)Application.Current;

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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MediaPlayer.Close();

            if (this.IsRequireSaveSettings)
            {
                this.SaveSettings().Wait();
            }

            base.OnExit(e);
        }

        private async Task LoadSettings()
        {
            var settingFilePath = this.GetLocalFilePath(Config.SettingFileName);

            this.Setting = await MessagePackUtility.DeserializeFile<ApplicationSetting>(settingFilePath).ConfigureAwait(false);

            if (this.Setting == null)
            {
                this.Setting = new ApplicationSetting();
            }
        }

        private async Task SaveSettings()
        {
            var settingFilePath = this.GetLocalFilePath(Config.SettingFileName);

            await MessagePackUtility.SerializeFile(this.Setting, settingFilePath).ConfigureAwait(false);
        }

        public string GetAssemlyDirectory()
        {
            return Path.GetDirectoryName(this._assembly.Location);
        }

        public string GetLocalFilePath(string filename)
        {
            return Path.Combine(this.GetAssemlyDirectory(), filename);
        }

        private void InitializeDapper()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(new MemoryStreamTypeHandler());
            SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        }

        public void ForceShutdown()
        {
            this.IsRequireSaveSettings = false;
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.Shutdown();
        }
    }
}
