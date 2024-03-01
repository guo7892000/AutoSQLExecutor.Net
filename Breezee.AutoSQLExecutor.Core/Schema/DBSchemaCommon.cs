using org.breezee.MyPeachNet;
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
            char[] charSplits = new char[] { ':', '：', ' ', '\r' };
            string[] arr = sComment.Split(charSplits);
            if (isTable)
            {
                if (dr.Table.Columns.Contains(DBTableEntity.SqlString.Comments))
                {
                    dr[DBTableEntity.SqlString.Comments] = sComment.TrimEnd(charSplits);
                    dr[DBTableEntity.SqlString.NameCN] = arr[0].Trim();
                    if (arr.Length > 1)
                    {
                        if (dr.Table.Columns.Contains(DBTableEntity.SqlString.Extra))
                        {
                            //从第二个数组字符开始截取
                            string sNext = arr[1];
                            if (string.IsNullOrEmpty(sNext))
                            {
                                dr[DBTableEntity.SqlString.Extra] = sNext;
                            }
                            else
                            {
                                int iExtStart = sComment.IndexOf(sNext);
                                dr[DBTableEntity.SqlString.Extra] = sComment.Substring(iExtStart).Trim();
                            }
                        }
                    }
                }
            }
            else
            {
                if (dr.Table.Columns.Contains(DBColumnEntity.SqlString.Comments))
                {
                    dr[DBColumnEntity.SqlString.Comments] = sComment.TrimEnd(charSplits);
                    dr[DBColumnEntity.SqlString.NameCN] = arr[0].Trim();
                    if (arr.Length > 1)
                    {
                        if (dr.Table.Columns.Contains(DBColumnEntity.SqlString.Extra))
                        {
                            //从第二个数组字符开始截取
                            string sNext = arr[1];
                            if (string.IsNullOrEmpty(sNext))
                            {
                                dr[DBColumnEntity.SqlString.Extra] = sNext;
                            }
                            else
                            {
                                int iExtStart = sComment.IndexOf(sNext);
                                dr[DBColumnEntity.SqlString.Extra] = sComment.Substring(iExtStart).Trim();
                            }
                        }
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
