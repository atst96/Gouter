using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Extensions
{
    internal static class SQLiteExtensions
    {
        public static T Get<T>(this SQLiteDataReader reader, int columnIndex)
        {
            return (T)reader.GetValue(columnIndex);
        }

        public static T GetOrDefault<T>(this SQLiteDataReader reader, int columnIndex)
        {
            return reader.GetValue(columnIndex) is T value ? value : default;
        }

        public static T Get<T>(this SQLiteDataReader reader, string columnName)
        {
            return (T)reader[columnName];
        }

        public static T GetOrDefault<T>(this SQLiteDataReader reader, string columnName)
        {
            return reader[columnName] is T value ? value : default;
        }
    }
}
