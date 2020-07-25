using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;

namespace Gouter.Components
{
    /// <summary>
    /// データセット
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class DbSet<TEntity> : IEnumerable<TEntity>
    {
        private readonly IDbConnection _dbConnection;

        public DbSet(IDbConnection dbConnection)
        {
            this._dbConnection = dbConnection;
        }

        /// <summary>
        /// すべてまたは指定条件のレコードを削除する。
        /// </summary>
        /// <param name="statementOptions">オプション</param>
        /// <returns>削除されたレコードの件数</returns>
        public int BulkDelete(Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.BulkDelete(statementOptions);

        /// <summary>
        /// すべてまたは指定条件のレコードを削除する。
        /// </summary>
        /// <param name="statementOptions">オプション</param>
        /// <returns>削除されたレコードの件数</returns>
        public Task<int> BulkDeleteAsync(Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.BulkDeleteAsync(statementOptions);

        /// <summary>
        /// 複数のレコードを更新する。
        /// </summary>
        /// <param name="updateData">主キーを除く更新内容</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>更新されたレコードの件数</returns>
        public int BulkUpdate(TEntity updateData, Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.BulkUpdate(updateData, statementOptions);

        /// <summary>
        /// 複数のレコードを更新する。
        /// </summary>
        /// <param name="updateData">主キーを除く更新内容</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>更新されたレコードの件数</returns>
        public Task<int> BulkUpdateAsync(TEntity updateData, Action<IConditionalBulkSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.BulkUpdateAsync(updateData, statementOptions);

        /// <summary>
        /// すべてまたは指定条件のレコード数を取得する。
        /// </summary>
        /// <param name="statementOptions">オプション</param>
        /// <returns>レコードの件数</returns>
        public int Count(Action<IConditionalSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.Count(statementOptions);

        /// <summary>
        /// すべてまたは指定条件のレコード数を取得する。
        /// </summary>
        /// <param name="statementOptions">オプション</param>
        /// <returns>レコードの件数</returns>
        public Task<int> CountAsync(Action<IConditionalSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.CountAsync(statementOptions);

        /// <summary>
        /// 指定のレコードを削除する。
        /// </summary>
        /// <param name="entityToDelete">削除するレコード</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>削除結果</returns>
        public bool Delete(TEntity entityToDelete, Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.Delete(entityToDelete, statementOptions);

        /// <summary>
        /// 指定のレコードを削除する。
        /// </summary>
        /// <param name="entityToDelete">削除するレコード</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>削除結果</returns>
        public Task<bool> DeleteAsync(TEntity entityToDelete, Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.DeleteAsync(entityToDelete, statementOptions);

        /// <summary>
        /// すべてまたは指定条件のレコードを取得する。
        /// </summary>
        /// <param name="statementOptions">オプション</param>
        /// <returns>取得結果</returns>
        public IEnumerable<TEntity> Find(Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.Find(statementOptions);

        /// <summary>
        /// すべてまたは指定条件のレコードを取得する。
        /// </summary>
        /// <param name="statementOptions">オプション</param>
        /// <returns>取得結果</returns>
        public Task<IEnumerable<TEntity>> FindAsync(Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.FindAsync(statementOptions);

        /// <summary>
        /// 指定のレコードを取得する。
        /// </summary>
        /// <param name="entityKeys">取得するレコード（主キー）</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>取得結果。一致するレコードがなければnull</returns>
        public TEntity Get(TEntity entityKeys, Action<ISelectSqlSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.Get(entityKeys, statementOptions);

        /// <summary>
        /// 指定のレコードを取得する。
        /// </summary>
        /// <param name="entityKeys">取得するレコード（主キー）</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>取得結果。一致するレコードがなければnull</returns>
        public Task<TEntity> GetAsync(TEntity entityKeys, Action<ISelectSqlSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.GetAsync(entityKeys, statementOptions);

        public IEnumerator<TEntity> GetEnumerator()
            => this.Find().GetEnumerator();

        /// <summary>
        /// レコードを挿入する。
        /// </summary>
        /// <param name="entityToInsert">レコード内容</param>
        /// <param name="statementOptions">オプション</param>
        public void Insert(TEntity entityToInsert, Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.Insert(entityToInsert, statementOptions);

        /// <summary>
        /// レコードを挿入する。
        /// </summary>
        /// <param name="entityToInsert">レコード内容</param>
        /// <param name="statementOptions">オプション</param>
        public Task InsertAsync(TEntity entityToInsert, Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.InsertAsync(entityToInsert, statementOptions);

        /// <summary>
        /// 単一レコードを更新する。
        /// </summary>
        /// <param name="entityToUpdate">更新するレコード内容</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>更新結果</returns>
        public bool Update(TEntity entityToUpdate, Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this._dbConnection.Update(entityToUpdate, statementOptions);

        /// <summary>
        /// 単一レコードを更新する。
        /// </summary>
        /// <param name="entityToUpdate">更新するレコード内容</param>
        /// <param name="statementOptions">オプション</param>
        /// <returns>更新結果</returns>
        public Task<bool> UpdateAsync(TEntity entityToUpdate, Action<IStandardSqlStatementOptionsBuilder<TEntity>> statementOptions = null)
            => this.UpdateAsync(entityToUpdate, statementOptions);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
