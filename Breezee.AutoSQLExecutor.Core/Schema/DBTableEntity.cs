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
    public class DBTableEntity
    {
        public string DBName;
        public string Owner;
        public string Schema;
        public string Name;
        public string Comments;
        public string NameCN;
        public string Extra;
        public static DBTableEntity GetEntity(DataRow dr)
        {
            DBTableEntity entity = new DBTableEntity();
            entity.DBName = dr[SqlString.DBName].ToString();
            entity.Owner = dr[SqlString.Owner].ToString();
            entity.Schema = dr[SqlString.Schema].ToString();
            entity.Name = dr[SqlString.Name].ToString();
            entity.Comments = dr[SqlString.Comments].ToString();
            entity.NameCN = dr[SqlString.NameCN].ToString();
            entity.Extra = dr[SqlString.Extra].ToString();
            return entity;
        }
        public static DataTable GetTableStruct()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] { 
                new DataColumn(SqlString.DBName), 
                new DataColumn(SqlString.Owner), 
                new DataColumn(SqlString.Schema), 
                new DataColumn(SqlString.Name),
                new DataColumn(SqlString.Comments),
                new DataColumn(SqlString.NameCN),
                new DataColumn(SqlString.Extra) 
            });
            dt.TableName = "DBSchemaTables";
            return dt;
        }

        /// <summary>
        /// SQL中列名称
        /// MySql中的列名比较规范，所以以它为准
        /// </summary>
        public static class SqlString
        {
            public static string DBName = "DATABASE_NAME";//数据库名
            public static string Owner = "TABLE_OWNER";//所有者
            public static string Schema = "TABLE_SCHEMA";//架构，如SqlServer的dbo
            public static string Name = "TABLE_NAME";
            public static string Comments = "TABLE_COMMENT";
            public static string NameCN = "TABLE_NAME_CN";//中文名称
            public static string Extra = "TABLE_EXTRA";//额外信息
        }
    }
}
