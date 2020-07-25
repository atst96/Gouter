using System;
using System.Data;
using Gouter.DataModels;

namespace Gouter.Components
{
    /// <summary>
    /// データベースのコンテキスト
    /// </summary>
    internal class DbContext : IDisposable
    {
        private readonly IDbConnection _dbConnection;

        /// <summary>
        /// トラック情報
        /// </summary>
        public DbSet<TrackDataModel> Tracks { get; }

        /// <summary>
        /// アルバム情報
        /// </summary>
        public DbSet<AlbumDataModel> Albums { get; set; }

        /// <summary>
        /// アートワーク情報
        /// </summary>
        public DbSet<AlbumArtworksDataModel> AlbumArtworks { get; }

        /// <summary>
        /// コンテキストを生成する。
        /// </summary>
        /// <param name="dbConnection">DBのコネクション</param>
        public DbContext(IDbConnection dbConnection)
        {
            this._dbConnection = dbConnection;

            this.Tracks = new DbSet<TrackDataModel>(dbConnection);
            this.Albums = new DbSet<AlbumDataModel>(dbConnection);
            this.AlbumArtworks = new DbSet<AlbumArtworksDataModel>(dbConnection);
        }

        /// <summary>
        /// コンテキストを破棄する。
        /// </summary>
        void IDisposable.Dispose()
        {
            this._dbConnection?.Dispose();
        }
    }
}
