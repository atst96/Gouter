using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlKata.Execution;

namespace Gouter.DataModels
{
    internal class TrackDataModel : DataModelBase<TrackDataModel>
    {
        public static string TableName { get; } = Database.TableNames.Tracks;

        public int Id { get; set; }
        public int AlbumId { get; set; }
        public string Path { get; set; }
        public int Duration { get; set; }
        public int? Disk { get; set; }
        public int? Track { get; set; }
        public int? Year { get; set; }
        public string AlbumArtist { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        private static SqlKata.Query GetQueryBuilder()
        {
            return GetQueryBuilder(TableName);
        }

        public static IEnumerable<TrackDataModel> GetAll()
        {
            return GetQueryBuilder().Get<TrackDataModel>();
        }

        public void Insert()
        {
            GetQueryBuilder().Insert(new Dictionary<string, object>
            {
                ["id"] = this.Id,
                ["album_id"] = this.AlbumId,
                ["path"] = this.Path,
                ["duration"] = this.Duration,
                ["disk"] = this.Disk,
                ["track"] = this.Track,
                ["year"] = this.Year,
                ["album_artist"] = this.AlbumArtist,
                ["title"] = this.Title,
                ["artist"] = this.Artist,
                ["genre"] = this.Genre,
                ["created_at"] = this.CreatedAt,
                ["updated_at"] = this.UpdatedAt,
            });
        }

        public void Delete()
        {
            GetQueryBuilder().Where("id", this.Id).Delete();
        }
    }
}
