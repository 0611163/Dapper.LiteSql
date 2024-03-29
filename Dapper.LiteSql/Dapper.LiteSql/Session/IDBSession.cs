﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.LiteSql
{
    /// <summary>
    /// IDBSession接口
    /// 一个IDBSession实例对应一个数据库连接，一个IDBSession实例只有一个数据库连接
    /// IDBSession不是线程安全的，不能跨线程使用
    /// </summary>
    public partial interface IDBSession : IDisposable
    {
        #region 创建SqlString对象
        /// <summary>
        /// 创建SqlString对象
        /// </summary>
        ISqlString CreateSql(string sql = null, params object[] args);
        #endregion

        #region 创建SqlString对象
        /// <summary>
        /// 创建SqlString对象
        /// </summary>
        ISqlQueryable<T> CreateSql<T>(string sql = null, params object[] args) where T : new();

        /// <summary>
        /// 创建IQueryable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="alias">别名，默认值t</param>
        ISqlQueryable<T> Queryable<T>(string alias = null) where T : new();

        /// <summary>
        /// 创建IQueryable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="expression">返回匿名对象的表达式</param>
        ISqlQueryable<T> Queryable<T>(Expression<Func<T, object>> expression) where T : new();
        #endregion

        #region 查询下一个ID
        /// <summary>
        /// 查询下一个ID
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        int QueryNextId<T>();
        #endregion

        #region ForList
        /// <summary>
        /// 创建 in 或 not in SQL
        /// </summary>
        SqlValue ForList(IList list);
        #endregion

        #region 从连接池池获取连接
        /// <summary>
        /// 从连接池池获取连接
        /// </summary>
        DbConnectionExt GetConnection(DbTransactionExt _tran = null);

        /// <summary>
        /// 从连接池池获取连接
        /// </summary>
        Task<DbConnectionExt> GetConnectionAsync(DbTransactionExt _tran = null);
        #endregion

        #region 设置 数据库字段名与实体类属性名映射
        /// <summary>
        /// 设置 数据库字段名与实体类属性名映射
        /// </summary>
        void SetTypeMap(Type type);

        /// <summary>
        /// 设置 数据库字段名与实体类属性名映射
        /// </summary>
        void SetTypeMap<T>();
        #endregion

    }
}
