using System;
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

        public static SqlKata.Query GetQueryBuilder()
        {
            return GetQueryBuilder(TableName);
        }

        public static IEnumerable<AlbumDataModel> GetAll()
        {
            return GetQueryBuilder().Get<AlbumDataModel>();
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
                ["artwork"] = artworkStream,
            };

            GetQueryBuilder().Insert(data);
        }

        public void Delete()
        {
            GetQueryBuilder().Where("id", this.Id).Delete();
        }
    }
}
