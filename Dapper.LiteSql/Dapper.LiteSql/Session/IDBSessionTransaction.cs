﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.LiteSql
{
    public partial interface IDBSession
    {
        #region 开始事务
        /// <summary>
        /// 开始事务
        /// </summary>
        DbTransactionExt BeginTransaction();
        #endregion

        #region 提交事务
        /// <summary>
        /// 提交事务
        /// </summary>
        void CommitTransaction();
        #endregion

        #region 回滚事务(出错时调用该方法回滚)
        /// <summary>
        /// 回滚事务(出错时调用该方法回滚)
        /// </summary>
        void RollbackTransaction();
        #endregion

    }
}
