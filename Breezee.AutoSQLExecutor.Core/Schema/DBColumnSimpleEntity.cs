using Breezee.Core.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 列命名简化的数据库列实体
    /// 为了方便字符拼接功能
    /// </summary>
    public class DBColumnSimpleEntity
    {
        //Table
        public string TableName;//表编码
        public string TableNameCN;
        public string TableNameUpper;//表编码的大驼峰式
        public string TableNameLower;//表编码的小驼峰式
        public string TableSchema;
        //Column
        public string Name;//列名
        public string NameCN;//列中文名称：从列备注中拆分
        public string NameUpper;//列编码的大驼峰式
        public string NameLower;//列编码的小驼峰式

        public string DataType;//列类型（不含长度或精度）
        public string DataLength;//长度
        public string DataPrecision;//精度
        public string DataScale;//尺度，数值范围
        public string DataTypeFull;//类型全称，如decimail(14,4)

        public string Default;//默认值
        public string NotNull;//非空
        public DBColumnKeyType KeyType;//主外键：PK主键，FK外键
        public int SortNum;//排序号

        public string Comments;//包括列名称和额外信息
        public string Extra;//列额外信息：从列备注中拆分

        private static IDictionary<string, string> relCol = new Dictionary<string, string>();

        public static IDictionary<string, string> RelCol { get => GetDic(); }

        public static IDictionary<string, string> GetDic() 
        {
            if (relCol.Keys.Count() == 0)
            {
                relCol[SqlString.TableName] = DBColumnEntity.SqlString.TableName;
                relCol[SqlString.TableNameCN] = DBColumnEntity.SqlString.TableSchema;
                relCol[SqlString.TableNameUpper] = DBColumnEntity.SqlString.TableName;
                relCol[SqlString.TableNameLower] = DBColumnEntity.SqlString.TableName;
                relCol[SqlString.TableSchema] = DBColumnEntity.SqlString.TableSchema;

                relCol[SqlString.Name] = DBColumnEntity.SqlString.Name;
                relCol[SqlString.NameCN] = DBColumnEntity.SqlString.NameCN;
                relCol[SqlString.NameUpper] = DBColumnEntity.SqlString.Name;
                relCol[SqlString.NameLower] = DBColumnEntity.SqlString.Name;

                relCol[SqlString.DataType] = DBColumnEntity.SqlString.DataType;
                relCol[SqlString.DataLength] = DBColumnEntity.SqlString.DataLength;
                relCol[SqlString.DataPrecision] = DBColumnEntity.SqlString.DataPrecision;
                relCol[SqlString.DataScale] = DBColumnEntity.SqlString.DataScale;
                relCol[SqlString.DataTypeFull] = DBColumnEntity.SqlString.DataTypeFull;

                relCol[SqlString.Default] = DBColumnEntity.SqlString.Default;
                relCol[SqlString.NotNull] = DBColumnEntity.SqlString.NotNull;
                relCol[SqlString.KeyType] = DBColumnEntity.SqlString.KeyType;
                relCol[SqlString.SortNum] = DBColumnEntity.SqlString.SortNum;
                relCol[SqlString.Comments] = DBColumnEntity.SqlString.Comments;
                relCol[SqlString.Extra] = DBColumnEntity.SqlString.Extra;
            }
            return relCol;
        }
        public static DBColumnSimpleEntity GetEntity(DataRow dr)
        {
            DBColumnSimpleEntity entity = new DBColumnSimpleEntity();
            entity.TableSchema = dr[DBColumnEntity.SqlString.TableSchema].ToString();
            entity.TableName = dr[DBColumnEntity.SqlString.TableName].ToString();
            entity.SortNum = int.Parse(dr[DBColumnEntity.SqlString.SortNum].ToString());
            entity.Name = dr[DBColumnEntity.SqlString.Name].ToString();
            entity.DataType = dr[DBColumnEntity.SqlString.DataType].ToString();
            entity.DataLength = dr[DBColumnEntity.SqlString.DataLength].ToString();
            entity.DataPrecision = dr[DBColumnEntity.SqlString.DataPrecision].ToString();
            entity.DataScale = dr[DBColumnEntity.SqlString.DataScale].ToString();
            entity.DataTypeFull = dr[DBColumnEntity.SqlString.DataTypeFull].ToString();
            entity.Default = dr[DBColumnEntity.SqlString.Default].ToString();
            entity.NotNull = dr[DBColumnEntity.SqlString.NotNull].ToString();
            if(!string.IsNullOrEmpty(dr[DBColumnEntity.SqlString.KeyType].ToString()))
            {
                entity.KeyType = dr[DBColumnEntity.SqlString.KeyType].ToString().ToUpper() == "PK" ? DBColumnKeyType.PK : DBColumnKeyType.FK;
            }
            else
            {
                entity.KeyType = DBColumnKeyType.None;
            }
            entity.Comments = dr[DBColumnEntity.SqlString.Comments].ToString();
            entity.NameCN = dr[DBColumnEntity.SqlString.NameCN].ToString();
            entity.Extra = dr[DBColumnEntity.SqlString.Extra].ToString();
            return entity;
        }

        public static DataTable GetDataRow(List<DBColumnEntity> entityList)
        {
            DataTable dt = GetTableStruct();
            foreach (var entity in entityList)
            {
                DataRow dr = dt.NewRow();
                //表相关
                dr[SqlString.TableName] = entity.TableName;
                dr[SqlString.TableNameCN] = entity.TableSchema;
                dr[SqlString.TableNameUpper] = entity.TableName.FirstLetterUpper();
                dr[SqlString.TableNameLower] = entity.TableSchema.FirstLetterUpper(false);
                dr[SqlString.TableSchema] = entity.TableSchema;
                //列相关
                dr[SqlString.Name] = entity.Name;
                dr[SqlString.NameCN] = entity.NameCN;
                dr[SqlString.NameUpper] = entity.Name.FirstLetterUpper();
                dr[SqlString.NameLower] = entity.Name.FirstLetterUpper(false);

                dr[SqlString.DataType] = entity.DataType;
                dr[SqlString.DataLength] = entity.DataLength;
                dr[SqlString.DataPrecision] = entity.DataPrecision;
                dr[SqlString.DataScale] = entity.DataScale;
                dr[SqlString.DataTypeFull] = entity.DataTypeFull;

                dr[SqlString.SortNum] = entity.SortNum;
                dr[SqlString.Default] = entity.Default;
                dr[SqlString.NotNull] = entity.NotNull;
                dr[SqlString.KeyType] = entity.KeyType;

                dr[SqlString.Comments] = entity.Comments;  
                dr[SqlString.Extra] = entity.Extra;

                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static DataTable GetTableStruct()
        {
            DataTable dt = new DataTable();
            dt.TableName = "SimpleColumns";
            foreach (string sKey in RelCol.Keys)
            {
                DataColumn dc = new DataColumn(sKey);
                dt.Columns.Add(dc);
            }
            return dt;
        }

        public static class SqlString
        {
            public static string TableName = "T";//表编码
            public static string TableNameCN = "T1";
            public static string TableNameUpper = "T2";//表编码的大驼峰式
            public static string TableNameLower = "T3";//表编码的小驼峰式
            public static string TableSchema = "T9";

            public static string Name = "C";//列名
            public static string NameCN = "C1";//列中文名称：从列备注中拆分
            public static string NameUpper = "C2";//列编码的大驼峰式
            public static string NameLower = "C3";//列编码的小驼峰式
            
            public static string DataType = "D";//列类型（不含长度或精度）
            public static string DataLength = "D1";//长度
            public static string DataPrecision = "D2";//精度
            public static string DataScale = "D3";//尺度，数值范围
            public static string DataTypeFull = "D9";//类型全称，如decimail(14,4)

            public static string Default = "F";//默认值
            public static string NotNull = "N";//非空
            public static string KeyType = "K";//主外键：PK主键，FK外键
            public static string SortNum = "S";//排序号

            public static string Comments = "R";//包括列名称和额外信息
            public static string Extra = "R1";//列额外信息：从列备注中拆分

        }
    }
}
