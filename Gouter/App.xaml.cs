using Gouter.Extensions;
using Gouter.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        internal static App Instance { get; private set; }

        internal ApplicationSetting Setting { get; private set; }

        public bool IsRequireSaveSettings { get; private set; } = true;

        protected override async void OnStartup(StartupEventArgs e)
        {
            App.Instance = (App)Application.Current;

            base.OnStartup(e);

            try
            {
                await this.LoadSettings();
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (this.IsRequireSaveSettings)
            {
                this.SaveSettings();
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

        private async void SaveSettings()
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

        public void ForceShutdown()
        {
            this.IsRequireSaveSettings = false;
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.Shutdown();
        }
    }
}
