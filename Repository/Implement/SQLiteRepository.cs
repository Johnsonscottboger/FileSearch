using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Repository.Attribute;
using Repository.Interface;

namespace Repository.Implement
{
    public class SQLiteRepository : IRepository
    {
        private static readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1);

        private int _month;
        private int _year;

        private string _connString = ConfigurationManager.AppSettings["ConnectionString"];

        private IDbConnection _conn;

        /// <summary>
        /// 初始化数据库连接,连接到指定年数据库
        /// </summary>
        /// <param name="year">年份</param>
        public SQLiteRepository()
        {
            var connectionString = this._connString;
            this._conn = new SQLiteConnection(connectionString);
            this._conn.Open();
        }

        /// <summary>
        /// 使用指定连接对象,初始化数据库
        /// </summary>
        /// <param name="conn"></param>
        public SQLiteRepository(IDbConnection conn)
        {
            if (conn == null)
                throw new ArgumentNullException(nameof(conn));
            this._conn = conn;
            this._conn.Open();
        }

        /// <summary>
        /// 获取事务
        /// </summary>
        /// <returns></returns>
        public IDbTransaction DbTransaction
        {
            get { return this._conn.BeginTransaction(); }
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        public IDbConnection Connection
        {
            get { return this._conn; }
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="T">添加的数据类型</typeparam>
        /// <param name="entity">对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public Task AddAsync<T>(T entity, IDbTransaction trans = null)
        {
            return Task.Factory.StartNew(() =>
            {
                CreateTableIfNotExists<T>();
                var builder = new StringBuilder();
                var tableName = (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).ElementAtOrDefault(0) as TableNameAttribute)?.TableName ?? typeof(T).Name;
                builder.Append($"INSERT INTO {tableName} (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Where(p => !p.IsDefined(typeof(KeyAttribute))).Select(p => p.Name)));
                builder.Append(") VALUES (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Where(p => !p.IsDefined(typeof(KeyAttribute))).Select(p => $"@{p.Name}")));
                builder.Append(");");
                var sql = builder.ToString();
                var rowsAffected = this._conn.Execute(sql, entity, trans);
                if (rowsAffected == 0)
                {
                    throw new Exception("添加数据库失败");
                }
            });
        }

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <typeparam name="T">添加的数据类型</typeparam>
        /// <param name="entity">对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public Task AddAsync<T>(IEnumerable<T> entities, IDbTransaction trans = null)
        {
            return Task.Factory.StartNew(() =>
            {
                CreateTableIfNotExists<T>();

                var builder = new StringBuilder();
                var tableName = (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).ElementAtOrDefault(0) as TableNameAttribute)?.TableName ?? typeof(T).Name;
                builder.Append($"INSERT INTO {tableName} (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Where(p => !p.IsDefined(typeof(KeyAttribute))).Select(p => p.Name)));
                builder.Append(") VALUES (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Where(p => !p.IsDefined(typeof(KeyAttribute))).Select(p => $"@{p.Name}")));
                builder.Append(");");
                var sql = builder.ToString();
                trans = trans ?? this._conn.BeginTransaction();
                try
                {
                    var result = Parallel.ForEach(entities, entity =>
                    {
                        var rowsAffected = this._conn.Execute(sql, entity, trans);
                        if (rowsAffected == 0)
                        {
                            throw new Exception("未完全插入");
                        }
                    });

                    if (result.IsCompleted)
                    {
                        trans.Commit();
                    }
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            });
        }

        /// <summary>
        /// 查找全部
        /// </summary>
        /// <typeparam name="T">查找类型</typeparam>
        /// <param name="whereSql">条件查询语句: <code>WHERE ID = @id</code></param>
        /// <param name="param">参数</param>
        /// <returns>所有结果</returns>
        public Task<IEnumerable<T>> FindAllAsync<T>(string whereSql = null, object param = null)
        {
            var tableName = (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).ElementAtOrDefault(0) as TableNameAttribute)?.TableName ?? typeof(T).Name;
            var sql = $@"SELECT {string.Join(", ", typeof(T).GetProperties().Select(p => p.Name))}
                         FROM {tableName}";
            if (!string.IsNullOrWhiteSpace(whereSql))
                sql = $"{sql} {whereSql}";
            return this._conn.QueryAsync<T>(sql, param);
        }

        /// <summary>
        /// 查找第一条
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereSql">条件查询语句: 例如:<code>WHERE ID = @id</code></param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public async Task<T> FindFirstOrDefaultAsync<T>(string whereSql = null, object param = null) where T : class
        {
            var all = await FindAllAsync<T>(whereSql, param);
            return all?.FirstOrDefault();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="param">参数</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public Task UpdateAsync(string sql, object param = null, IDbTransaction trans = null)
        {
            return Task.Factory.StartNew(() =>
            {
                this._conn.Execute(sql, param, trans);
            });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="param">参数</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public Task DeleteAsync(string sql, object param = null, IDbTransaction trans = null)
        {
            return Task.Factory.StartNew(() =>
            {
                this._conn.Execute(sql, param, trans);
            });
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null)
        {
            return Task.Factory.StartNew(() =>
            {
                return this._conn.Execute(sql, param, transaction);
            });
        }

        /// <summary>
        /// 添加或更新,使用索引查找,如果索引列已存在相同的值,则替换之,否则插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public Task AddOrUpdateAsync<T>(T entity, IDbTransaction trans = null)
        {
            return Task.Factory.StartNew(async () =>
            {
                CreateTableIfNotExists<T>();

                var builder = new StringBuilder();
                var tableName = (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).ElementAtOrDefault(0) as TableNameAttribute)?.TableName ?? typeof(T).Name;
                builder.Append($"INSERT OR REPLACE INTO {tableName} (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Select(p => p.Name)));
                builder.Append(") VALUES (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Select(p => $"@{p.Name}")));
                builder.Append(");");
                var sql = builder.ToString();
                await _asyncLock.WaitAsync();
                try
                {
                    var rowsAffected = this._conn.Execute(sql, entity, trans);
                    if (rowsAffected == 0)
                    {
                        throw new Exception("受影响的行数为0");
                    }
                }
                finally
                {
                    _asyncLock.Release();
                }
            });
        }

        /// <summary>
        /// 添加或更新集合,使用索引查找,如果索引列已存在相同的值,则替换之,否则插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        public Task AddOrUpdateAsync<T>(IEnumerable<T> entities, IDbTransaction trans = null)
        {
            return Task.Factory.StartNew(async () =>
            {
                CreateTableIfNotExists<T>();
                var builder = new StringBuilder();
                var tableName = (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).ElementAtOrDefault(0) as TableNameAttribute)?.TableName ?? typeof(T).Name;
                builder.Append($"INSERT OR REPLACE INTO {tableName} (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Select(p => p.Name)));
                builder.Append(") VALUES (");
                builder.Append(string.Join(", ", typeof(T).GetProperties().Select(p => $"@{p.Name}")));
                builder.Append(");");

                await _asyncLock.WaitAsync();
                try
                {
                    trans = trans ?? this._conn.BeginTransaction();
                    var result = Parallel.ForEach(entities, entity =>
                    {
                        var rowsAffected = this._conn.Execute(builder.ToString(), entity, trans);
                        if (rowsAffected == 0)
                        {
                            throw new Exception("添加数据库失败");
                        }
                    });

                    if (result.IsCompleted)
                    {
                        trans.Commit();
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
                finally
                {
                    _asyncLock.Release();
                }
            });
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="param">参数</param>
        /// <param name="trans">事务</param>
        public void Execute(string sql, object param = null, IDbTransaction trans = null)
        {
            this._conn.Execute(sql, param, trans);
        }

        #region 私有方法
        /// <summary>
        /// 创建表,如果表不存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void CreateTableIfNotExists<T>()
        {
            var type = typeof(T);
            var tableName = (typeof(T).GetCustomAttributes(typeof(TableNameAttribute), true).ElementAtOrDefault(0) as TableNameAttribute)?.TableName ?? typeof(T).Name;
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("CREATE TABLE IF NOT EXISTS ").Append(tableName).Append(" (");

            Func<PropertyInfo, string> GetColumnString = (p) =>
            {
                var builder = new StringBuilder();
                builder.Append($@"{p.Name} {GetSqliteTypeName(p.PropertyType)}");
                if (p.IsDefined(typeof(KeyAttribute)))
                    builder.Append(" PRIMARY KEY ");
                if (p.IsDefined(typeof(AutoIncrementAttribute)))
                    builder.Append(" AUTOINCREMENT ");
                if (p.IsDefined(typeof(RequiredAttribute)))
                    builder.Append(" NOT NULL ");
                return builder.ToString();
            };

            sqlBuilder.Append(string.Join(", ", type.GetProperties().Select(p => GetColumnString(p))));
            sqlBuilder.Append(");");

            _asyncLock.Wait();
            try
            {
                this._conn.Execute(sqlBuilder.ToString());
            }
            finally
            {
                _asyncLock.Release();
            }

            sqlBuilder.Clear();
            //创建索引
            var indexColumnNames = type.GetProperties().Where(p => p.IsDefined(typeof(IndexAttribute))).Select(p => p.Name);
            _asyncLock.Wait();
            try
            {
                if (type.GetProperties().Any(p => p.IsDefined(typeof(IndexAttribute))) && !IsExistsIndex($"{type.Name}{string.Join("", indexColumnNames)}Index"))
                {
                    sqlBuilder.Append("CREATE INDEX ").Append(type.Name).Append(string.Join("", indexColumnNames)).Append("Index ON ").Append(tableName).Append("(");
                    sqlBuilder.Append(string.Join(", ", indexColumnNames));
                    sqlBuilder.Append(");");
                    this._conn.Execute(sqlBuilder.ToString());
                }
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        /// <summary>
        /// 判断索引是否存在
        /// </summary>
        /// <param name="indexName">索引名称</param>
        /// <returns>是否能存在</returns>
        private bool IsExistsIndex(string indexName)
        {
            var sql = $"SELECT COUNT(*) AS Count FROM sqlite_master WHERE type = 'index' AND name = '{indexName}'";
            return this._conn.Query(sql).Any(p => p.Count > 0);
        }

        /// <summary>
        /// 根据C#类型返回对应的SQLite类型名称
        /// </summary>
        /// <param name="type">C#类型</param>
        /// <returns>对应的SQLite类型名称</returns>
        private string GetSqliteTypeName(Type type)
        {
            var dict = new Dictionary<Type, string>()
            {
                { typeof(Boolean), "BIT"},
                { typeof(DateTime),"DATETIME"},
                { typeof(Decimal), "DECIMAL"},
                { typeof(Int32),   "INT"},
                { typeof(Int64),   "INTEGER"},
                { typeof(String),  "NVARCHAR"},
                { typeof(Double),  "REAL"},
                { typeof(Single),  "SINGLE"},
                { typeof(Int16),   "SMALLINT"},
                { typeof(UInt16),  "SMALLUINT"},
                { typeof(Byte),    "TINYINT"},
                { typeof(SByte),   "TINYSINT"},
                { typeof(UInt32),  "UINT"},
                { typeof(Guid),    "UNIQUEIDENTIFIER"},
                { typeof(UInt64),  "UNSIGNEDINTEGER"},
                { typeof(Single?), "SINGLE"},
                { typeof(Int32?),  "INT"},
                { typeof(Int64?),  "INTEGER"},
                { typeof(DateTime?),"DATETIME"},
            };
            return dict[type];
        }

        #endregion
    }
}
