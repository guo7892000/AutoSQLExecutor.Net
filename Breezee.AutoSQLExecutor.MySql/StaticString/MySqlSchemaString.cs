using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.MySql
{
    /// <summary>
    /// 获取列架构（Columns）信息得到的表中所有列信息
    /// </summary>
    public class MySqlSchemaString
    {
        public static class DataBase
        {
            public static readonly string Name = "DATABASE_NAME";
        }
        public static class Table
        {
            public static readonly string TableSchema = "TABLE_SCHEMA";
            public static readonly string TableName = "TABLE_NAME";
            public static readonly string TableComment = "TABLE_COMMENT";
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
            public static readonly string ColumnType = "COLUMN_TYPE";
            public static readonly string KeyType = "COLUMN_KEY";
            public static readonly string ColumnComment = "COLUMN_COMMENT";
        }
    }
}
