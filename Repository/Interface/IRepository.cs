using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IRepository
    {
        /// <summary>
        /// 获取一个事务
        /// </summary>
        /// <returns></returns>
        IDbTransaction DbTransaction { get; }

        /// <summary>
        /// 获取连接对象
        /// </summary>
        /// <returns></returns>
        IDbConnection Connection { get; }

        /// <summary>
        /// 查找所有
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereSql">WHERE 语句</param>
        /// <param name="param">参数</param>
        /// <returns>所有结果</returns>
        Task<IEnumerable<T>> FindAllAsync<T>(string whereSql = null, object param = null);

        /// <summary>
        /// 查找第一条结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereSql">WHERE 语句</param>
        /// <param name="param">参数</param>
        /// <returns>第一条结果</returns>
        Task<T> FindFirstOrDefaultAsync<T>(string whereSql = null, object param = null) where T : class;

        /// <summary>
        /// 添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        Task AddAsync<T>(T entity, IDbTransaction trans = null);

        /// <summary>
        /// 添加集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        Task AddAsync<T>(IEnumerable<T> entities, IDbTransaction trans = null);

        /// <summary>
        /// 添加或更新,使用索引查找,如果索引列已存在相同的值,则替换之,否则插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        Task AddOrUpdateAsync<T>(T entity, IDbTransaction trans = null);

        /// <summary>
        /// 添加或更新集合,使用索引查找,如果索引列已存在相同的值,则替换之,否则插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="trans">事务</param>
        /// <returns></returns>
        Task AddOrUpdateAsync<T>(IEnumerable<T> entities, IDbTransaction trans = null);

        Task UpdateAsync(string sql, object param = null, IDbTransaction trans = null);

        Task DeleteAsync(string sql, object param = null, IDbTransaction trans = null);

        Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null);

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        void Execute(string sql, object param = null, IDbTransaction trans = null);
    }
}
