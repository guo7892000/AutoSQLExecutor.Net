using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// SQL字符类型
    /// </summary>
    public enum SqlStringType
    {
        /// <summary>
        /// 已参数化的SQL：可直接执行
        /// </summary>
        SqlParamed,
        /// <summary>
        /// 未参数化的SQL：需要参数化
        /// </summary>
        SqlNoParamed,
        /// <summary>
        /// 已参数化的SQL路径：需要从配置文件中读取SQL
        /// </summary>
        ConfigPathParamed,
        /// <summary>
        /// 未参数化的SQL路径：需要从配置文件中读取SQL
        /// </summary>
        ConfigPathNoParamed
    }
}
