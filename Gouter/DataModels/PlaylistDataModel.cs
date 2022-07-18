using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using LiteDB;

namespace Gouter.DataModels;

/// <summary>
/// プレイリスト情報のデータモデル
/// </summary>
[Table("playlists")]
internal class PlaylistDataModel
{
    /// <summary>
    /// プレイリストID
    /// </summary>
    [BsonId(true)]
    [BsonField("id")]
    public int Id { get; set; }

    /// <summary>
    /// アルバム名
    /// </summary>
    [BsonField("name")]
    public string Name { get; set; }

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
