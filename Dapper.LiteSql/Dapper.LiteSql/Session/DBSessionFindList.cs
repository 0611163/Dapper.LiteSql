using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Dapper.LiteSql
{
    public partial class DBSession : ISession
    {
        #region QueryList<T> 查询列表
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(string sql) where T : new()
        {
            SqlFilter(ref sql);
            SetTypeMap<T>();
            OnExecuting?.Invoke(sql, null);

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                return _conn.Conn.Query<T>(sql).ToList();
            }
        }
        #endregion

        #region QueryListAsync<T> 查询列表
        /// <summary>
        /// 查询列表
        /// </summary>
        public async Task<List<T>> QueryListAsync<T>(string sql) where T : new()
        {
            SqlFilter(ref sql);
            SetTypeMap<T>();
            OnExecuting?.Invoke(sql, null);

            using (_conn = await DbConnectionFactory.GetConnectionAsync(_provider, _connectionString, _tran))
            {
                return (await _conn.Conn.QueryAsync<T>(sql)).ToList();
            }
        }
        #endregion


        #region QueryList<T> 查询列表(参数化查询)
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(string sql, DbParameter[] cmdParms) where T : new()
        {
            SetTypeMap<T>();
            OnExecuting?.Invoke(sql, cmdParms);

            using (_conn = DbConnectionFactory.GetConnection(_provider, _connectionString, _tran))
            {
                return _conn.Conn.Query<T>(sql, ToDynamicParameters(cmdParms)).ToList();
            }
        }
        #endregion

        #region QueryListAsync<T> 查询列表(参数化查询)
        /// <summary>
        /// 查询列表
        /// </summary>
        public async Task<List<T>> QueryListAsync<T>(string sql, DbParameter[] cmdParms) where T : new()
        {
            SetTypeMap<T>();
            OnExecuting?.Invoke(sql, cmdParms);

            using (_conn = await DbConnectionFactory.GetConnectionAsync(_provider, _connectionString, _tran))
            {
                return (await _conn.Conn.QueryAsync<T>(sql, ToDynamicParameters(cmdParms))).ToList();
            }
        }
        #endregion

        #region 查询列表(传SqlString)
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(ISqlString sql) where T : new()
        {
            return QueryList<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        public Task<List<T>> QueryListAsync<T>(ISqlString sql) where T : new()
        {
            return QueryListAsync<T>(sql.SQL, sql.Params);
        }
        #endregion
    }
}
