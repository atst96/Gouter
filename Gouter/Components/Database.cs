using System;
using Gouter.Components;
using LiteDB;

namespace Gouter
{
    internal class Database : IDisposable
    {
        private LiteDB.BsonMapper _mapper;
        private ILiteDatabase _connection;

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
            if (this._connection != null)
            {
                throw new InvalidOperationException();
            }

            var mapper = new BsonMapper();

            var connectionString = new ConnectionString
            {
                Collation = Collation.Binary,
                Filename = filePath,
                ReadOnly = false,
                Upgrade = true,
            };
            var conn = new LiteDatabase(connectionString, mapper);

            this._mapper = mapper;
            this._connection = conn;

            this.Context = new DbContext(this._connection);

            this.ConfigurationDatabase();

            this.IsConnected = true;
        }

        /// <summary>
        /// SQLiteの設定を行う。
        /// </summary>
        private void ConfigurationDatabase()
        {
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
        public DbTransaction BeginTransaction()
        {
            var transaction = new DbTransaction(this._connection);
            transaction.Begin();

            return transaction;
        }

        /// <summary>
        /// リソースを破棄する。
        /// </summary>
        public void Dispose()
        {
            this.Disconnect();
        }

        public void Flush()
        {
            this._connection.Checkpoint();
        }
    }
}
