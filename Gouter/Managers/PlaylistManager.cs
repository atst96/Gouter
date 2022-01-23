using System;
using System.Linq;

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
    /// アルバムプレイリストの一覧
    /// </summary>
    public ObservableList<AlbumPlaylist> Albums { get; } = new ObservableList<AlbumPlaylist>();

    /// <summary>
    /// PlaylistManagerを生成する
    /// </summary>
    /// <param name="database">データベース</param>
    /// <param name="albumManager">アルバム情報</param>
    public PlaylistManager(Database database, AlbumManager albumManager)
    {
        this._database = database ?? throw new InvalidOperationException();
        this._albumManager = albumManager ?? throw new InvalidOperationException();

        albumManager.Registered += this.OnRegistered;
        albumManager.Removed += this.OnRemoved;
    }

    /// <summary>
    /// アルバム情報の登録通知
    /// </summary>
    /// <param name="albumInfo">アルバム情報</param>
    private void OnRegistered(object sender, AlbumInfo albumInfo)
    {
        this.Albums.Add(albumInfo.Playlist);
    }

    /// <summary>
    /// アルバム情報の削除通知
    /// </summary>
    /// <param name="albumInfo">アルバム情報</param>
    private void OnRemoved(object sender, AlbumInfo albumInfo)
    {
        this.Albums.Remove(albumInfo.Playlist);
    }

    /// <summary>
    /// プレイリストを読み込む。
    /// </summary>
    internal void Load()
    {
        this.Albums.AddRange(this._albumManager.Albums.Select(a => a.Playlist));
    }

    /// <summary>
    /// リソースを破棄する
    /// </summary>
    public void Dispose()
    {
        var albumManager = this._albumManager;
        albumManager.Registered -= this.OnRegistered;
        albumManager.Removed -= this.OnRemoved;
    }
}
