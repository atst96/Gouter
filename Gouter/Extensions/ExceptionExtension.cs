using System;
using System.Linq;

namespace Gouter.Extensions
{
    /// <summary>
    /// 例外クラス関係の拡張メソッド
    /// </summary>
    internal static class ExceptionExtension
    {
        /// <summary>
        /// 例外からメッセージを生成する
        /// </summary>
        /// <param name="exception">例外オブジェクト</param>
        /// <returns></returns>
        public static string GetMessage(this Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                // AggregateExceptionの場合は複数の例外メッセージを結合する

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
