using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gouter.DataModels;

namespace Gouter.Managers;

/// <summary>
/// トラック情報の管理を行う
/// </summary>
internal sealed class TrackManager : ICollection<TrackInfo>
{
    /// <summary>
    /// データベース
    /// </summary>
    private readonly Database _database;

    /// <summary>
    /// トラックの最終ID
    /// </summary>
    private volatile int _latestTrackId = -1;

    /// <summary>
    /// トラックIDリスト
    /// </summary>
    private readonly HashSet<int> _registeredTrackIds = new();

    /// <summary>
    /// トラック情報リスト(内部用)
    /// </summary>
    private readonly List<TrackInfo> _registeredTracks = new();

    /// <summary>
    /// トラック情報リスト
    /// </summary>
    public IReadOnlyList<TrackInfo> Tracks => this._registeredTracks;

    /// <summary>
    /// TrackManagerを生成する。
    /// </summary>
    /// <param name="database">データベース</param>
    public TrackManager(Database database)
    {
        this._database = database ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// トラックIDを生成する
    /// </summary>
    /// <returns>新しいトラックID</returns>
    public int GenerateId()
    {
        return ++this._latestTrackId;
    }

    /// <summary>
    /// トラック情報を登録する。
    /// </summary>
    /// <param name="trackInfo">トラック情報</param>
    private void AddInternal(TrackInfo trackInfo)
    {
        if (this._registeredTrackIds.Contains(trackInfo.Id))
        {
            throw new InvalidOperationException("トラックIDが重複しています。");
        }

        this._registeredTrackIds.Add(trackInfo.Id);
        this._registeredTracks.Add(trackInfo);
    }

    /// <summary>
    /// トラック情報を登録する。
    /// </summary>
    /// <param name="trackInfo">トラック情報</param>
    /// <returns>トラック情報</returns>
    public TrackInfo Add(TrackInfo trackInfo)
    {
        var dbContext = this._database.Context;
        dbContext.Tracks.Insert(new TrackDataModel
        {
            Id = trackInfo.Id,
            AlbumId = trackInfo.AlbumInfo.Id,
            Path = trackInfo.Path,
            Duration = (int)trackInfo.Duration.TotalMilliseconds,
            Disk = trackInfo.DiskNumber,
            Track = trackInfo.TrackNumber,
            Year = trackInfo.Year,
            AlbumArtist = trackInfo.AlbumArtist,
            Title = trackInfo.Title,
            Artist = trackInfo.Artist,
            Genre = trackInfo.Genre,
            CreatedAt = trackInfo.RegisteredAt,
            UpdatedAt = trackInfo.UpdatedAt,
        });

        this.AddInternal(trackInfo);

        return trackInfo;
    }

    /// <summary>
    /// コレクション内の要素を削除する
    /// </summary>
    public void Clear()
    {
        this._registeredTracks.Clear();
        this._registeredTrackIds.Clear();
    }

    /// <summary>
    /// コレクション内に要素が存在するかを取得する
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(TrackInfo item)
        => this._registeredTracks.Contains(item);

    /// <summary>
    /// コレクションの要素をコピーする
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(TrackInfo[] array, int arrayIndex)
        => this._registeredTracks.CopyTo(array, arrayIndex);

    /// <summary>
    /// コレクション内の要素を削除する
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(TrackInfo item)
    {
        _ = item ?? throw new ArgumentNullException(nameof(item));

        return this._registeredTracks.Remove(item)
            && this._registeredTrackIds.Remove(item.Id);
    }

    /// <summary>
    /// コレクションの要素数を削除する
    /// </summary>
    public int Count => this._registeredTracks.Count;

    /// <summary>
    /// コレクションが読み取り専用であるかどうかを取得する
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// トラック情報を登録する。
    /// </summary>
    /// <param name="tracks">トラック情報</param>
    private void AddInternal(IEnumerable<TrackInfo> tracks)
    {
        var trackIds = tracks.Select(t => t.Id);
        if (trackIds.Any(this._registeredTrackIds.Contains))
        {
            throw new InvalidOperationException("トラックIDが重複しています。");
        }

        this._registeredTrackIds.UnionWith(trackIds);
        this._registeredTracks.AddRange(tracks);
    }

    /// <summary>
    /// トラック情報を登録する。
    /// </summary>
    /// <param name="trackInfo">トラック情報</param>
    /// <returns>トラック情報</returns>
    public void Add(IEnumerable<TrackInfo> tracks)
    {
        var tracksData = tracks.Select(track => new TrackDataModel
        {
            Id = track.Id,
            AlbumId = track.AlbumInfo.Id,
            Path = track.Path,
            Duration = (int)track.Duration.TotalMilliseconds,
            Disk = track.DiskNumber,
            Track = track.TrackNumber,
            Year = track.Year,
            AlbumArtist = track.AlbumArtist,
            Title = track.Title,
            Artist = track.Artist,
            Genre = track.Genre,
            CreatedAt = track.RegisteredAt,
            UpdatedAt = track.UpdatedAt,
        });

        this.AddInternal(tracks);

        var dbContext = this._database.Context;
        dbContext.Tracks.InsertBulk(tracksData);
    }

    /// <summary>
    /// トラックリストをデータベースから読み込む。
    /// </summary>
    /// <param name="albumManager"></param>
    public void Load(AlbumManager albumManager)
    {
        if (this._registeredTracks.Count > 0)
        {
            // トラック情報が1件でも登録されている場合は操作を受け付けない
            throw new InvalidOperationException();
        }

        var tracksByAlbumId = this._database.Context.Tracks.GroupBy(key => key.AlbumId);

        foreach (var trackGroup in tracksByAlbumId)
        {
            var album = albumManager.FromId(trackGroup.Key);
            var tracks = trackGroup.Select(t => new TrackInfo(t, album)).ToArray();

            this.AddInternal(tracks);
            album.Playlist.Tracks.AddRange(tracks);
        }

        if (this._registeredTracks.Count > 0)
        {
            this._latestTrackId = this._registeredTrackIds.Max();
        }
    }

    /// <summary>
    /// <see cref="IEnumerator"/>を取得する
    /// </summary>
    /// <returns></returns>
    public IEnumerator<TrackInfo> GetEnumerator() => this._registeredTracks.GetEnumerator();

    /// <summary>
    /// <see cref="IEnumerator"/>を取得する
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// 要素をつ追加する
    /// </summary>
    /// <param name="item"></param>
    void ICollection<TrackInfo>.Add(TrackInfo item) => this.Add(item);
}
