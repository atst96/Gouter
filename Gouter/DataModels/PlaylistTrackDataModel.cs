using System;
using System.ComponentModel.DataAnnotations.Schema;
using LiteDB;

namespace Gouter.DataModels;

[Table("playlist_tracks")]
internal class PlaylistTrackDataModel
{
    /// <summary>
    /// プレイリストID
    /// </summary>
    [BsonField("playlist_id")]
    public int PlaylistId { get; set; }

    /// <summary>
    /// トラックID
    /// </summary>
    [BsonField("track_id")]
    public int TrackId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    [BsonField("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
