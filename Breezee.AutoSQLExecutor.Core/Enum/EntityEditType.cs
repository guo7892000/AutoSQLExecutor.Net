using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    public enum EntityEditType
    {
        /// <summary>
        /// 新增
        /// </summary>
        Add,
        /// <summary>
        /// 更新
        /// </summary>
        Update,
        /// <summary>
        /// 物理删除
        /// </summary>
        Delete,
        /// <summary>
        /// 逻辑删除
        /// </summary>
        Disable,
        /// <summary>
        /// 其他
        /// </summary>
        Other
    }
}
