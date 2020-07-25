using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gouter.DataModels
{
    /// <summary>
    /// アートワーク情報のデータモデル
    /// </summary>
    [Table("album_artworks")]
    internal class AlbumArtworksDataModel
    {
        /// <summary>
        /// アルバムID
        /// </summary>
        [Column("album_id"), Key]
        public int AlbumId { get; set; }

        /// <summary>
        /// アートワーク
        /// </summary>
        [Column("artwork")]
        public byte[] Artwork { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
