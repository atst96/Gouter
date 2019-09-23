using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Gouter.Components.TypeMappers;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Gouter
{
    internal static class Database
    {
        private static SQLiteConnection _connection;
        private static QueryFactory _queryFactory;

        public static bool IsConnected { get; private set; } = false;

        public static void Connect()
        {
            if (_connection != null)
            {
                throw new InvalidOperationException();
            }

            ConfigurationDapper();

            var sqlConfig = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = App.Instance.GetLocalFilePath(Config.LibraryFileName),
            };

            _connection = new SQLiteConnection(sqlConfig.ToString());
            _connection.Open();

            InitializeSQLite();

            _queryFactory = new QueryFactory(_connection, new SqliteCompiler());

            IsConnected = true;
        }

        private static void ConfigurationDapper()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(new MemoryStreamTypeHandler());
        }

        private static void InitializeSQLite()
        {
            // foreign_keysを有効にする
            using var getForeignKeysCommand = CreateCommand("PRAGMA foreign_keys");
            bool isForeginKeysEnabled = getForeignKeysCommand.ExecuteScalar().ToString() == "1";

            if (isForeginKeysEnabled)
            {
                ExecuteNonQuery("PRAGMA foreign_keys = ON;");
            }
        }

        public static void Disconnect()
        {
            IsConnected = false;
            _connection?.Dispose();
        }

        public static DbTransaction BeginTransaction()
        {
            return _connection.BeginTransaction();
        }

        public static DbCommand CreateCommand(string query)
        {
            var command = _connection.CreateCommand();
            command.CommandText = query;

            return command;
        }

        public static void ExecuteNonQuery(string query)
        {
            using var command = CreateCommand(query);
            command.ExecuteNonQuery();
        }

        public static QueryFactory GetQueryFactory()
        {
            return _queryFactory;
        }

        public static IEnumerable<string> EnumerateTableNames()
        {
            return GetQueryFactory()
                .Query("sqlite_master")
                .Select("name")
                .Where("type", "table")
                .Get<string>();
        }

        public static class TableNames
        {
            public const string Albums = "albums";
            public const string Tracks = "tracks";
        }

        public static class Queries
        {
            public static readonly string CreateAlbumsTable = $@"
CREATE TABLE {TableNames.Albums} (
    id INT PRIMARY KEY NOT NULL,
    key TEXT,
    name TEXT,
    artist TEXT,
    is_compilation BOOL,
    artwork BLOB
)";
            public static readonly string CreateTracksTable = $@"
CREATE TABLE {TableNames.Tracks} (
    id INT PRIMARY KEY NOT NULL,
    album_id INT NOT NULL,
    path TEXT,
    duration INT,
    disk INT,
    track INT,
    year INT,
    album_artist TEXT,
    title TEXT,
    artist TEXT,
    genre TEXT,
    foreign key (album_id) references {TableNames.Albums}(id)
)";
        }
    }
}
