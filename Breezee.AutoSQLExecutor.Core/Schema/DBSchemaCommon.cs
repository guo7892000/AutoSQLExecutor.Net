using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    public class DBSchemaCommon
    {
        public static void SetComment(DataRow dr, string sComment,bool isTable=true)
        {
            if (string.IsNullOrEmpty(sComment)) return;
            string[] arr = sComment.Split(new char[] { ':', '：' });
            if (isTable)
            {
                if (dr.Table.Columns.Contains(DBTableEntity.SqlString.Comments))
                {
                    dr[DBTableEntity.SqlString.Comments] = sComment;
                    dr[DBTableEntity.SqlString.NameCN] = arr[0];
                    if (arr.Length > 1 && dr.Table.Columns.Contains(DBColumnEntity.SqlString.Extra))
                    {
                        dr[DBTableEntity.SqlString.Extra] = arr[1];
                    }
                }
            }
            else
            {
                if (dr.Table.Columns.Contains(DBColumnEntity.SqlString.Comments))
                {
                    dr[DBColumnEntity.SqlString.Comments] = sComment;
                    dr[DBColumnEntity.SqlString.NameCN] = arr[0];
                    if (arr.Length > 1 && dr.Table.Columns.Contains(DBColumnEntity.SqlString.Extra))
                    {
                        dr[DBColumnEntity.SqlString.Extra] = arr[1];
                    }

                }
            }            
        }



        public static String GetFullType(DataRow dr)
        {
            string sFullType = dr[DBColumnEntity.SqlString.DataTypeFull].ToString();
            if (string.IsNullOrEmpty(sFullType)) return sFullType;

            string sDataType = dr[DBColumnEntity.SqlString.DataType].ToString().ToLower();
            string sLen = dr[DBColumnEntity.SqlString.DataLength].ToString().ToLower();
            string sPrecision = dr[DBColumnEntity.SqlString.DataPrecision].ToString().ToLower();
            string sScale = dr[DBColumnEntity.SqlString.DataScale].ToString().ToLower();

            //if (sDataType.Equals("date") || sDataType.Equals("datetime") || sDataType.Equals("date") 
            //    || sDataType.Equals("date") || sDataType.Equals("date") || sDataType.Equals("date") 
            //    || sDataType.Equals("date"))


            return sFullType;
        }

        //public static bool IsLengthType(string sDataType)
        //{
        //    List<string> list = new List<string>();
            
        //    if (sDataType.Equals("varchar") || sDataType.Equals("nvarchar") || sDataType.Equals("varchar2")
        //        || sDataType.Equals("date") || sDataType.Equals("date") || sDataType.Equals("date")
        //        || sDataType.Equals("date"))
        //    {
        //        return true;
        //    }
        //}

        //public static bool IsPrecision(string sDataType)
        //{
        //    if (sDataType.Equals("decimal") || sDataType.Equals("number") || sDataType.Equals("numeric")
        //        || sDataType.Equals("date") || sDataType.Equals("date") || sDataType.Equals("date")
        //        || sDataType.Equals("date"))
        //    {
        //        return true;
        //    }
        //}
    }
}
