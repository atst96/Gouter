using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Gouter.Utils
{
    /// <summary>
    /// データベースに関するユーティリティクラス
    /// </summary>
    internal static class DbUtil
    {
        /// <summary>
        /// データモデルのテーブル名を取得する。
        /// </summary>
        /// <typeparam name="TDataModel">データモデルの型</typeparam>
        /// <returns></returns>
        public static string GetTableName<TDataModel>()
        {
            var modelType = typeof(TDataModel);

            // Table属性を取得する
            var tableAttr = modelType.GetCustomAttribute<TableAttribute>();
            if (tableAttr != null)
            {
                // Table属性が指定されていればテーブル名を返す。
                return tableAttr.Name;
            }

            return modelType.Name;
        }
    }
}
