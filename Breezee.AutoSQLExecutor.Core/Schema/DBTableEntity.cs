using Breezee.Core.Interface;
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
        public string TableNameUpper;//表编码的大驼峰式
        public string TableNameLower;//表编码的小驼峰式
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
            dr[SqlString.NameUpper] = entity.Name.FirstLetterUpper();
            dr[SqlString.NameLower] = entity.Name.FirstLetterUpper(false);
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
                new DataColumn(SqlString.NameUpper),
                new DataColumn(SqlString.NameLower),
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
            public static string NameUpper = "TABLE_NAME_UPPER";//表编码的大驼峰式UpperCamelCase
            public static string NameLower = "TABLE_NAME_LOWER";//表编码的小驼峰式lowerCamelCase
        }
    }
}
