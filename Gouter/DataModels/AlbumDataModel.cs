using System;
using System.ComponentModel.DataAnnotations.Schema;
using LiteDB;

namespace Gouter.DataModels;

/// <summary>
/// アルバム情報のデータモデル
/// </summary>
[Table("albums")]
internal class AlbumDataModel
{
    /// <summary>
    /// アルバムID
    /// </summary>
    [BsonId(false)]
    public int Id { get; set; }

    /// <summary>
    /// アルバムキー
    /// </summary>
    [BsonField("key")]
    public string Key { get; set; }

    /// <summary>
    /// アーティスト名
    /// </summary>
    [BsonField("artist")]
    public string Artist { get; set; }

    /// <summary>
    /// データID
    /// </summary>
    [BsonField("artwork_id")]
    public string ArtworkId { get; set; }

    /// <summary>
    /// アルバム名
    /// </summary>
    [BsonField("name")]
    public string Name { get; set; }

    /// <summary>
    /// コンピレーションアルバムかどうかのフラグ
    /// </summary>
    [BsonField("is_compilation")]
    public bool? IsCompilation { get; set; }

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
