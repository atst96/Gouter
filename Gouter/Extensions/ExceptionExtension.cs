using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Extensions
{
    internal static class ExceptionExtension
    {
        public static string GetMessage(this Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                var messages = aggregateException.InnerExceptions.Select(ex => ex.GetMessage());

                return string.Join("\n\n--\n", messages);
            }
            else
            {
                return exception.Message;
            }
        }
    }
}
