﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.LiteSql
{
    /// <summary>
    /// MSSQL 数据库提供者基类
    /// </summary>
    public class SQLiteProviderBase : IProvider
    {
        #region OpenQuote 引号
        /// <summary>
        /// 引号
        /// </summary>
        public string OpenQuote
        {
            get
            {
                return "`";
            }
        }
        #endregion

        #region CloseQuote 引号
        /// <summary>
        /// 引号
        /// </summary>
        public string CloseQuote
        {
            get
            {
                return "`";
            }
        }
        #endregion

        #region 创建 DbConnection
        public virtual DbConnection CreateConnection(string connectionString) { return null; }
        #endregion

        #region 生成 DbParameter
        public virtual DbParameter GetDbParameter(string name, object value) { return null; }
        #endregion

        #region GetParameterMark
        public string GetParameterMark()
        {
            return ":";
        }
        #endregion

        #region 创建获取最大编号SQL
        public string CreateGetMaxIdSql(string tableName, string key)
        {
            return string.Format("SELECT Max(cast({0} as int)) FROM {1}", key, tableName);
        }
        #endregion

        #region 创建分页SQL
        public string CreatePageSql(string sql, string orderby, int pageSize, int currentPage)
        {
            StringBuilder sb = new StringBuilder();
            int startRow = 0;
            int endRow = 0;

            #region 分页查询语句
            startRow = pageSize * (currentPage - 1);

            sb.Append(sql);
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                sb.Append(" ");
                sb.Append(orderby);
            }
            sb.AppendFormat(" limit {0} offset {1}", pageSize, startRow);
            #endregion

            return sb.ToString();
        }
        #endregion

        #region ForContains
        public SqlValue ForContains(string value)
        {
            return new SqlValue(" '%' || {0} || '%' ", value);
        }
        #endregion

        #region ForStartsWith
        public SqlValue ForStartsWith(string value)
        {
            return new SqlValue(" {0} || '%' ", value);
        }
        #endregion

        #region ForEndsWith
        public SqlValue ForEndsWith(string value)
        {
            return new SqlValue(" '%' || {0} ", value);
        }
        #endregion

        #region ForDateTime
        public SqlValue ForDateTime(DateTime dateTime)
        {
            return new SqlValue(" strftime({0}, '%Y-%m-%d %H:%M:%S') ", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        #endregion

        #region ForList
        public SqlValue ForList(IList list)
        {
            List<string> argList = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                argList.Add(":inParam" + i);
            }
            string args = string.Join(",", argList);

            return new SqlValue("(" + args + ")", list);
        }
        #endregion

    }
}