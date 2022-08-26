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
        #region QueryById<T> 根据Id查询实体
        /// <summary>
        /// 根据Id查询实体
        /// </summary>
        public T QueryById<T>(object id) where T : new()
        {
            Type type = typeof(T);

            DbParameter[] cmdParms = new DbParameter[1];
            string idName = GetIdName(type);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            cmdParms[0] = _provider.GetDbParameter(_parameterMark + idName, id);

            string sql = string.Format("select * from {0} where {3}={1}{2}", GetTableName(_provider, type), _parameterMark, idName, idNameWithQuote);

            object result = Find(type, sql, cmdParms);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        #region QueryByIdAsync<T> 根据Id查询实体
        /// <summary>
        /// 根据Id查询实体
        /// </summary>
        public async Task<T> QueryByIdAsync<T>(object id) where T : new()
        {
            Type type = typeof(T);

            DbParameter[] cmdParms = new DbParameter[1];
            string idName = GetIdName(type);
            string idNameWithQuote = _provider.OpenQuote + idName + _provider.CloseQuote;
            cmdParms[0] = _provider.GetDbParameter(_parameterMark + idName, id);

            string sql = string.Format("select * from {0} where {3}={1}{2}", GetTableName(_provider, type), _parameterMark, idName, idNameWithQuote);

            object result = await FindAsync(type, sql, cmdParms);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion


        #region Query<T> 根据sql查询实体
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public T Query<T>(string sql) where T : new()
        {
            Type type = typeof(T);
            object result = Find(type, sql, null);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        #region QueryAsync<T> 根据sql查询实体
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public async Task<T> QueryAsync<T>(string sql) where T : new()
        {
            Type type = typeof(T);
            object result = await FindAsync(type, sql, null);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion


        #region Query<T> 根据sql查询实体(参数化查询)
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public T Query<T>(string sql, DbParameter[] args) where T : new()
        {
            Type type = typeof(T);
            object result = Find(type, sql, args);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion

        #region QueryAsync<T> 根据sql查询实体(参数化查询)
        /// <summary>
        /// 根据sql查询实体
        /// </summary>
        public async Task<T> QueryAsync<T>(string sql, DbParameter[] args) where T : new()
        {
            Type type = typeof(T);
            object result = await FindAsync(type, sql, args);

            if (result != null)
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }
        #endregion


        #region Find 根据实体查询实体
        /// <summary>
        /// 根据实体查询实体
        /// </summary>
        private object Find(object obj)
        {
            Type type = obj.GetType();

            string sql = string.Format("select * from {0} where {1}", GetTableName(_provider, type), CreatePkCondition(_provider, obj.GetType(), obj));

            return Find(type, sql, null);
        }
        #endregion

        #region Find 根据实体查询实体
        /// <summary>
        /// 根据实体查询实体
        /// </summary>
        private Task<object> FindAsync(object obj)
        {
            Type type = obj.GetType();

            string sql = string.Format("select * from {0} where {1}", GetTableName(_provider, type), CreatePkCondition(_provider, obj.GetType(), obj));

            return FindAsync(type, sql, null);
        }
        #endregion

        #region Find 查询实体
        /// <summary>
        /// 查询实体
        /// </summary>
        private object Find(Type type, string sql, DbParameter[] parameters)
        {
            SetTypeMap(type);
            OnExecuting?.Invoke(sql, parameters);

            return _conn.QueryFirstOrDefault(type, sql, ToDynamicParameters(parameters));
        }
        #endregion

        #region FindAsync 查询实体
        /// <summary>
        /// 查询实体
        /// </summary>
        private async Task<object> FindAsync(Type type, string sql, DbParameter[] parameters)
        {
            SetTypeMap(type);
            OnExecuting?.Invoke(sql, parameters);

            return await _conn.QueryFirstOrDefaultAsync(type, sql, ToDynamicParameters(parameters));
        }
        #endregion

    }
}
