using System;
using System.Linq;
using System.Windows.Media;
using Gouter.MediaPlayer;
using Gouter.Playlists;

namespace Gouter.Managers;

/// <summary>
/// プレイリストの管理を行うクラス
/// </summary>
internal class PlaylistManager : IDisposable
{
    /// <summary>
    /// データベース
    /// </summary>
    private readonly Database _database;

    /// <summary>
    /// アルバムマネージャ
    /// </summary>
    private readonly AlbumManager _albumManager;

    /// <summary>
    /// プレイリストマネージャ
    /// </summary>
    private readonly CustomPlaylistManager _customPlaylistManager;

    /// <summary>
    /// アルバムプレイリストの一覧
    /// </summary>
    public ObservableList<AlbumPlaylist> Albums { get; } = new ObservableList<AlbumPlaylist>();

    /// <summary>
    /// プレイリスト
    /// </summary>
    public ObservableList<CustomPlaylist> CustomPlaylists { get; } = new ObservableList<CustomPlaylist>();

    /// <summary>
    /// PlaylistManagerを生成する
    /// </summary>
    /// <param name="database">データベース</param>
    /// <param name="albumManager">アルバム情報</param>
    public PlaylistManager(Database database, AlbumManager albumManager, CustomPlaylistManager customPlaylistManager)
    {
        this._database = database ?? throw new InvalidOperationException();
        this._albumManager = albumManager ?? throw new InvalidOperationException();
        this._customPlaylistManager = customPlaylistManager ?? throw new InvalidOperationException();

        albumManager.Registered += this.OnAlbumRegistered;
        albumManager.Removed += this.OnAlbumRemoved;

        customPlaylistManager.Registered += this.OnCustomPlaylistRegistered;
        customPlaylistManager.Removed += this.OnCustomPlaylistRemoved;
    }

    /// <summary>
    /// アルバム情報の登録通知
    /// </summary>
    /// <param name="albumInfo">アルバム情報</param>
    private void OnAlbumRegistered(object sender, AlbumInfo albumInfo)
    {
        this.Albums.Add(albumInfo.Playlist);
    }

    /// <summary>
    /// アルバム情報の削除通知
    /// </summary>
    /// <param name="albumInfo">アルバム情報</param>
    private void OnAlbumRemoved(object sender, AlbumInfo albumInfo)
    {
        this.Albums.Remove(albumInfo.Playlist);
    }

    /// <summary>
    /// アルバム情報の登録通知
    /// </summary>
    /// <param name="albumInfo">アルバム情報</param>
    private void OnCustomPlaylistRegistered(object sender, PlaylistInfo albumInfo)
    {
        this.CustomPlaylists.Add(albumInfo.Playlist);
    }

    /// <summary>
    /// アルバム情報の削除通知
    /// </summary>
    /// <param name="albumInfo">アルバム情報</param>
    private void OnCustomPlaylistRemoved(object sender, PlaylistInfo albumInfo)
    {
        this.CustomPlaylists.Remove(albumInfo.Playlist);
    }

    /// <summary>
    /// プレイリストを読み込む。
    /// </summary>
    internal void Load()
    {
        this.Albums.AddRange(this._albumManager.Albums.Select(a => a.Playlist));
        this.CustomPlaylists.AddRange(this._customPlaylistManager.Playlists.Select(p => p.Playlist));
    }

    /// <summary>
    /// リソースを破棄する
    /// </summary>
    public void Dispose()
    {
        var albumManager = this._albumManager;
        albumManager.Registered -= this.OnAlbumRegistered;
        albumManager.Removed -= this.OnAlbumRemoved;

        var customPlaylistManager = this._customPlaylistManager;
        customPlaylistManager.Registered -= this.OnCustomPlaylistRegistered;
        customPlaylistManager.Removed -= this.OnCustomPlaylistRemoved;
    }
}
