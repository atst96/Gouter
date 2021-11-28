using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gouter.Utils;
using LiteDB;

namespace Gouter.Components
{
    /// <summary>
    /// データセット
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class DbSet<TEntity> : ILiteCollection<TEntity>, IEnumerable<TEntity>
    {
        private readonly ILiteDatabase _connection;

        private readonly string _collectionName = string.Empty;

        private readonly ILiteCollection<TEntity> _collection;

        public DbSet(ILiteDatabase connection)
        {
            this._connection = connection;
            this._collectionName = DbUtil.GetTableName<TEntity>();
            this._collection = connection.GetCollection<TEntity>(this._collectionName);
            this.Initialize();
        }

        /// <summary>
        /// 初期化時
        /// </summary>
        protected virtual void Initialize()
        {
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return this.FindAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TEntity>)this).GetEnumerator();

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return this._collection.FindAll().GetEnumerator();
        }

        #region IDataCollection<T>の実装

        public string Name => this._collection.Name;

        public BsonAutoId AutoId => this._collection.AutoId;

        public EntityMapper EntityMapper => this._collection.EntityMapper;

        public ILiteCollection<TEntity> Include<K>(Expression<Func<TEntity, K>> keySelector)
        {
            return this._collection.Include(keySelector);
        }

        public ILiteCollection<TEntity> Include(BsonExpression keySelector)
        {
            return this._collection.Include(keySelector);
        }

        public bool Upsert(TEntity entity)
        {
            return this._collection.Upsert(entity);
        }

        public int Upsert(IEnumerable<TEntity> entities)
        {
            return this._collection.Upsert(entities);
        }

        public bool Upsert(BsonValue id, TEntity entity)
        {
            return this._collection.Upsert(id, entity);
        }

        public bool Update(TEntity entity)
        {
            return this._collection.Update(entity);
        }

        public bool Update(BsonValue id, TEntity entity)
        {
            return this._collection.Update(id, entity);
        }

        public int Update(IEnumerable<TEntity> entities)
        {
            return this._collection.Update(entities);
        }

        public int UpdateMany(BsonExpression transform, BsonExpression predicate)
        {
            return this._collection.UpdateMany(transform, predicate);
        }

        public int UpdateMany(Expression<Func<TEntity, TEntity>> extend, Expression<Func<TEntity, bool>> predicate)
        {
            return this._collection.UpdateMany(extend, predicate);
        }

        public BsonValue Insert(TEntity entity)
        {
            return this._collection.Insert(entity);
        }

        public void Insert(BsonValue id, TEntity entity)
        {
            this._collection.Insert(id, entity);
        }

        public int Insert(IEnumerable<TEntity> entities)
        {
            return this._collection.Insert(entities);
        }

        public int InsertBulk(IEnumerable<TEntity> entities, int batchSize = 5000)
        {
            return this._collection.InsertBulk(entities, batchSize);
        }

        public bool EnsureIndex(string name, BsonExpression expression, bool unique = false)
        {
            return this._collection.EnsureIndex(name, expression, unique);
        }

        public bool EnsureIndex(BsonExpression expression, bool unique = false)
        {
            return this._collection.EnsureIndex(expression, unique);
        }

        public bool EnsureIndex<K>(Expression<Func<TEntity, K>> keySelector, bool unique = false)
        {
            return this._collection.EnsureIndex(keySelector, unique);
        }

        public bool EnsureIndex<K>(string name, Expression<Func<TEntity, K>> keySelector, bool unique = false)
        {
            return this._collection.EnsureIndex(name, keySelector, unique);
        }

        public bool DropIndex(string name)
        {
            return this._collection.DropIndex(name);
        }

        public ILiteQueryable<TEntity> Query()
        {
            return this._collection.Query();
        }

        public IEnumerable<TEntity> Find(BsonExpression predicate, int skip = 0, int limit = int.MaxValue)
        {
            return this._collection.Find(predicate, skip, limit);
        }

        public IEnumerable<TEntity> Find(LiteDB.Query query, int skip = 0, int limit = int.MaxValue)
        {
            return this._collection.Find(query, skip, limit);
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            return this._collection.Find(predicate, skip, limit);
        }

        public TEntity FindById(BsonValue id)
        {
            return this._collection.FindById(id);
        }

        public TEntity FindOne(BsonExpression predicate)
        {
            return this._collection.FindOne(predicate);
        }

        public TEntity FindOne(string predicate, BsonDocument parameters)
        {
            return this._collection.FindOne(predicate, parameters);
        }

        public TEntity FindOne(BsonExpression predicate, params BsonValue[] args)
        {
            return this._collection.FindOne(predicate, args);
        }

        public TEntity FindOne(Expression<Func<TEntity, bool>> predicate)
        {
            return this._collection.FindOne(predicate);
        }

        public TEntity FindOne(LiteDB.Query query)
        {
            return this._collection.FindOne(query);
        }

        public IEnumerable<TEntity> FindAll()
        {
            return this._collection.FindAll();
        }

        public bool Delete(BsonValue id)
        {
            return this._collection.Delete(id);
        }

        public int DeleteAll()
        {
            return this._collection.DeleteAll();
        }

        public int DeleteMany(BsonExpression predicate)
        {
            return this._collection.DeleteMany(predicate);
        }

        public int DeleteMany(string predicate, BsonDocument parameters)
        {
            return this._collection.DeleteMany(predicate, parameters);
        }

        public int DeleteMany(string predicate, params BsonValue[] args)
        {
            return this._collection.DeleteMany(predicate, args);
        }

        public int DeleteMany(Expression<Func<TEntity, bool>> predicate)
        {
            return this._collection.DeleteMany(predicate);
        }

        public int Count()
        {
            return this._collection.Count();
        }

        public int Count(BsonExpression predicate)
        {
            return this._collection.Count(predicate);
        }

        public int Count(string predicate, BsonDocument parameters)
        {
            return this._collection.Count(predicate, parameters);
        }

        public int Count(string predicate, params BsonValue[] args)
        {
            return this._collection.Count(predicate, args);
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return this._collection.Count(predicate);
        }

        public int Count(LiteDB.Query query)
        {
            return this._collection.Count(query);
        }

        public long LongCount()
        {
            return this._collection.LongCount();
        }

        public long LongCount(BsonExpression predicate)
        {
            return this._collection.LongCount(predicate);
        }

        public long LongCount(string predicate, BsonDocument parameters)
        {
            return this._collection.LongCount(predicate, parameters);
        }

        public long LongCount(string predicate, params BsonValue[] args)
        {
            return this._collection.LongCount(predicate, args);
        }

        public long LongCount(Expression<Func<TEntity, bool>> predicate)
        {
            return this._collection.LongCount(predicate);
        }

        public long LongCount(LiteDB.Query query)
        {
            return this._collection.LongCount(query);
        }

        public bool Exists(BsonExpression predicate)
        {
            return this._collection.Exists(predicate);
        }

        public bool Exists(string predicate, BsonDocument parameters)
        {
            return this._collection.Exists(predicate, parameters);
        }

        public bool Exists(string predicate, params BsonValue[] args)
        {
            return this._collection.Exists(predicate, args);
        }

        public bool Exists(Expression<Func<TEntity, bool>> predicate)
        {
            return this._collection.Exists(predicate);
        }

        public bool Exists(LiteDB.Query query)
        {
            return this._collection.Exists(query);
        }

        public BsonValue Min(BsonExpression keySelector)
        {
            return this._collection.Min(keySelector);
        }

        public BsonValue Min()
        {
            return this._collection.Min();
        }

        public K Min<K>(Expression<Func<TEntity, K>> keySelector)
        {
            return this._collection.Min(keySelector);
        }

        public BsonValue Max(BsonExpression keySelector)
        {
            return this._collection.Max(keySelector);
        }

        public BsonValue Max()
        {
            return this._collection.Max();
        }

        public K Max<K>(Expression<Func<TEntity, K>> keySelector)
        {
            return this._collection.Max(keySelector);
        }

        #endregion
    }
}
