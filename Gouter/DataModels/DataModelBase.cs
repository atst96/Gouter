using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.DataModels
{
    internal class DataModelBase
    {
        public static SqlKata.Query GetQueryBuilder(Database database, string tableName)
        {
            return database.GetQueryBuilder().Query(tableName);
        }
    }

    internal class DataModelBase<T> : DataModelBase where T: DataModelBase<T>
    {

    }
}
