using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;

namespace Dapper.LiteSql
{
    public partial class DBSession : ISession
    {
        #region SQL打印
        /// <summary>
        /// SQL打印
        /// </summary>
        public Action<string, DbParameter[]> OnExecuting { get; set; }
        #endregion

        #region  执行简单SQL语句

        #region Exists 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exists(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = _conn.ExecuteScalar(sqlString);

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region ExistsAsync 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await _conn.ExecuteScalarAsync(sqlString);

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion


        #region QuerySingle<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public T QuerySingle<T>(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = _conn.ExecuteScalar(sqlString);

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingle 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public object QuerySingle(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = _conn.ExecuteScalar(sqlString);

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion

        #region QuerySingleAsync<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public async Task<T> QuerySingleAsync<T>(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await _conn.ExecuteScalarAsync(sqlString);

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingleAsync 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        public async Task<object> QuerySingleAsync(string sqlString)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await _conn.ExecuteScalarAsync(sqlString);

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion


        #region QueryCount 查询数量
        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql, int pageSize, out long pageCount)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = QueryCount(sql);
            pageCount = (count - 1) / pageSize + 1;
            return count;
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns>查询结果的数量</returns>
        public async Task<CountResult> QueryCountAsync(string sql, int pageSize)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = await QueryCountAsync(sql);
            long pageCount = (count - 1) / pageSize + 1;
            return new CountResult(count, pageCount);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);
            return QuerySingle<long>(sql);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>查询结果的数量</returns>
        public Task<long> QueryCountAsync(string sql)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);
            return QuerySingleAsync<long>(sql);
        }
        #endregion

        #endregion

        #region 执行带参SQL语句

        #region Exists 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exists(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = _conn.ExecuteScalar(sqlString, ToDynamicParameters(cmdParms));

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region ExistsAsync 是否存在
        /// <summary>
        /// 是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await _conn.ExecuteScalarAsync(sqlString, ToDynamicParameters(cmdParms));

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion


        #region QuerySingle<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public T QuerySingle<T>(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = _conn.ExecuteScalar(sqlString, ToDynamicParameters(cmdParms));

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingle 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public object QuerySingle(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = _conn.ExecuteScalar(sqlString, ToDynamicParameters(cmdParms));

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion

        #region QuerySingleAsync<T> 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public async Task<T> QuerySingleAsync<T>(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await _conn.ExecuteScalarAsync(sqlString, ToDynamicParameters(cmdParms));

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }
        #endregion

        #region QuerySingleAsync 查询单个值
        /// <summary>
        /// 查询单个值
        /// </summary>
        /// <param name="sqlString">查询语句</param>
        /// <param name="cmdParms">参数</param>
        public async Task<object> QuerySingleAsync(string sqlString, DbParameter[] cmdParms)
        {
            SqlFilter(ref sqlString);
            OnExecuting?.Invoke(sqlString, null);

            object obj = await _conn.ExecuteScalarAsync(sqlString, ToDynamicParameters(cmdParms));

            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                return null;
            }
            else
            {
                return obj;
            }
        }
        #endregion


        #region QueryCount 查询数量
        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql, DbParameter[] cmdParms, int pageSize, out long pageCount)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = QueryCount(sql, cmdParms);
            pageCount = (count - 1) / pageSize + 1;
            return count;
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns>查询结果的数量</returns>
        public async Task<CountResult> QueryCountAsync(string sql, DbParameter[] cmdParms, int pageSize)
        {
            if (pageSize <= 0) throw new Exception("pageSize不能小于或等于0");
            long count = await QueryCountAsync(sql, cmdParms);
            long pageCount = (count - 1) / pageSize + 1;
            return new CountResult(count, pageCount);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>查询结果的数量</returns>
        public long QueryCount(string sql, DbParameter[] cmdParms)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);

            return long.Parse(_conn.ExecuteScalar(sql, ToDynamicParameters(cmdParms)).ToString());
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>查询结果的数量</returns>
        public async Task<long> QueryCountAsync(string sql, DbParameter[] cmdParms)
        {
            sql = string.Format("select count(*) from ({0}) T", sql);

            return long.Parse((await _conn.ExecuteScalarAsync(sql, ToDynamicParameters(cmdParms))).ToString());
        }
        #endregion

        #endregion

    }
}
