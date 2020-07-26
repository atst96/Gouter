using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dapper;
using Gouter.Components;
using Gouter.DataModels;
using Gouter.Utils;
using Microsoft.Data.Sqlite;

namespace Gouter
{
    internal class Database : IDisposable
    {
        private SqliteConnection _connection;

        public DbContext Context { get; private set; }

        public bool IsConnected { get; private set; } = false;

        public Database()
        {
        }

        /// <summary>
        /// データベースファイルを読み込む。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public void Connect(string filePath)
        {
            if (this._connection != null && this._connection.State == ConnectionState.Open)
            {
                throw new InvalidOperationException();
            }

            var sqlConfig = new SqliteConnectionStringBuilder
            {
                DataSource = filePath,
            };

            this._connection = new SqliteConnection(sqlConfig.ToString());
            this.Context = new DbContext(this._connection);
            this._connection.Open();

            this.InitializeSQLite();

            this.IsConnected = this._connection.State == ConnectionState.Open;
        }

        /// <summary>
        /// SQLiteの設定を行う。
        /// </summary>
        private void InitializeSQLite()
        {
            // foreign_keysを有効にする
            bool isForeginKeysEnabled = this._connection.ExecuteScalar("PRAGMA foreign_keys").ToString() == "1";

            if (!isForeginKeysEnabled)
            {
                this._connection.Execute("PRAGMA foreign_keys = ON");
            }
        }

        /// <summary>
        /// DB接続を切断する。
        /// </summary>
        public void Disconnect()
        {
            this.IsConnected = false;
            this._connection?.Dispose();
        }


        /// <summary>
        /// テーブルを準備する。
        /// </summary>
        public void PrepareTable()
        {
            // データベースの初期化処理
            var queryList = new Dictionary<string, string>
            {
                [TableNames.Albums] = Queries.CreateAlbumsTable,
                [TableNames.Tracks] = Queries.CreateTracksTable,
                [TableNames.AlbumArtworks] = Queries.CreateAlbumArtworksTable,
            };

            var tables = new HashSet<string>(this.EnumerateTableNames());

            var queries = queryList.Keys
                .Where(tbl => !tables.Contains(tbl))
                .Select(tbl => queryList[tbl]);

            if (queries.Any())
            {
                var sql = string.Join(";\n", queries);
                this._connection.Execute(sql);
            }
        }

        /// <summary>
        /// トランザクションを開始する。
        /// </summary>
        /// <returns></returns>
        public IDbTransaction BeginTransaction()
        {
            return this._connection.BeginTransaction();
        }

        /// <summary>
        /// コマンドを生成する。
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IDbCommand CreateCommand(string query)
        {
            var command = this._connection.CreateCommand();
            command.CommandText = query;

            return command;
        }

        /// <summary>
        /// クエリを実行する。
        /// </summary>
        /// <param name="query">クエリ</param>
        public void ExecuteNonQuery(string query)
        {
            using var command = this.CreateCommand(query);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// テーブル名を列挙する。
        /// </summary>
        /// <returns>テーブル名一覧</returns>
        public IEnumerable<string> EnumerateTableNames()
        {
            using var reader = this._connection.ExecuteReader("SELECT name from sqlite_master WHERE type = 'table'");

            while (reader.Read())
            {
                var tableName = reader.GetString(0);
                yield return tableName;
            }
        }

        /// <summary>
        /// リソースを破棄する。
        /// </summary>
        public void Dispose()
        {
            this.Disconnect();
        }

        /// <summary>
        /// テーブル名
        /// </summary>
        public static class TableNames
        {
            /// <summary>
            /// アルバムのテーブル名
            /// </summary>
            public static readonly string Albums = DbUtil.GetTableName<AlbumDataModel>();

            /// <summary>
            /// トラックのテーブル名
            /// </summary>
            public static readonly string Tracks = DbUtil.GetTableName<TrackDataModel>();

            /// <summary>
            /// アート枠のテーブル名
            /// </summary>
            public static readonly string AlbumArtworks = DbUtil.GetTableName<AlbumArtworksDataModel>();
        }

        /// <summary>
        /// SQLクエリ
        /// </summary>
        public static class Queries
        {
            public static readonly string CreateAlbumsTable = $@"
CREATE TABLE {TableNames.Albums} (
    id INT PRIMARY KEY NOT NULL,
    key TEXT,
    name TEXT,
    artist TEXT,
    is_compilation BOOL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
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
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    foreign key (album_id) references {TableNames.Albums}(id)
)";

            public static readonly string CreateAlbumArtworksTable = $@"
CREATE TABLE {TableNames.AlbumArtworks} (
    album_id PRIMARY KEY NOT NULL,
    artwork BLOB,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    foreign key (album_id) references {TableNames.Albums}(id)
)";
        }
    }
}
