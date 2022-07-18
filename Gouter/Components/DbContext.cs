using System;
using Gouter.DataModels;
using LiteDB;

namespace Gouter.Components;

/// <summary>
/// データベースのコンテキスト
/// </summary>
internal class DbContext : IDisposable
{
    /// <summary>
    /// データベース接続
    /// </summary>
    private readonly ILiteDatabase _dbConnection;

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
    /// プレイリスト情報
    /// </summary>
    public DbSet<PlaylistDataModel> Playlists { get; }

    /// <summary>
    /// プレイリスト・トラック紐付け情報
    /// </summary>
    public DbSet<PlaylistTrackDataModel> PlaylistTracks { get; }

    /// <summary>
    /// コンテキストを生成する。
    /// </summary>
    /// <param name="connection">DBのコネクション</param>
    public DbContext(ILiteDatabase connection)
    {
        this._dbConnection = connection;

        this.Tracks = new DbSet<TrackDataModel>(connection);
        this.Albums = new DbSet<AlbumDataModel>(connection);
        this.AlbumArtworks = new DbSet<AlbumArtworksDataModel>(connection);
        this.Playlists = new DbSet<PlaylistDataModel>(connection);
        this.PlaylistTracks = new DbSet<PlaylistTrackDataModel>(connection);
    }

    /// <summary>
    /// コンテキストを破棄する。
    /// </summary>
    void IDisposable.Dispose()
    {
        this._dbConnection?.Dispose();
    }
}
