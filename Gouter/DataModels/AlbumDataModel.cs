using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gouter.DataModels
{
    /// <summary>
    /// アルバム情報のデータモデル
    /// </summary>
    [Table("albums")]
    internal class AlbumDataModel
    {
        /// <summary>
        /// アルバムID
        /// </summary>
        [Column("id"), Key]
        public int Id { get; set; }

        /// <summary>
        /// アルバムキー
        /// </summary>
        [Column("key")]
        public string Key { get; set; }

        /// <summary>
        /// アーティスト名
        /// </summary>
        [Column("artist")]
        public string Artist { get; set; }

        /// <summary>
        /// アルバム名
        /// </summary>
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// コンピレーションアルバムかどうかのフラグ
        /// </summary>
        [Column("is_compilation")]
        public bool? IsCompilation { get; set; }

        /// <summary>
        /// アートワーク
        /// </summary>
        [NotMapped]
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
