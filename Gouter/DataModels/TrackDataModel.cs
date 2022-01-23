using System;
using System.ComponentModel.DataAnnotations.Schema;
using LiteDB;

namespace Gouter.DataModels;

/// <summary>
/// トラック情報のデータモデル
/// </summary>
[Table("tracks")]
internal class TrackDataModel
{
    /// <summary>
    /// トラックID
    /// </summary>
    [BsonId(false)]
    public int Id { get; set; }

    /// <summary>
    /// アルバムID
    /// </summary>
    [BsonField("album_id")]
    public int AlbumId { get; set; }

    /// <summary>
    /// ファイルパス
    /// </summary>
    [BsonField("path")]
    public string Path { get; set; }

    /// <summary>
    /// 尺
    /// </summary>
    [BsonField("duration")]
    public int Duration { get; set; }

    /// <summary>
    /// ディスク番号
    /// </summary>
    [BsonField("disk")]
    public int? Disk { get; set; }

    /// <summary>
    /// トラック番号
    /// </summary>
    [BsonField("track")]
    public int? Track { get; set; }

    /// <summary>
    /// 年
    /// </summary>
    [BsonField("year")]
    public int? Year { get; set; }

    /// <summary>
    /// アルバムアーティスト
    /// </summary>
    [BsonField("album_artist")]
    public string AlbumArtist { get; set; }

    /// <summary>
    /// タイトル
    /// </summary>
    [BsonField("title")]
    public string Title { get; set; }

    /// <summary>
    /// アーティスト
    /// </summary>
    [BsonField("artist")]
    public string Artist { get; set; }

    /// <summary>
    /// ジャンル
    /// </summary>
    [BsonField("genre")]
    public string Genre { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    [BsonField("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    [BsonField("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}
