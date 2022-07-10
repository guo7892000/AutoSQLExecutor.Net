using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DataBaseType
    {
        /// <summary>
        /// SqlServer数据库
        /// </summary>
        SqlServer,

        /// <summary>
        /// Oracle数据库
        /// </summary>
        Oracle,

        /// <summary>
        /// MySql数据库
        /// </summary>
        MySql,

        /// <summary>
        /// SQLite数据库
        /// </summary>
        SQLite,

        /// <summary>
        /// PostgreSql数据库
        /// </summary>
        PostgreSql,
    }
}
