using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.LiteSql
{
    /// <summary>
    /// 标识数据库表
    /// </summary>
    [Serializable, AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }

        /// <summary>
        /// 标识数据库表
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        public TableAttribute(string tableName = null)
        {
            TableName = tableName;
        }
    }
}
