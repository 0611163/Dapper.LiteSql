﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.LiteSql
{
    public class SqlString<T> : SqlString where T : new()
    {
        #region 构造函数
        public SqlString(IProvider provider, ISession session, string sql = null, params object[] args)
            : base(provider, session, sql, args)
        {

        }
        #endregion

        #region WhereIf
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="expression">Lambda 表达式</param>
        public SqlString<T> WhereIf(bool condition, Expression<Func<T, object>> expression)
        {
            if (condition)
            {
                Where(expression);
            }

            return this;
        }
        #endregion

        #region WhereIf
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="expression">Lambda 表达式</param>
        public SqlString<T> WhereIf<U>(bool condition, Expression<Func<U, object>> expression)
        {
            if (condition)
            {
                Where<U>(expression);
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public SqlString<T> Where(Expression<Func<T, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " and (" + condition.VisitLambda(expression, out dbParameters) + ")";

                _sql.Append(result);
                if (dbParameters != null)
                {
                    _paramList.AddRange(dbParameters.ToList());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public SqlString<T> Where<U>(Expression<Func<U, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " and (" + condition.VisitLambda(expression, out dbParameters) + ")";

                _sql.Append(result);
                if (dbParameters != null)
                {
                    _paramList.AddRange(dbParameters.ToList());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public SqlString<T> Where<U>(Expression<Func<T, U, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " and (" + condition.VisitLambda(expression, out dbParameters) + ")";

                _sql.Append(result);
                if (dbParameters != null)
                {
                    _paramList.AddRange(dbParameters.ToList());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return this;
        }
        #endregion

        #region Where
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="expression">Lambda 表达式</param>
        public SqlString<T> Where<U, D>(Expression<Func<T, U, D, object>> expression)
        {
            try
            {
                ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.Where);

                DbParameter[] dbParameters;
                string result = " and (" + condition.VisitLambda(expression, out dbParameters) + ")";

                _sql.Append(result);
                if (dbParameters != null)
                {
                    _paramList.AddRange(dbParameters.ToList());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return this;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// 追加 order by SQL
        /// </summary>
        public SqlString<T> OrderBy(Expression<Func<T, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.OrderBy);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            if (!_orderBySQL.Contains("order by"))
            {
                _orderBySQL += string.Format(" order by {0} asc ", sql);
            }
            else
            {
                _orderBySQL += string.Format(", {0} asc ", sql);
            }

            return this;
        }
        #endregion

        #region OrderByDescending
        /// <summary>
        /// 追加 order by SQL
        /// </summary>
        public SqlString<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.OrderByDescending);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            if (!_orderBySQL.Contains("order by"))
            {
                _orderBySQL += string.Format(" order by {0} desc ", sql);
            }
            else
            {
                _orderBySQL += string.Format(", {0} desc ", sql);
            }

            return this;
        }
        #endregion

        #region LeftJoin
        /// <summary>
        /// 追加 left join SQL
        /// </summary>
        public SqlString<T> LeftJoin<U>(Expression<Func<T, U, object>> expression)
        {
            ExpressionHelper<T> condition = new ExpressionHelper<T>(this, _provider, _dbParameterNames, SqlStringMethod.LeftJoin);
            DbParameter[] dbParameters;
            string sql = condition.VisitLambda(expression, out dbParameters);

            string tableName = _dbSession.GetTableName(_provider, typeof(U));

            string alias = sql.Split('=')[1].Split('.')[0].Trim();

            _sql.Replace("where 1=1", string.Empty);
            _sql.AppendFormat(" left join {0} {1} on {2} where 1=1 ", tableName, alias, sql);

            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// 创建单表查询SQL
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="alias">别名，默认值t</param>
        public SqlString<T> Select(string alias = null)
        {
            Type type = typeof(T);
            alias = alias ?? "t";

            _sql.AppendFormat("select ", _dbSession.GetTableName(_provider, type));

            PropertyInfoEx[] propertyInfoExArray = DBSession.GetEntityProperties(type);
            foreach (PropertyInfoEx propertyInfoEx in propertyInfoExArray)
            {
                PropertyInfo propertyInfo = propertyInfoEx.PropertyInfo;
                if (propertyInfo.GetCustomAttribute<ColumnAttribute>() != null)
                {
                    _sql.AppendFormat("{0}.{1}{2}{3},", alias, _provider.OpenQuote, propertyInfoEx.FieldName, _provider.CloseQuote);
                }
            }

            _sql.Remove(_sql.Length - 1, 1);

            _sql.AppendFormat(" from {0} {1}", _dbSession.GetTableName(_provider, type), alias);

            _sql.Append(" where 1=1 ");

            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// 追加 select SQL
        /// </summary>
        public SqlString<T> Select<U>(Expression<Func<U, object>> expression, Expression<Func<T, object>> expression2)
        {
            DbParameter[] dbParameters;

            ExpressionHelper<U> condition = new ExpressionHelper<U>(this, _provider, _dbParameterNames, SqlStringMethod.Select);
            string sql = condition.VisitLambda(expression, out dbParameters);

            ExpressionHelper<U> condition2 = new ExpressionHelper<U>(this, _provider, _dbParameterNames, SqlStringMethod.Select);
            string sql2 = condition.VisitLambda(expression2, out dbParameters);

            string[] leftRigth = _sql.ToString().Split(new string[] { "from" }, StringSplitOptions.None);
            string left = leftRigth[0];
            string right = leftRigth[1];

            _sql = new StringBuilder(string.Format("{0}, {1} as {2} from {3}", left, sql, sql2.Split('.')[1].Trim(), right));

            return this;
        }
        #endregion

        #region ToList
        /// <summary>
        /// 执行查询
        /// </summary>
        public List<T> ToList()
        {
            return _session.QueryList<T>(this.SQL + this._orderBySQL, this.Params);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public async Task<List<T>> ToListAsync()
        {
            return await _session.QueryListAsync<T>(this.SQL + this._orderBySQL, this.Params);
        }
        #endregion

        #region ToPageList
        /// <summary>
        /// 执行查询
        /// </summary>
        public List<T> ToPageList(int page, int pageSize)
        {
            return _session.QueryPage<T>(this.SQL, this._orderBySQL, pageSize, page, this.Params);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        public async Task<List<T>> ToPageListAsync(int page, int pageSize)
        {
            return await _session.QueryPageAsync<T>(this.SQL, this._orderBySQL, pageSize, page, this.Params);
        }
        #endregion

        #region Count
        /// <summary>
        /// 返回数量
        /// </summary>
        public long Count()
        {
            return _session.QueryCount(this.SQL + this._orderBySQL, this.Params);
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        public long CountAsync()
        {
            return _session.QueryCount(this.SQL + this._orderBySQL, this.Params);
        }
        #endregion

        #region FirstOrDefault
        /// <summary>
        /// 返回数量
        /// </summary>
        public T FirstOrDefault()
        {
            return _session.QueryList<T>(this.SQL + this._orderBySQL, this.Params).FirstOrDefault();
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        public async Task<T> FirstOrDefaultAsync()
        {
            return (await _session.QueryListAsync<T>(this.SQL + this._orderBySQL, this.Params)).FirstOrDefault();
        }
        #endregion

        #region First
        /// <summary>
        /// 返回数量
        /// </summary>
        public T First()
        {
            return _session.QueryList<T>(this.SQL + this._orderBySQL, this.Params).First();
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        public async Task<T> FirstAsync()
        {
            return (await _session.QueryListAsync<T>(this.SQL + this._orderBySQL, this.Params)).First();
        }
        #endregion

        #region Exists
        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exists()
        {
            return _session.Exists(this.SQL + this._orderBySQL, this.Params);
        }

        /// <summary>
        /// 返回数量
        /// </summary>
        public async Task<bool> ExistsAsync()
        {
            return await _session.ExistsAsync(this.SQL + this._orderBySQL, this.Params);
        }
        #endregion

    }
}