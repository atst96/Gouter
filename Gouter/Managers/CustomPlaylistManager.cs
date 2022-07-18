using System;
using System.Collections.Generic;
using System.Linq;
using Gouter.Components;
using Gouter.DataModels;
using Gouter.MediaPlayer;
using LiteDB;

namespace Gouter.Managers;

/// <summary>
/// プレイリスト管理クラス
/// </summary>
internal class CustomPlaylistManager
{
    /// <summary>DB</summary>
    private Database _database;
    private TrackManager _trackManager;

    /// <summary>
    /// プレイリスト一覧
    /// </summary>
    public ConcurrentObservableList<PlaylistInfo> Playlists { get; } = new ConcurrentObservableList<PlaylistInfo>();

    /// <summary>
    /// アルバム登録時のイベント
    /// </summary>
    public event EventHandler<PlaylistInfo> Registered;

    /// <summary>
    /// アルバム削除時のイベント
    /// </summary>
    public event EventHandler<PlaylistInfo> Removed;

    public CustomPlaylistManager(Database database, TrackManager tracks)
    {
        this._database = database;
        this._trackManager = tracks;
    }

    public void Load()
    {
        var dbContext = this._database.Context;

        var tracksByPlaylist = dbContext.PlaylistTracks
            .GroupBy(t => t.PlaylistId)
            .ToDictionary(t => t.Key);

        var playlists = dbContext.Playlists.Select(i =>
        {
            var playlist = new PlaylistInfo(i);
            if (tracksByPlaylist.TryGetValue(i.Id, out var playlistTracks))
            {
                var trackIds = playlistTracks.Select(t => t.TrackId).ToHashSet();
                var tracks = this._trackManager
                    .Where(t => trackIds.Contains(t.Id))
                    .Select(t => new PlaylistTrackInfo(playlist, t));

                playlist.Playlist.Tracks.AddRange(tracks);
            }

            return playlist;
        });

        this.Playlists.AddRange(playlists);
    }

    /// <summary>
    /// プレイリストを作成する
    /// </summary>
    /// <param name="tracks"></param>
    /// <param name="name"></param>
    public PlaylistInfo Create(string name, ICollection<TrackInfo> tracks = null)
    {
        // プレイリストを登録して新規IDを発行する
        var newId = this._database.Context.Playlists.Insert(new PlaylistDataModel
        {
            Name = name,
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = DateTimeOffset.Now,
        }).AsInt32;

        // プレイリストにトラック情報を紐付けする
        var playlist = new PlaylistInfo(newId, name);
        if (tracks is not null && tracks.Count > 0)
        {
            this.Add(playlist, tracks);
        }

        this.Playlists.Add(playlist);
        this.Registered?.Invoke(this, playlist);

        return playlist;
    }

    /// <summary>
    /// プレイリストにトラック情報を追加する
    /// </summary>
    /// <param name="plyalist"></param>
    /// <param name="tracks"></param>
    public void Add(PlaylistInfo plyalist, ICollection<TrackInfo> tracks)
    {
        this._database.Context.PlaylistTracks.InsertBulk(tracks.Select(t => new PlaylistTrackDataModel
        {
            PlaylistId = plyalist.Id,
            TrackId = t.Id,
            CreatedAt = DateTimeOffset.Now,
        }));

        plyalist.Playlist.Tracks.AddRange(
            tracks.Select(t => new PlaylistTrackInfo(plyalist, t)));
    }

    public void Delete(PlaylistInfo playlist, ICollection<TrackInfo> tracks)
    {
        var trackIds = tracks.Select(i => i.Id).ToHashSet();

        this._database.Context.PlaylistTracks.DeleteMany(
            i => i.PlaylistId == playlist.Id && trackIds.Contains(i.TrackId));

        // TODO: 
        // ((ObservableList<TrackInfo>)playlist.Tracks).RemoveRange();
    }

    /// <summary>
    /// プレイリスト情報を削除する
    /// </summary>
    /// <param name="playlist"></param>
    public void Delete(PlaylistInfo playlist)
    {
        this._database.Context.Playlists.Delete(new BsonValue(playlist.Id));

        this.Playlists.Remove(playlist);

        this._database.Context.PlaylistTracks.DeleteMany(i => i.PlaylistId == playlist.Id);

        playlist.Playlist.Tracks.Clear();
    }
}
