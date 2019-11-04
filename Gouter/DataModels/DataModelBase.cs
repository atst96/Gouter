using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.DataModels
{
    /// <summary>
    /// データモデルの基底クラス
    /// </summary>
    internal class DataModelBase
    {
        /// <summary>クエリビルダを取得する</summary>
        /// <param name="database">データベース</param>
        /// <param name="tableName">テーブル名</param>
        /// <returns>クエリビルダ</returns>
        public static SqlKata.Query GetQueryBuilder(Database database, string tableName)
        {
            return database.GetQueryBuilder().Query(tableName);
        }
    }

    /// <summary>
    /// データモデルの基底クラス
    /// </summary>
    /// <typeparam name="T">モデルの型</typeparam>
    internal class DataModelBase<T> : DataModelBase where T: DataModelBase<T>
    {

    }
}
