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

        internal static AlbumManager AlbumManager { get; } = new AlbumManager();
        internal static MusicTrackManager TrackManager { get; } = new MusicTrackManager();

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

            Database.Connect();

            this.InitializeDatabase();
            this.LoadCachedTracks();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Database.Disconnect();

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

        private void InitializeDatabase()
        {
            var tables = Database.EnumerateTableNames();

            if (!tables.Contains(Database.TableNames.Albums))
            {
                Database.ExecuteNonQuery(Database.Queries.CreateAlbumsTable);
            }

            if (!tables.Contains(Database.TableNames.Tracks))
            {
                Database.ExecuteNonQuery(Database.Queries.CreateTracksTable);
            }
        }

        private void LoadCachedTracks()
        {
            using (var row = Database.Select(Database.TableNames.Albums))
            {
                while (row.Read())
                {
                    int id = row.Get<int>(0);
                    var key = row.Get<string>(1);
                    var name = row.Get<string>(2);
                    var artist = row.Get<string>(3);
                    bool isCompilation = row.Get<bool>(4);
                    byte[] artwork = row.GetOrDefault<byte[]>(5);

                    var albumInfo = new AlbumInfo(id, key, name, artist, isCompilation, artwork);
                    AlbumManager.Add(albumInfo);
                }
            }

            using (var row = Database.Select(Database.TableNames.Tracks))
            {
                while (row.Read())
                {
                    int id = row.Get<int>(0);
                    int albumId = row.Get<int>(1);
                    var path = row.Get<string>(2);
                    int duration = row.Get<int>(3);
                    int disk = row.Get<int>(4);
                    int track = row.Get<int>(5);
                    int year = row.Get<int>(6);
                    var albumArtist = row.Get<string>(7);
                    var title = row.Get<string>(8);
                    var artist = row.Get<string>(9);
                    var genre = row.Get<string>(10);

                    var trackInfo = new TrackInfo(id, albumId, path, duration, disk, track, year, albumArtist, title, artist, genre);
                    App.TrackManager.Tracks.Add(trackInfo);
                }
            }

            if (AlbumManager.Albums.Count > 0)
            {
                AlbumManager.SetAlbumIndex(AlbumManager.Albums.Max(a => a.Id));
            }

            if (TrackManager.Tracks.Count > 0)
            {
                MusicTrackManager.SetTrackIndex(TrackManager.Tracks.Max(t => t.Id));
            }
        }

        public void ForceShutdown()
        {
            this.IsRequireSaveSettings = false;
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            this.Shutdown();
        }
    }
}
