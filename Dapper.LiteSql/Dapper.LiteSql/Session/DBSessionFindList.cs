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

            return _conn.Query<T>(sql).ToList();
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

            return (await _conn.QueryAsync<T>(sql)).ToList();
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

            return _conn.Query<T>(sql, ToDynamicParameters(cmdParms)).ToList();
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

            return (await _conn.QueryAsync<T>(sql, ToDynamicParameters(cmdParms))).ToList();
        }
        #endregion

        #region 查询列表(传SqlString)
        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>(SqlString sql) where T : new()
        {
            return QueryList<T>(sql.SQL, sql.Params);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        public Task<List<T>> QueryListAsync<T>(SqlString sql) where T : new()
        {
            return QueryListAsync<T>(sql.SQL, sql.Params);
        }
        #endregion
    }
}
