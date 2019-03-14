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
        internal static SQLiteConnection SqlConnection { get; private set; }

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

            var sqlConfig = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                //DataSource = ":memory:"
                DataSource = GetLocalFilePath(Config.LibraryFileName),
            };

            SqlConnection = new SQLiteConnection(sqlConfig.ToString());
            SqlConnection.Open();

            sqlConfig = null;

            this.InitializeDatabase();
            this.LoadCachedTracks();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SqlConnection?.Dispose();

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
            if (!HasDataTable("albums"))
            {
                using (var cmd = SqlConnection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE albums (id INT PRIMARY KEY NOT NULL, key TEXT, name TEXT, artist TEXT, is_compilation BOOL, artwork BLOB)";
                    cmd.ExecuteNonQuery();
                }
            }

            if (!HasDataTable("tracks"))
            {
                using (var cmd = SqlConnection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE tracks (id INT PRIMARY KEY NOT NULL, album_id INT NOT NULL, path TEXT, duration INT, disk INT, track INT, year INT, album_artist TEXT, title TEXT, artist TEXT, genre TEXT)";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool HasDataTable(string tableName)
        {
            using (var command = SqlConnection.CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";

                command.Parameters.Add(new SQLiteParameter
                {
                    DbType = System.Data.DbType.String,
                    Value = tableName,
                });
                command.Prepare();

                Console.WriteLine(command.CommandText);

                using (var reader = command.ExecuteReader())
                {
                    return reader.Read() && reader.GetString(0) == tableName;
                }
            }
        }

        private void LoadCachedTracks()
        {
            using (var cmd = SqlConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM albums";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        var key = reader.GetString(1);
                        var name = reader.GetString(2);
                        var artist = reader.GetString(3);
                        bool isCompilation = reader.GetBoolean(4);
                        var artwork = reader[5] as byte[];

                        AlbumManager.Add(new AlbumInfo(id, key, name, artist, isCompilation, artwork));
                    }
                }
            }

            using (var cmd = SqlConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM tracks";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int albumId = reader.GetInt32(1);
                        var path = reader.GetString(2);
                        int duration = reader.GetInt32(3);
                        int disk = reader.GetInt32(4);
                        int track = reader.GetInt32(5);
                        int year = reader.GetInt32(6);
                        var albumArtist = reader.GetString(7);
                        var title = reader.GetString(8);
                        var artist = reader.GetString(9);
                        var genre = reader.GetString(10);

                        App.TrackManager.Tracks.Add(new TrackInfo(id, albumId, path, duration, disk, track, year, albumArtist, title, artist, genre));
                    }
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
