using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 百分号格式，默认Both
    /// </summary>
    public enum PercentSignStyle
    {
        /// <summary>
        /// 左右显示，如 %word%
        /// </summary>
        Both,

        /// <summary>
        /// 在左，如 %word
        /// </summary>
        Left,

        /// <summary>
        /// 在右，如 word%
        /// </summary>
        Right,

        /// <summary>
        /// 无
        /// </summary>
        None
    }
}
