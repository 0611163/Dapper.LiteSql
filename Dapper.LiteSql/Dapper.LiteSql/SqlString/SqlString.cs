﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dapper.LiteSql
{
    /// <summary>
    /// 参数化查询SQL字符串
    /// </summary>
    public class SqlString : ISqlString
    {
        #region 变量属性

        protected IProvider _provider;

        protected StringBuilder _sql = new StringBuilder();

        protected List<DbParameter> _paramList = new List<DbParameter>();

        protected Regex _regex = new Regex(@"[@|:]([a-zA-Z_]{1}[a-zA-Z0-9_]+)", RegexOptions.IgnoreCase);

        /// <summary>
        /// 参数化查询的参数
        /// </summary>
        public DbParameter[] Params { get { return _paramList.ToArray(); } }

        /// <summary>
        /// 参数化查询的SQL
        /// </summary>
        public string SQL { get { return _sql.ToString(); } }

        /// <summary>
        /// SQL参数的参数名称(防止参数名称重名)
        /// </summary>
        protected HashSet<string> _dbParameterNames = new HashSet<string>();

        protected ISession _session;

        protected DBSession _dbSession;

        /// <summary>
        /// 子查询SQL集合
        /// </summary>
        protected List<string> _subSqls = new List<string>();

        #endregion

        #region 构造函数
        public SqlString(IProvider provider, ISession session, string sql = null, params object[] args)
        {
            _provider = provider;
            _session = session;
            _dbSession = session as DBSession;

            if (sql != null)
            {
                Append(sql, args);
            }
        }
        #endregion

        #region Append
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="args">参数(支持多个参数或者把多个参数放在一个匿名对象中)</param>
        public ISqlString Append(string sql, params object[] args)
        {
            if (args == null) throw new Exception("参数args不能为null");

            //匿名对象处理
            bool anonymousType = false;
            Dictionary<string, object> dictValues = new Dictionary<string, object>();
            if (args.Length == 1)
            {
                Type type = args[0].GetType();
                if (type.Name.Contains("<>f__AnonymousType"))
                {
                    anonymousType = true;
                    PropertyInfo[] props = type.GetProperties();
                    foreach (PropertyInfo propInfo in props)
                    {
                        dictValues.Add(propInfo.Name, propInfo.GetValue(args[0]));
                    }
                }
            }

            Dictionary<string, object> dict = new Dictionary<string, object>();
            MatchCollection mc = _regex.Matches(sql);
            int argIndex = 0;
            foreach (Match m in mc)
            {
                string val1 = m.Groups[1].Value;
                if (!dict.ContainsKey(val1))
                {
                    dict.Add(val1, null);
                    Type parameterType = typeof(object);
                    if (anonymousType)
                    {
                        if (dictValues.ContainsKey(val1))
                        {
                            object obj = dictValues[val1];
                            if (obj != null) parameterType = obj.GetType();
                        }
                    }
                    else
                    {
                        if (argIndex < args.Length)
                        {
                            object obj = args[argIndex];
                            if (obj != null) parameterType = obj.GetType();
                        }
                    }
                    sql = ReplaceSql(sql, m.Value, val1, parameterType);
                }
            }

            if (!anonymousType && args.Length < dict.Keys.Count) throw new Exception("SqlString.AppendFormat参数不够");

            List<string> keyList = dict.Keys.ToList();
            for (int i = 0; i < keyList.Count; i++)
            {
                string key = keyList[i];
                object value;
                if (anonymousType)
                {
                    if (dictValues.ContainsKey(key))
                    {
                        value = dictValues[key];
                    }
                    else
                    {
                        throw new Exception("参数" + key + "缺少值");
                    }
                }
                else
                {
                    value = args[i];
                }
                Type valueType = value != null ? value.GetType() : null;

                if (valueType == typeof(SqlValue))
                {
                    SqlValue sqlValue = value as SqlValue;
                    Type parameterType = sqlValue.Value.GetType();
                    if (sqlValue.Value.GetType().Name != typeof(List<>).Name)
                    {
                        string markKey = _provider.GetParameterName(key, parameterType);
                        sql = sql.Replace(markKey, string.Format(sqlValue.Sql, markKey));
                        _paramList.Add(_provider.GetDbParameter(key, sqlValue.Value));
                    }
                    else
                    {
                        string markKey = _provider.GetParameterName(key, parameterType);
                        sql = sql.Replace(markKey, string.Format(sqlValue.Sql, markKey));
                        string[] keyArr = sqlValue.Sql.Replace("(", string.Empty).Replace(")", string.Empty).Replace("@", string.Empty).Split(',');
                        IList valueList = (IList)sqlValue.Value;
                        for (int k = 0; k < valueList.Count; k++)
                        {
                            object item = valueList[k];
                            _paramList.Add(_provider.GetDbParameter(keyArr[k], item));
                        }
                    }
                }
                else
                {
                    _paramList.Add(_provider.GetDbParameter(key, value));
                }
            }

            _sql.Append(string.Format(" {0} ", sql.Trim()));

            return this;
        }
        #endregion

        #region Append
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="args">参数(支持多个参数或者把多个参数放在一个匿名对象中)</param>
        public ISqlQueryable<T> Append<T>(string sql, params object[] args) where T : new()
        {
            return Append(sql, args) as ISqlQueryable<T>;
        }

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="subSql">子SQL</param>
        public ISqlString Append(string sql, ISqlString subSql)
        {
            _sql.Append(sql + " (" + subSql.SQL + ")");
            _paramList.AddRange(subSql.Params);
            return this;
        }

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="subSql">子SQL</param>
        public ISqlQueryable<T> Append<T>(string sql, ISqlString subSql) where T : new()
        {
            _sql.Append(sql + " (" + subSql.SQL + ")");
            _paramList.AddRange(subSql.Params);
            return this as ISqlQueryable<T>; ;
        }
        #endregion

        #region AppendIf
        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="sql">SQL</param>
        /// <param name="args">参数(支持多个参数或者把多个参数放在一个匿名对象中)</param>
        public ISqlQueryable<T> AppendIf<T>(bool condition, string sql, params object[] args) where T : new()
        {
            return AppendIf(condition, sql, args) as ISqlQueryable<T>;
        }

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="sql">SQL</param>
        /// <param name="argsFunc">参数</param>
        public ISqlQueryable<T> AppendIf<T>(bool condition, string sql, params Func<object>[] argsFunc) where T : new()
        {
            return AppendIf(condition, sql, argsFunc) as ISqlQueryable<T>;
        }

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="sql">SQL</param>
        /// <param name="args">参数</param>
        public ISqlString AppendIf(bool condition, string sql, params object[] args)
        {
            if (condition)
            {
                Append(sql, args);
            }

            return this;
        }

        /// <summary>
        /// 追加参数化SQL
        /// </summary>
        /// <param name="condition">当condition等于true时追加SQL，等于false时不追加SQL</param>
        /// <param name="sql">SQL</param>
        /// <param name="argsFunc">参数</param>
        public ISqlString AppendIf(bool condition, string sql, params Func<object>[] argsFunc)
        {
            if (condition)
            {
                object[] args = new object[argsFunc.Length];
                for (int i = 0; i < argsFunc.Length; i++)
                {
                    args[i] = argsFunc[i]();
                }

                Append(sql, args);
            }

            return this;
        }
        #endregion

        #region AppendFormat
        /// <summary>
        /// 封装 StringBuilder AppendFormat 追加非参数化SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="args">参数</param>
        public ISqlQueryable<T> AppendFormat<T>(string sql, params object[] args) where T : new()
        {
            return AppendFormat(sql, args) as ISqlQueryable<T>;
        }

        /// <summary>
        /// 封装 StringBuilder AppendFormat 追加非参数化SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="args">参数</param>
        public ISqlString AppendFormat(string sql, params object[] args)
        {
            if (_regex.IsMatch(sql)) throw new Exception("追加参数化SQL请使用Append");
            _sql.AppendFormat(string.Format(" {0} ", sql.Trim()), args);

            return this;
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            return _sql.ToString();
        }
        #endregion

        #region ReplaceSql
        /// <summary>
        /// 调用该方法的原因：参数化查询，SQL语句中统一使用@，而有的数据库不是@
        /// </summary>
        private string ReplaceSql(string sql, string oldStr, string name, Type parameterType)
        {
            string newStr = _provider.GetParameterName(name, parameterType);
            return sql.Replace(oldStr, newStr);
        }
        #endregion

        #region ForContains
        /// <summary>
        /// 创建 Like SQL
        /// </summary>
        public SqlValue ForContains(string value)
        {
            return _provider.ForContains(value);
        }
        #endregion

        #region ForStartsWith
        /// <summary>
        /// 创建 Like SQL
        /// </summary>
        public SqlValue ForStartsWith(string value)
        {
            return _provider.ForStartsWith(value);
        }
        #endregion

        #region ForEndsWith
        /// <summary>
        /// 创建 Like SQL
        /// </summary>
        public SqlValue ForEndsWith(string value)
        {
            return _provider.ForEndsWith(value);
        }
        #endregion

        #region ForDateTime
        /// <summary>
        /// 创建 日期时间类型转换 SQL
        /// </summary>
        /// <param name="dateTime">日期时间</param>
        public SqlValue ForDateTime(DateTime dateTime)
        {
            return _provider.ForDateTime(dateTime);
        }
        #endregion

        #region ForList
        /// <summary>
        /// 创建 in 或 not in SQL
        /// </summary>
        public SqlValue ForList(IList list)
        {
            return _provider.ForList(list);
        }
        #endregion

        #region RemoveSubSqls
        /// <summary>
        /// 返回移除子查询后的SQL
        /// </summary>
        protected string RemoveSubSqls(string sql)
        {
            StringBuilder sb = new StringBuilder(sql);
            foreach (string subSql in _subSqls)
            {
                sb.Replace(subSql, string.Empty);
            }
            return sb.ToString();
        }
        #endregion

        #region 实现ISqlString增删改查接口

        /// <summary>
        /// 查询实体
        /// </summary>
        public T Query<T>() where T : new()
        {
            return _session.Query<T>(this);
        }

        /// <summary>
        /// 查询实体
        /// </summary>
        public Task<T> QueryAsync<T>() where T : new()
        {
            return _session.QueryAsync<T>(this);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        public List<T> QueryList<T>() where T : new()
        {
            return _session.QueryList<T>(this);
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        public Task<List<T>> QueryListAsync<T>() where T : new()
        {
            return _session.QueryListAsync<T>(this);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public List<T> QueryPage<T>(string orderby, int pageSize, int currentPage) where T : new()
        {
            return _session.QueryPage<T>(this, orderby, pageSize, currentPage);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public Task<List<T>> QueryPageAsync<T>(string orderby, int pageSize, int currentPage) where T : new()
        {
            return _session.QueryPageAsync<T>(this, orderby, pageSize, currentPage);
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        public int DeleteByCondition<T>()
        {
            return _session.DeleteByCondition<T>(this);
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync<T>()
        {
            return _session.DeleteByConditionAsync<T>(this);
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        public int DeleteByCondition(Type type)
        {
            return _session.DeleteByCondition(type, this);
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        public Task<int> DeleteByConditionAsync(Type type)
        {
            return _session.DeleteByConditionAsync(type, this);
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        public int Execute()
        {
            return _session.Execute(this);
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        public Task<int> ExecuteAsync()
        {
            return _session.ExecuteAsync(this);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exists()
        {
            return _session.Exists(this);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        public Task<bool> ExistsAsync()
        {
            return _session.ExistsAsync(this);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public object QuerySingle()
        {
            return _session.QuerySingle(this);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public T QuerySingle<T>()
        {
            return _session.QuerySingle<T>(this);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public Task<object> QuerySingleAsync()
        {
            return _session.QuerySingleAsync(this);
        }

        /// <summary>
        /// 查询单个值
        /// </summary>
        public Task<T> QuerySingleAsync<T>()
        {
            return _session.QuerySingleAsync<T>(this);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        public long QueryCount()
        {
            return _session.QueryCount(this);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        public Task<long> QueryCountAsync()
        {
            return _session.QueryCountAsync(this);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        public long QueryCount(int pageSize, out long pageCount)
        {
            return _session.QueryCount(this, pageSize, out pageCount);
        }

        /// <summary>
        /// 给定一条查询SQL，返回其查询结果的数量
        /// </summary>
        public Task<CountResult> QueryCountAsync(int pageSize)
        {
            return _session.QueryCountAsync(this, pageSize);
        }

        #endregion

    }
}
