﻿using System;
using System.Threading.Tasks;

namespace Dapper.LiteSql
{
    /// <summary>
    /// LiteSqlClient
    /// </summary>
    public class LiteSqlClient : ILiteSqlClient
    {
        #region 变量
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string _connectionString;

        /// <summary>
        /// 数据库类型
        /// </summary>
        private DBType? _dbType;

        /// <summary>
        /// 数据库提供者类型
        /// </summary>
        private Type _providerType;

        /// <summary>
        /// 主键自增全局配置
        /// </summary>
        private bool _autoIncrement;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="provider">数据库Provider</param>
        /// <param name="autoIncrement">主键自增全局配置(如果实体类或实体类的主键添加了AutoIncrementAttribute特性则不使用全局配置)</param>
        public LiteSqlClient(string connectionString, DBType dbType, IProvider provider, bool autoIncrement = false)
        {
            _connectionString = connectionString;
            _dbType = dbType;
            _autoIncrement = autoIncrement;

            ProviderFactory.RegisterDBProvider(dbType, provider);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="providerType">数据库提供者类型</param>
        /// <param name="provider">数据库Provider</param>
        /// <param name="autoIncrement">主键自增全局配置(如果实体类或实体类的主键添加了AutoIncrementAttribute特性则不使用全局配置)</param>
        public LiteSqlClient(string connectionString, Type providerType, IProvider provider, bool autoIncrement = false)
        {
            _connectionString = connectionString;
            _providerType = providerType;
            _autoIncrement = autoIncrement;

            ProviderFactory.RegisterDBProvider(providerType, provider);
        }
        #endregion

        #region 获取 ISession
        /// <summary>
        /// 获取 ISession
        /// </summary>
        public ISession GetSession(SplitTableMapping splitTableMapping = null)
        {
            DBSession dbSession;

            if (_dbType != null)
            {
                dbSession = new DBSession(_connectionString, _dbType.Value, splitTableMapping, _autoIncrement);
            }
            else
            {
                dbSession = new DBSession(_connectionString, _providerType, splitTableMapping, _autoIncrement);
            }
            return dbSession;
        }
        #endregion

        #region 获取 ISession (异步)
        /// <summary>
        /// 获取 ISession (异步)
        /// </summary>
        public Task<ISession> GetSessionAsync(SplitTableMapping splitTableMapping = null)
        {
            DBSession dbSession;

            if (_dbType != null)
            {
                dbSession = new DBSession(_connectionString, _dbType.Value, splitTableMapping, _autoIncrement);
            }
            else
            {
                dbSession = new DBSession(_connectionString, _providerType, splitTableMapping, _autoIncrement);
            }
            return Task.FromResult(dbSession as ISession);
        }
        #endregion

    }
}
