using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 数据库列实体
    /// </summary>
    public class DBColumnEntity
    {
        //Table
        public string TableSchema;
        public string TableName;
        //Column
        public int SortNum;//排序号
        public string Name;
        public string DataType;
        public string DataLength;
        public string DataPrecision;
        public string DataScale;
        public string DataTypeFull;
        public string Default;
        public string NotNull;
        public DBColumnKeyType KeyType;
        public string Comments;
        public string NameCN;
        public string Extra;

        public static DBColumnEntity GetEntity(DataRow dr)
        {
            DBColumnEntity entity = new DBColumnEntity();
            entity.TableSchema = dr[SqlString.TableSchema].ToString();
            entity.TableName = dr[SqlString.TableName].ToString();
            entity.SortNum = int.Parse(dr[SqlString.SortNum].ToString());
            entity.Name = dr[SqlString.Name].ToString();
            entity.DataType = dr[SqlString.DataType].ToString();
            entity.DataLength = dr[SqlString.DataLength].ToString();
            entity.DataPrecision = dr[SqlString.DataPrecision].ToString();
            entity.DataScale = dr[SqlString.DataScale].ToString();
            entity.DataTypeFull = dr[SqlString.DataTypeFull].ToString();
            entity.Default = dr[SqlString.Default].ToString();
            entity.NotNull = dr[SqlString.NotNull].ToString();
            if(!string.IsNullOrEmpty(dr[SqlString.KeyType].ToString()))
            {
                entity.KeyType = dr[SqlString.KeyType].ToString().ToUpper() == "PK" ? DBColumnKeyType.PK : DBColumnKeyType.FK;
            }
            else
            {
                entity.KeyType = DBColumnKeyType.None;
            }
            entity.Comments = dr[SqlString.Comments].ToString();
            entity.NameCN = dr[SqlString.NameCN].ToString();
            entity.Extra = dr[SqlString.Extra].ToString();
            return entity;
        }

        public static DataTable GetDataRow(List<DBColumnEntity> entityList)
        {
            DataTable dt = GetTableStruct();
            foreach (var entity in entityList)
            {
                DataRow dr = dt.NewRow();
                dr[SqlString.TableSchema] = entity.TableSchema;
                dr[SqlString.TableName] = entity.TableName;
                dr[SqlString.SortNum] = entity.SortNum;
                dr[SqlString.Name] = entity.Name;
                dr[SqlString.DataType] = entity.DataType;
                dr[SqlString.DataLength] = entity.DataLength;
                dr[SqlString.DataPrecision] = entity.DataPrecision;
                dr[SqlString.DataScale] = entity.DataScale;
                dr[SqlString.DataTypeFull] = entity.DataTypeFull;
                dr[SqlString.Default] = entity.Default;
                dr[SqlString.NotNull] = entity.NotNull;
                dr[SqlString.KeyType] = entity.KeyType;
                dr[SqlString.Comments] = entity.Comments;
                dr[SqlString.NameCN] = entity.NameCN;
                dr[SqlString.Extra] = entity.Extra;

                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static DataTable GetTableStruct()
        {
            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn(SqlString.TableSchema);
            DataColumn dc1 = new DataColumn(SqlString.TableName);
            DataColumn dc2 = new DataColumn(SqlString.SortNum,typeof(int));
            DataColumn dc3 = new DataColumn(SqlString.Name);
            DataColumn dc4 = new DataColumn(SqlString.DataType);
            DataColumn dc5 = new DataColumn(SqlString.DataLength);
            DataColumn dc6 = new DataColumn(SqlString.DataPrecision);
            DataColumn dc7 = new DataColumn(SqlString.DataScale);
            DataColumn dc8 = new DataColumn(SqlString.DataTypeFull);
            DataColumn dc9 = new DataColumn(SqlString.Default);
            DataColumn dc10 = new DataColumn(SqlString.NotNull);
            DataColumn dc11 = new DataColumn(SqlString.KeyType);
            DataColumn dc12 = new DataColumn(SqlString.Comments);
            DataColumn dc13 = new DataColumn(SqlString.NameCN);
            DataColumn dc14 = new DataColumn(SqlString.Extra);
            dt.Columns.AddRange(new DataColumn[] { dc,dc1, dc2, dc3, dc4,dc5,dc6,dc7,dc8,dc9,dc10,dc11,dc12, dc13, dc14 });
            dt.TableName = "DBSchemaColumns";
            return dt;
        }

        /// <summary>
        /// SQL中列名称
        /// MySql中的列名比较规范，所以以它为准
        /// </summary>
        public static class SqlString
        {
            public static string TableSchema = "TABLE_SCHEMA"; 
            public static string TableName = "TABLE_NAME";

            public static string SortNum = "ORDINAL_POSITION";//排序号
            public static string Name = "COLUMN_NAME";//列名
            public static string DataType = "DATA_TYPE";//列类型（不含长度或精度）
            public static string DataLength = "CHARACTER_MAXIMUM_LENGTH";//长度
            public static string DataPrecision = "NUMERIC_PRECISION";//精度
            public static string DataScale = "NUMERIC_SCALE";//尺度，数值范围
            public static string DataTypeFull = "COLUMN_TYPE";//类型全称，如decimail(14,4)
            public static string Default = "COLUMN_DEFAULT";//默认值
            public static string NotNull = "NOTNULL";//非空
            public static string KeyType = "COLUMN_KEY";//主外键：PK主键，FK外键
            public static string Comments = "COLUMN_COMMENT";//包括列名称和额外信息
            public static string NameCN = "COLUMN_CN";//列中文名称：从列备注中拆分
            public static string Extra = "COLUMN_EXTRA";//列额外信息：从列备注中拆分

        }
    }
}
