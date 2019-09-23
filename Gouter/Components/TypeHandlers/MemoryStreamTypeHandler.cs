using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Gouter.Components.TypeHandlers
{
    internal class MemoryStreamTypeHandler : SqlMapper.TypeHandler<MemoryStream>
    {
        public override MemoryStream Parse(object value)
        {
            if (value is byte[] bytesData)
            {
                return new MemoryStream(bytesData);
            }
            else if (value == null)
            {
                return null;
            }

            throw new NotSupportedException();
        }

        public override void SetValue(IDbDataParameter parameter, MemoryStream value)
        {
            parameter.Value = value?.ToArray();
        }
    }
}
