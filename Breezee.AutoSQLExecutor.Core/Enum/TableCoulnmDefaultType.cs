using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*********************************************************
 * 对象名称：表默认值类型
 * 对象类别：枚举
 * 创建作者：黄国辉
 * 创建日期：2014-7-25
 * 对象说明：主要提供表默认值类型
 * 修改历史：
 *      V1.0 新建 hgh 2014-7-25
 * ******************************************************/
namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 名称：表默认值类型
    /// 作者：黄国辉
    /// 日期：2013-11-10
    /// 用法：dt.Columns["CREATE_TIME"].ExtendedProperties[StaticConstant.FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE]=TableCoulnmDefaultType.DateTime
    ///     在调用IDataAccess.UpdateTable方法时，会针对这个列取相应的动态固定值，而不是取原表中的列值。
    /// </summary>
    public enum TableCoulnmDefaultType
    {
        /// <summary>
        /// 时间格式
        /// </summary>
        DateTime,

        /// <summary>
        /// 时间戳格式
        /// </summary>
        TimeStamp,

        /// <summary>
        /// GUID格式
        /// </summary>
        Guid
    }
}
