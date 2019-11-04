using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlKata.Execution;

namespace Gouter.DataModels
{
    /// <summary>
    /// アルバム情報のデータモデル
    /// </summary>
    internal class AlbumDataModel : DataModelBase<AlbumDataModel>
    {
        /// <summary>テーブル名</summary>
        public static string TableName { get; } = Database.TableNames.Albums;

        public int Id { get; set; }
        public string Key { get; set; }
        public string Artist { get; set; }
        public string Name { get; set; }
        public bool? IsCompilation { get; set; }
        public byte[] Artwork { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>クエリビルダを取得する</summary>
        /// <param name="database">データベース</param>
        /// <returns>クエリビルダ</returns>
        public static SqlKata.Query GetQueryBuilder(Database database)
        {
            return GetQueryBuilder(database, TableName);
        }

        /// <summary>テーブル内のアルバム情報を全件取得する</summary>
        /// <param name="database">データベース</param>
        /// <returns>アルバム情報一覧</returns>
        public static IEnumerable<AlbumDataModel> GetAll(Database database)
        {
            var awTableName = Database.TableNames.AlbumArtworks;

            return GetQueryBuilder(database)
                .LeftJoin(awTableName, $"{TableName}.id", $"{awTableName}.album_id")
                .Get<AlbumDataModel>();
        }

        /// <summary>テーブルにアルバム情報を登録する</summary>
        /// <param name="database">データベース</param>
        public void Insert(Database database)
        {
            var artworkStream = this.Artwork != null
                ? new MemoryStream(this.Artwork)
                : null;

            var data = new Dictionary<string, object>
            {
                ["id"] = this.Id,
                ["key"] = this.Key,
                ["artist"] = this.Artist,
                ["name"] = this.Name,
                ["is_compilation"] = this.IsCompilation,
                ["created_at"] = this.CreatedAt,
                ["updated_at"] = this.UpdatedAt,
            };

            var artworkData = new Dictionary<string, object>
            {
                ["album_id"] = this.Id,
                ["artwork"] = artworkStream,
                ["created_at"] = this.CreatedAt,
                ["updated_at"] = this.UpdatedAt,
            };

            GetQueryBuilder(database).Insert(data);

            if (artworkStream != null)
            {
                GetQueryBuilder(database, Database.TableNames.AlbumArtworks).Insert(artworkData);
            }

            artworkStream?.Dispose();
        }

        /// <summary>テーブルからアルバム情報を削除する</summary>
        /// <param name="database">アルバム名</param>
        public void Delete(Database database)
        {
            GetQueryBuilder(database).Where("id", this.Id).Delete();
        }
    }
}
