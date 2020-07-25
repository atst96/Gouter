using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gouter.DataModels
{
    /// <summary>
    /// トラック情報のデータモデル
    /// </summary>
    [Table("tracks")]
    internal class TrackDataModel
    {
        /// <summary>
        /// トラックID
        /// </summary>
        [Column("id"), Key, Required]
        public int Id { get; set; }

        /// <summary>
        /// アルバムID
        /// </summary>
        [Column("album_id")]
        public int AlbumId { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        [Column("path")]
        public string Path { get; set; }

        /// <summary>
        /// 尺
        /// </summary>
        [Column("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// ディスク番号
        /// </summary>
        [Column("disk")]
        public int? Disk { get; set; }

        /// <summary>
        /// トラック番号
        /// </summary>
        [Column("track")]
        public int? Track { get; set; }

        /// <summary>
        /// 年
        /// </summary>
        [Column("year")]
        public int? Year { get; set; }

        /// <summary>
        /// アルバムアーティスト
        /// </summary>
        [Column("album_artist")]
        public string AlbumArtist { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        [Column("title")]
        public string Title { get; set; }

        /// <summary>
        /// アーティスト
        /// </summary>
        [Column("artist")]
        public string Artist { get; set; }

        /// <summary>
        /// ジャンル
        /// </summary>
        [Column("genre")]
        public string Genre { get; set; }

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
