using System;
using System.Collections.Generic;
using Gouter.DataModels;
using Gouter.Playlists;

namespace Gouter.MediaPlayer;

/// <summary>
/// プレイリスト詳細情報
/// </summary>
internal class PlaylistInfo : NotificationObject, IPlaylistInfo
{
    /// <summary>
    /// 内部プレイリストID
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// プレイリスト名
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// プレイリスト作成日時
    /// </summary>
    public DateTimeOffset RegisteredAt { get; }

    /// <summary>
    /// プレイリスト更新日時
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// プレイリスト情報
    /// </summary>
    public CustomPlaylist Playlist { get; }

    /// <summary>
    /// プレイリスト詳細情報を生成する
    /// <param name="id">プレイリストID</param>
    /// <param name="name">プレイリスト名</param>
    /// </summary>
    public PlaylistInfo(int id, string name)
    {
        this.Id = id;
        this.Name = name;
        this.RegisteredAt = DateTimeOffset.Now;
        this.UpdatedAt = this.RegisteredAt;

        this.Playlist = new CustomPlaylist(this);
    }

    /// <summary>
    /// プレイリスト詳細情報を生成する
    /// </summary>
    /// <param name="playlist"></param>
    public PlaylistInfo(PlaylistDataModel playlist)
    {
        this.Id = playlist.Id;
        this.Name = playlist.Name;
        this.RegisteredAt = playlist.CreatedAt;
        this.UpdatedAt = playlist.UpdatedAt;

        this.Playlist = new CustomPlaylist(this);
    }
}
