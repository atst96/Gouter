using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Components.TypeHandlers
{
    internal class DateTimeOffsetTypeHandler : Dapper.SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            if (value is string strValue)
            {
                return DateTimeOffset.Parse(strValue);
            }

            throw new NotSupportedException();
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.ToString();
        }
    }
}
