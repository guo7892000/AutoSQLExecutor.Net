using Breezee.Core.Interface;
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
        public string DBName;
        public string Owner;
        public string TableSchema;
        public string TableName;
        public string TableNameUpper;//表编码的大驼峰式
        public string TableNameLower;//表编码的小驼峰式
        public string TableNameCN;
        public string TableComments;
        public string TableExtra;
        //Column
        public int SortNum;//排序号
        public string Name;
        public string NameUpper;//列编码的大驼峰式
        public string NameLower;//列编码的小驼峰式
        public string NameCN;
        public string DataType;
        public string DataLength;
        public string DataPrecision;
        public string DataScale;
        public string DataTypeFull;
        public string Default;
        public string NotNull;
        public DBColumnKeyType KeyType;
        public string Comments;
        public string Extra;

        public static DBColumnEntity GetEntity(DataRow dr)
        {
            DBColumnEntity entity = new DBColumnEntity();
            entity.DBName = dr[SqlString.DBName].ToString();
            entity.Owner = dr[SqlString.Owner].ToString();
            entity.TableSchema = dr[SqlString.TableSchema].ToString();
            entity.TableName = dr[SqlString.TableName].ToString();
            entity.TableNameUpper = dr[SqlString.TableName].ToString().FirstLetterUpper();
            entity.TableNameLower = dr[SqlString.TableName].ToString().FirstLetterUpper(false);
            entity.TableNameCN = dr[SqlString.TableNameCN].ToString();
            entity.TableComments = dr[SqlString.TableComments].ToString();
            entity.TableExtra = dr[SqlString.TableExtra].ToString();

            entity.SortNum = int.Parse(dr[SqlString.SortNum].ToString());
            entity.Name = dr[SqlString.Name].ToString();
            entity.NameUpper = dr[SqlString.Name].ToString().FirstLetterUpper();
            entity.NameLower = dr[SqlString.Name].ToString().FirstLetterUpper(false);
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
                dr[SqlString.DBName] = entity.DBName;
                dr[SqlString.Owner] = entity.Owner;
                dr[SqlString.TableSchema] = entity.TableSchema;
                dr[SqlString.TableName] = entity.TableName;
                dr[SqlString.TableNameUpper] = entity.TableName.FirstLetterUpper();
                dr[SqlString.TableNameLower] = entity.TableName.FirstLetterUpper(false);

                dr[SqlString.TableNameCN] = entity.TableNameCN;
                dr[SqlString.TableComments] = entity.TableComments;
                dr[SqlString.TableExtra] = entity.TableExtra;

                dr[SqlString.SortNum] = entity.SortNum;
                dr[SqlString.Name] = entity.Name;
                dr[SqlString.NameUpper] = entity.Name.FirstLetterUpper();
                dr[SqlString.NameLower] = entity.Name.FirstLetterUpper(false);

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
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn(SqlString.DBName),
                new DataColumn(SqlString.Owner),
                new DataColumn(SqlString.TableSchema),
                new DataColumn(SqlString.TableName),
                new DataColumn(SqlString.TableNameUpper),
                new DataColumn(SqlString.TableNameLower),
                new DataColumn(SqlString.TableNameCN),
                new DataColumn(SqlString.TableComments),
                new DataColumn(SqlString.TableExtra),

                new DataColumn(SqlString.SortNum,typeof(int)),
                new DataColumn(SqlString.Name),
                new DataColumn(SqlString.NameUpper),
                new DataColumn(SqlString.NameLower),
                new DataColumn(SqlString.DataType),
                new DataColumn(SqlString.DataLength),
                new DataColumn(SqlString.DataPrecision),
                new DataColumn(SqlString.DataScale),
                new DataColumn(SqlString.DataTypeFull),
                new DataColumn(SqlString.Default),
                new DataColumn(SqlString.NotNull),
                new DataColumn(SqlString.KeyType),
                new DataColumn(SqlString.Comments),
                new DataColumn(SqlString.NameCN),
                new DataColumn(SqlString.Extra)
        });
            dt.TableName = "DBSchemaColumns";
            return dt;
        }

        /// <summary>
        /// SQL中列名称
        /// MySql中的列名比较规范，所以以它为准
        /// </summary>
        public static class SqlString
        {
            public static string DBName = DBTableEntity.SqlString.DBName;
            public static string Owner = DBTableEntity.SqlString.Owner;
            public static string TableSchema = DBTableEntity.SqlString.Schema;
            public static string TableName = DBTableEntity.SqlString.Name;
            public static string TableNameUpper = DBTableEntity.SqlString.NameUpper;//表编码的大驼峰式UpperCamelCase
            public static string TableNameLower = DBTableEntity.SqlString.NameLower;//表编码的小驼峰式UpperCamelCase
            public static string TableNameCN = DBTableEntity.SqlString.NameCN;
            public static string TableComments = DBTableEntity.SqlString.Comments;
            public static string TableExtra = DBTableEntity.SqlString.Extra;

            public static string SortNum = "ORDINAL_POSITION";//排序号
            public static string Name = "COLUMN_NAME";//列名
            public static string NameUpper = "COLUMN_NAME_UPPER";//列编码的大驼峰式UpperCamelCase
            public static string NameLower = "COLUMN_NAME_LOWER";//列编码的小驼峰式lowerCamelCase
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
