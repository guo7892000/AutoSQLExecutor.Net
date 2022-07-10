using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Collections;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// DataTable扩展方法
    /// </summary>
    public static class DataTableExtension
    {
        #region 复制表数据
        /// <summary>
        /// 复制表数据
        /// </summary>
        /// <remark>
        /// 用于明细表的保存
        /// </remark>
        /// <author>黄国辉</author>
        /// <param name="dtWrite">写入数据的表</param>
        /// <param name="dtSource">读取的源表</param>
        /// <returns></returns>
        /// <example>
        /// </example>
        public static DataTable CopyExistColumnData(this DataTable dtWrite, DataTable dtSource)
        {
            foreach (DataRow dr in dtSource.Rows)
            {
                DataRow drNew = dtWrite.NewRow();
                for (int i = 0; i < dtWrite.Columns.Count; i++)
                {
                    string columnName = dtWrite.Columns[i].ColumnName;
                    if (dtSource.Columns.Contains(columnName))
                    {
                        drNew[columnName] = dr[columnName];
                    }
                }
                dtWrite.Rows.Add(drNew);
            }
            return dtWrite;
        }
        #endregion

        #region 返回过滤条件后的表
        /// <summary>
        /// 返回过滤条件后的表
        /// </summary>
        /// <param name="dtIn">输入表</param>
        /// <param name="strFilter">过滤条件</param>
        /// <param name="isHaveEnptyAll">是否含空白行</param>
        /// <returns></returns>
        public static DataTable Filter(this DataTable dtIn, string strFilter, bool isHaveEnptyAll)
        {
            DataRow[] drArr = dtIn.Select(strFilter);
            DataTable dtReturn = dtIn.Clone();
            foreach (DataRow dr in drArr)
            {
                dtReturn.ImportRow(dr);
            }
            if (isHaveEnptyAll)
            {
                dtReturn.Rows.InsertAt(dtReturn.NewRow(), 0);
            }
            return dtReturn;
        }
        #endregion

        #region 将表转换为字典列表
        /// <summary>
        /// 将DataTable 转换为IList&lt;IDictionary&lt;string, string&gt;&gt;
        /// </summary>
        /// <remark>
        /// 用于明细表的保存
        /// </remark>
        /// <author>黄国辉</author>
        /// <param name="dt">传入需要构造字典LIST的表</param>
        /// <returns></returns>
        /// <example>
        /// DataTable dtInDetail = new DataTable();
        /// IList&lt;IDictionary&lt;string, string&gt;&gt; listDic = dtInDetail.ToListDic();
        /// </example>
        public static IList<IDictionary<string, string>> ToListDic(this DataTable dt)
        {
            IList<IDictionary<string, string>> listDic = new List<IDictionary<string, string>>();
            string columnName = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                IDictionary<string, string> dicNew = new Dictionary<string, string>();
                //变量赋值
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    columnName = dt.Columns[i].ColumnName;
                    dicNew["#" + columnName + "#"] = dr[columnName].ToString();
                }
                listDic.Add(dicNew);
            }
            return listDic;
        } 
        #endregion

        #region 将表和字典中值转换为字典列表
        /// <summary>
        /// 将DataTable + IDictionary&lt;string,string&gt;转换为IList&lt;IDictionary&lt;string, string&gt;&gt;
        /// </summary>
        /// <remark>
        /// 非扩展方法
        /// 用于明细表的保存
        /// </remark>
        /// <author>黄国辉</author>
        /// <param name="dt">传入需要构造字典LIST的表</param>
        /// <param name="dicPub">传入的字典,需要添加到每个新字典中的项，例如UER_ID</param>
        /// <returns>
        /// </returns>
        /// <example>
        /// DataTable dtInDetail = new DataTable();
        /// IList&lt;IDictionary&lt;string, string&gt;&gt; listDic = dtInDetail.ToListDic(dicParam);
        /// </example>
        public static IList<IDictionary<string, string>> ToListDic(this DataTable dt, IDictionary<string, string> dicPub)
        {
            IList<IDictionary<string, string>> listDic = new List<IDictionary<string, string>>();
            string columnName = string.Empty;

            foreach (DataRow dr in dt.Rows)
            {
                IDictionary<string, string> dicNew = new Dictionary<string, string>();
                //变量赋值
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    columnName = dt.Columns[i].ColumnName;
                    dicNew["#" + columnName + "#"] = dr[columnName].ToString();
                }
                if (dicPub != null && dicPub.Count > 0)
                {
                    foreach (KeyValuePair<string, string> item in dicPub)
                    {
                        dicNew[item.Key] = item.Value;
                    }
                }
                listDic.Add(dicNew);
            }
            return listDic;
        } 
        #endregion

        #region 将DataTable转换为IDictionary<string, string>型的字典
        /// <summary>
        /// 将DataTable转换为IDictionary&lt;string, string&gt;型的字典
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="KeyColName">KEY值列名</param>
        /// <param name="ValueColName">VALUE值列名</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDataMap(this DataTable dt, string KeyColName, string ValueColName)
        {
            Dictionary<string, string> dicMap = new Dictionary<string, string>();
            dicMap[""] = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    dicMap[dr[KeyColName].ToString()] = dr[ValueColName].ToString();
                }
            }
            return dicMap;
        }
        #endregion

        #region 为DataTable排序
        /// <summary>
        /// 为DataTable排序（注意：排序后的DataTable是并非原DataTable，而是新创建的）
        /// </summary>
        /// <author>黄国辉</author>
        /// <param name="dt">DataTable数据集</param>
        /// <param name="sortFiles">要排序列的字符串，它包含列名，后跟“ASC”（升序）或“DESC”（降序）。在默认情况下列按升序排序。多个列可用逗号隔开。</param>
        /// <remarks>
        /// 样例1：dtOrder.Sort("列1"); //按列1升序排列
        /// 样例2：dtOrder.Sort("列1 DESC"); //按列1降序排列
        /// 样例3：dtOrder.Sort("列1,列2 DESC"); //按列1和列2降序排列
        /// 样例4：dtOrder.Sort("列1,列2 DESC,列3 ASC"); //按列1和列2降序排列，按列3升序排列
        /// </remarks>
        /// <returns>排序后的DataTable</returns>
        public static DataTable Sort(this DataTable dt, string sortFiles)
        {
            DataView dv = dt.DefaultView;
            dv.Sort = sortFiles;
            return dv.ToTable();
        } 
        #endregion

        #region 设置列默认值类型
        public static void SetColumnDefaultType(this DataTable dt, string strColumnName, TableCoulnmDefaultType defaultType)
        {
            if (dt.Columns.Contains(strColumnName))
            {
                dt.Columns[strColumnName].ExtendedProperties[StaticConstant.FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE] = TableCoulnmDefaultType.DateTime;
            }
            else
            {
                throw new Exception(string.Format("表没有名为{0}的列！", strColumnName));
            }
        } 
        #endregion

        #region DataRow扩展

        #region 将DataRow[]转换为IDictionary<string, string>型的字典
        /// <summary>
        /// 将DataRow[]转换为IDictionary&lt;string, string&gt;型的字典
        /// </summary>
        /// <param name="drTmp"></param>
        /// <param name="KeyColName">KEY值列名</param>
        /// <param name="ValueColName">VALUE值列名</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDataMap(this DataRow[] drTmp, string KeyColName, string ValueColName)
        {
            Dictionary<string, string> dicMap = new Dictionary<string, string>();
            dicMap[""] = "";
            if (drTmp != null && drTmp.Length > 0)
            {
                foreach (DataRow dr in drTmp)
                {
                    dicMap[dr[KeyColName].ToString()] = dr[ValueColName].ToString();
                }
            }
            return dicMap;
        }
        #endregion

        #region 判断DataRow是否包含某列
        /// <summary>
        /// 判断DataRow是否包含某列
        /// </summary>
        /// <author>黄国辉</author>
        /// <param name="row">数据列</param>
        /// <param name="columnName">列名</param>
        /// <returns>存在则返回true，否则false</returns>
        public static bool ContainsColumn(this DataRow row, string columnName)
        {
            DataTable dt = row.Table;
            return dt.Columns.Contains(columnName);
        }
        #endregion

        #region 将DataRow转换为字典
        /// <summary>
        /// 将DataRow的转换为IDictionary&lt;string, string&gt;型的字典（dic["#Key#"] = Value）
        /// </summary>
        /// <param name="row">要转换的数据行</param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDict(this DataRow row)
        {
            DataTable dt = row.Table;
            IDictionary<string, string> dic = new Dictionary<string, string>();
            string columnName = string.Empty;

            //变量赋值
            foreach (DataColumn column in dt.Columns)
            {
                columnName = column.ColumnName;
                dic["#" + columnName + "#"] = row[columnName].ToString();
            }
            return dic;
        }
        #endregion

        #region 将DataRow和字典转换为字典
        /// <summary>
        /// 将DataRow的转换为IDictionary&lt;string, string&gt;型的字典（dic["#Key#"] = Value）
        /// </summary>
        /// <param name="row">要转换的数据行</param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDict(this DataRow row, IDictionary<string, string> dicPub)
        {
            DataTable dt = row.Table;
            IDictionary<string, string> dic = new Dictionary<string, string>();
            string columnName = string.Empty;

            if (dicPub != null && dicPub.Count > 0)
            {
                foreach (KeyValuePair<string, string> item in dicPub)
                {
                    dic[item.Key] = item.Value;
                }
            }

            //变量赋值
            foreach (DataColumn column in dt.Columns)
            {
                columnName = column.ColumnName;
                dic["#" + columnName + "#"] = row[columnName].ToString();
            }

            return dic;
        }
        #endregion 
        #endregion

        #region 移除空行方法
        public static void DeleteNullRow(this DataTable dtMain)
        {
            string strColNameList = "";
            for (int k = 0; k < dtMain.Columns.Count; k++)
            {
                string strOneDataCol = "([" + dtMain.Columns[k].ColumnName + "] is null ) ";
                if (k != 0)
                {
                    strColNameList = strColNameList + "and " + strOneDataCol;
                }
                else
                {
                    strColNameList = strColNameList + strOneDataCol;
                }

            }
            foreach (DataRow dr in dtMain.Select(strColNameList))
            {
                dtMain.Rows.Remove(dr);
            }
        }
        #endregion

        #region 将DataTable转化成Txt文档
        /// <summary>
        /// 将DataTable转化成Txt文档
        /// </summary>
        /// <param name="inputDataTable">需要转化的DataTable</param>
        /// <param name="filePath">文件保存的路径</param>
        /// <param name="splitFlag">文件分割符 默认为','</param>
        /// <param name="dicCol">过滤的DataTable中列</param>
        public static bool DataTableConvertTxt(this DataTable inputDataTable, string filePath, char splitFlag = ',', Dictionary<string, string> dicCol = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("文件路径不能为空!");
            }
            if (inputDataTable == null || inputDataTable.Rows.Count < 1)
            {
                throw new Exception("导入的数据不能为空!");
            }
            StreamWriter sw = new StreamWriter(filePath);
            try
            {
                foreach (DataRow dr in inputDataTable.Rows)
                {
                    StringBuilder builder = new StringBuilder();
                    if (dicCol == null)
                    {
                        for (int i = 0; i < inputDataTable.Columns.Count; i++)
                        {
                            builder.Append(dr[i].ToString() + splitFlag);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, string> q in dicCol)
                        {
                            builder.Append(dr[q.Key].ToString() + splitFlag);
                        }
                    }
                    sw.WriteLine(builder.ToString().TrimEnd(splitFlag));
                    sw.Flush();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sw.Close();
            }
        }
        #endregion

        #region 查找一个根节点的所有子节点
        /// <summary>
        /// 查找一个根节点的所有子节点
        /// </summary>
        /// <param name="dtAll">所有节点表数据</param>
        /// <param name="strIDColumnName">ID列名</param>
        /// <param name="strParentIDColumnName">父ID列名</param>
        /// <param name="strFisrtIDValue">第一个节点值</param>
        /// <returns>子节点表</returns>
        public static DataTable GetAllChild(this DataTable dtAll, string strIDColumnName, string strParentIDColumnName,
            string strFisrtIDValue, IDictionary<string, string[]> dicNotClude, bool IsIncludeRoot)
        {
            DataTable dtReturn = dtAll.Clone();
            DataRow[] drFisrt = dtAll.Select(strIDColumnName + "='" + strFisrtIDValue + "'");
            if (drFisrt.Length > 0)
            {
                if (IsIncludeRoot)
                {
                    dtReturn.ImportRow(drFisrt[0]);
                }
                GetChildData(dtAll, strIDColumnName, strParentIDColumnName, strFisrtIDValue, dicNotClude, dtReturn);
            }
            return dtReturn;
        }

        /// <summary>
        /// 查找一个根节点的所有子节点
        /// </summary>
        /// <param name="dtAll">所有节点表数据</param>
        /// <param name="strIDColumnName">ID列名</param>
        /// <param name="strParentIDColumnName">父ID列名</param>
        /// <param name="strFisrtIDValue">第一个节点值</param>
        /// <returns>子节点表</returns>
        public static DataTable GetAllChild(this DataTable dtAll, string strIDColumnName, string strParentIDColumnName,
            string strFisrtIDValue, IDictionary<string, string[]> dicNotClude)
        {
            return GetAllChild(dtAll, strIDColumnName, strParentIDColumnName, strFisrtIDValue, dicNotClude, true); ;
        }

        /// <summary>
        /// 递归获取子数据方法
        /// </summary>
        /// <param name="dtAll">所有节点表数据</param>
        /// <param name="strIDColumnName">ID列名</param>
        /// <param name="strParentIDColumnName">父ID列名</param>
        /// <param name="strParentIDValue">父节点值</param>
        /// <param name="dicNotClude">剔除的条件，键为列名，值为具体值。值为字符数组，以支持同一个列有多个排除值</param>
        /// <param name="dtReturn">子节点表</param>
        private static void GetChildData(DataTable dtAll, string strIDColumnName, string strParentIDColumnName,
            string strParentIDValue, IDictionary<string, string[]> dicNotClude, DataTable dtReturn)
        {
            #region 构造排除条件
            string strNotClude = "";
            if (dicNotClude != null)
            {
                IEnumerator itor = dicNotClude.GetEnumerator();
                while (itor.MoveNext())
                {
                    string sKey = ((KeyValuePair<string, string[]>)itor.Current).Key;
                    foreach (string item in dicNotClude[sKey])
                    {
                        strNotClude += " AND " + sKey + "<>'" + item + "' ";
                    }
                }
            }
            #endregion
            DataRow[] drFisrtChild = dtAll.Select(strParentIDColumnName + "='" + strParentIDValue + "' " + strNotClude);
            if (drFisrtChild.Length > 0)
            {
                foreach (DataRow dr in drFisrtChild)
                {
                    dtReturn.ImportRow(dr);
                    if (dtAll.Select(strParentIDColumnName + "='" + dr[strIDColumnName].ToString() + "' " + strNotClude).Length > 0)
                    {
                        GetChildData(dtAll, strIDColumnName, strParentIDColumnName, dr[strIDColumnName].ToString(), dicNotClude, dtReturn);
                    }
                }
            }
        }
        #endregion

    }
}
