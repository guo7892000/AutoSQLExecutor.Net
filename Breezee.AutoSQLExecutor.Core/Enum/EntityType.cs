using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    public enum EntityType
    {
        /// <summary>
        /// 表
        /// </summary>
        Table,
        /// <summary>
        /// 视图
        /// </summary>
        View,
        /// <summary>
        /// 存储过程
        /// </summary>
        Produce,
        /// <summary>
        /// 自定义视图
        /// </summary>
        CustomView,
        /// <summary>
        /// 其他
        /// </summary>
        Other
    }
}
