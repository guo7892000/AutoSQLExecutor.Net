using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Oracle
{
    /// <summary>
    /// 获取列架构（Columns）信息得到的表中所有列信息
    /// </summary>
    public class OracleSchemaString
    {
        public static class DataBase
        {
            public static readonly string Name = "NAME";//实际上是用户，不是数据库
        }
        public static class Table
        {
            public static readonly string Owner = "OWNER";
            public static readonly string TableName = "TABLE_NAME";

        }
        public static class Column
        {
            public static readonly string TableSchema = "OWNER";
            public static readonly string TableName = "TABLE_NAME";
            public static readonly string ColumnName = "COLUMN_NAME";
            public static readonly string OrdinalPosition = "id";
            //public static readonly string ColumnDefault = "COLUMN_DEFAULT";
            public static readonly string IsNullable = "NULLABLE";
            public static readonly string DataType = "DATATYPE";
            public static readonly string CharacterMaximumLength = "LENGTH";
            public static readonly string Numeric_Precision = "PRECISION";
            public static readonly string Numeric_Scale = "SCALE";
        }
    } 
}
