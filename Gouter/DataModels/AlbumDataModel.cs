﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlKata.Execution;

namespace Gouter.DataModels
{
    internal class AlbumDataModel : DataModelBase<AlbumDataModel>
    {
        public static string TableName { get; } = Database.TableNames.Albums;

        public int Id { get; set; }
        public string Key { get; set; }
        public string Artist { get; set; }
        public string Name { get; set; }
        public bool? IsCompilation { get; set; }
        public byte[] Artwork { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public static SqlKata.Query GetQueryBuilder()
        {
            return GetQueryBuilder(TableName);
        }

        public static IEnumerable<AlbumDataModel> GetAll()
        {
            var awTableName = Database.TableNames.AlbumArtworks;

            return GetQueryBuilder()
                .LeftJoin(awTableName, $"{TableName}.id", $"{awTableName}.album_id")
                .Get<AlbumDataModel>();
        }

        public void Insert()
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

            GetQueryBuilder().Insert(data);

            if (artworkStream != null)
            {
                GetQueryBuilder(Database.TableNames.AlbumArtworks).Insert(artworkData);
            }

            artworkStream?.Dispose();
        }

        public void Delete()
        {
            GetQueryBuilder().Where("id", this.Id).Delete();
        }
    }
}
