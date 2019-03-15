using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal static class Database
    {
        private static SQLiteConnection _connection;

        public static bool IsConnected { get; private set; } = false;

        public static void Connect()
        {
            if (_connection != null)
            {
                throw new InvalidOperationException();
            }

            var sqlConfig = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = App.Instance.GetLocalFilePath(Config.LibraryFileName),
            };

            _connection = new SQLiteConnection(sqlConfig.ToString());
            _connection.Open();

            IsConnected = true;

            sqlConfig = null;
        }

        public static void Disconnect()
        {
            IsConnected = false;
            _connection?.Dispose();
        }

        public static SQLiteTransaction BeginTransaction()
        {
            return _connection.BeginTransaction();
        }

        public static SQLiteCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }

        public static SQLiteCommand CreateCommand(string query)
        {
            var command = _connection.CreateCommand();

            command.CommandText = query;

            return command;
        }

        public static SQLiteDataReader ExecuteReader(string query)
        {
            using (var command = CreateCommand(query))
            {
                return command.ExecuteReader();
            }
        }

        public static void ExecuteNonQuery(string query)
        {
            using (var command = CreateCommand(query))
            {
                command.ExecuteNonQuery();
            }
        }

        public static SQLiteDataReader Select(string tableName)
        {
            using (var command = CreateCommand())
            {
                command.CommandText = "SELECT * FROM " + tableName;

                return command.ExecuteReader();
            }
        }

        public static void Insert(string tableName, IDictionary<string, object> values)
        {
            var columns = values.Keys;

            var columnsText = string.Join(",", columns);
            var valuesText = string.Join(",", columns.Select(key => "@" + key));

            using (var command = CreateCommand())
            {
                command.CommandText = $"INSERT INTO {tableName} ({columnsText}) VALUES ({valuesText})";

                foreach(var kvp in values)
                {
                    command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                command.Prepare();

                command.ExecuteNonQuery();
            }
        }

        public static IList<string> EnumerateTableNames()
        {
            var tableNames = new List<string>();

            using (var reader = ExecuteReader(Queries.SelectTables))
            {
                while (reader.Read())
                {
                    tableNames.Add(reader.GetString(0));
                }
            }

            return tableNames;
        }

        public static class TableNames
        {
            public const string Albums = "albums";
            public const string Tracks = "tracks";
        }

        public static class Queries
        {
            public const string CreateAlbumsTable = "CREATE TABLE albums (id INT PRIMARY KEY NOT NULL, key TEXT, name TEXT, artist TEXT, is_compilation BOOL, artwork BLOB)";
            public const string CreateTracksTable = "CREATE TABLE tracks (id INT PRIMARY KEY NOT NULL, album_id INT NOT NULL, path TEXT, duration INT, disk INT, track INT, year INT, album_artist TEXT, title TEXT, artist TEXT, genre TEXT)";

            public const string SelectTables = "SELECT name FROM sqlite_master WHERE type='table'";
        }
    }
}
