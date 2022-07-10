using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 数据库表实体
    /// </summary>
    public class DBDataBaseEntity
    {
        public string Name;
        public static DBDataBaseEntity GetEntity(DataRow dr)
        {
            DBDataBaseEntity entity = new DBDataBaseEntity();
            entity.Name = dr[SqlString.Name].ToString();
            return entity;
        }
        public static DataTable GetTableStruct()
        {
            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn(SqlString.Name);
            dt.Columns.AddRange(new DataColumn[] { dc1 });
            dt.TableName = "DataBases";
            return dt;
        }

        /// <summary>
        /// SQL中列名称
        /// </summary>
        public static class SqlString
        {
            public static string Name = "DATABASE_NAME";//数据库名
        }
    }
}
