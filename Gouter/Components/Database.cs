using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Gouter.Components.TypeHandlers;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Gouter
{
    internal class Database : IDisposable
    {
        private SQLiteConnection _connection;
        private QueryFactory _queryFactory;

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

            var sqlConfig = new SQLiteConnectionStringBuilder
            {
                Version = 3,
                DataSource = filePath,
            };

            this._connection = new SQLiteConnection(sqlConfig.ToString());
            this._connection.Open();

            this.InitializeSQLite();

            this._queryFactory = new QueryFactory(this._connection, new SqliteCompiler());

            this.IsConnected = this._connection.State == ConnectionState.Open;
        }

        /// <summary>
        /// SQLiteの設定を行う。
        /// </summary>
        private void InitializeSQLite()
        {
            // foreign_keysを有効にする
            using var getForeignKeysCommand = this.CreateCommand("PRAGMA foreign_keys");
            bool isForeginKeysEnabled = getForeignKeysCommand.ExecuteScalar().ToString() == "1";

            if (isForeginKeysEnabled)
            {
                this.ExecuteNonQuery("PRAGMA foreign_keys = ON");
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
        /// クエリビルダを取得する。
        /// </summary>
        /// <returns>クエリビルダ</returns>
        public QueryFactory GetQueryBuilder()
        {
            return this._queryFactory;
        }

        /// <summary>
        /// テーブル名を列挙する。
        /// </summary>
        /// <returns>テーブル名一覧</returns>
        public IEnumerable<string> EnumerateTableNames()
        {
            return this.GetQueryBuilder()
                .Query("sqlite_master")
                .Select("name")
                .Where("type", "table")
                .Get<string>();
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
            public const string Albums = "albums";
            public const string Tracks = "tracks";
            public const string AlbumArtworks = "album_artworks";
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
