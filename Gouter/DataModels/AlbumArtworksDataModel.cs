using System;
using System.ComponentModel.DataAnnotations.Schema;
using LiteDB;

namespace Gouter.DataModels;

/// <summary>
/// アートワーク情報のデータモデル
/// </summary>
[Table("album_artworks")]
internal class AlbumArtworksDataModel
{
    /// <summary>
    /// アルバムID
    /// </summary>
    [BsonId(false)]
    public int AlbumId { get; set; }

    /// <summary>
    /// アートワーク
    /// </summary>
    [BsonField("artwork")]
    public byte[] Artwork { get; set; }

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
