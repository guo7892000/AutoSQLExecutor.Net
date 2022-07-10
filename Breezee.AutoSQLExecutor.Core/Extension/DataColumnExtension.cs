using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    public static class DataColumnExtension
    {
        /// <summary>
        /// 设置表中列的扩展属性
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="defaultType"></param>
        /// <returns></returns>
        public static DataColumn ExtProp(this DataColumn dc, TableCoulnmDefaultType defaultType)
        {
            dc.ExtendedProperties[StaticConstant.FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE] = defaultType;
            return dc;
        }
    }
}
