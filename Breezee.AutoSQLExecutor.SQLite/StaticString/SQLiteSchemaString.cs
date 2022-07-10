using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.SQLite
{
    /// <summary>
    /// 获取列架构（Columns）信息得到的表中所有列信息
    /// </summary>
    public class SQLiteSchemaString
    {
        public static class DataBase
        {
            public static readonly string Name = "name";//一般是main，但可以附加更多数据库
        }
        public static class Table
        {
            public static readonly string TableSchema = "TABLE_SCHEMA";
            public static readonly string TableName = "TABLE_NAME";
            public static readonly string TableCatalog = "TABLE_CATALOG";

        }
        public static class Column
        {
            public static readonly string TableSchema = "TABLE_SCHEMA";
            public static readonly string TableName = "TABLE_NAME";
            public static readonly string ColumnName = "COLUMN_NAME";
            public static readonly string OrdinalPosition = "ORDINAL_POSITION";
            public static readonly string ColumnDefault = "COLUMN_DEFAULT";
            public static readonly string IsNullable = "IS_NULLABLE";
            public static readonly string DataType = "DATA_TYPE";
            public static readonly string CharacterMaximumLength = "CHARACTER_MAXIMUM_LENGTH";
            public static readonly string Numeric_Precision = "NUMERIC_PRECISION";
            public static readonly string Numeric_Scale = "NUMERIC_SCALE";
            //独有的字段
            public static readonly string PrimaryKey = "PRIMARY_KEY";
            public static readonly string AutoIncrement = "AUTOINCREMENT";
            public static readonly string Unique = "UNIQUE";
            public static readonly string EdmType = "EDM_TYPE";
        }
    }
}
