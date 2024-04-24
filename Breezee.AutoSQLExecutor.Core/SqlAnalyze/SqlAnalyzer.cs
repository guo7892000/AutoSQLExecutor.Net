using org.breezee.MyPeachNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// SQL分析器
    /// </summary>
    public class SqlAnalyzer
    {
        /// <summary>
        /// 获取SQL中的表清单
        /// </summary>
        /// <param name="sSql"></param>
        /// <returns></returns>
        public static List<string> GetTableList(string sSql)
        {
            //针对SQL移除注释
            MyPeachNetProperties properties= new MyPeachNetProperties();
            AbstractSqlParser sqlParser = new SelectSqlParser(properties);
            sSql = sqlParser.RemoveSqlRemark(sSql, new Dictionary<string, Object>());

            List<string> listTable = new List<string>();
            /*针对FROM或JOIN形式的表名：这里支持多种格式：
             * select * from   [dbo].[BAS_CITY] A,    `dbo`.`BAS_COUNTY` B 
JOIN [dbo].[BAS_PROCE] D ON A.XX= D.DD
UPDATE TAB1 SET DD=SD
MERGE   INTO Tb2
where A.CITY_ID=B.CITY_ID
             */

            //针对FROM、JOIN、UPDATE、MERGE INTO后面的表的正则表达式：暂不支持表名前加数据库名
            string tablePattern = @"\s*(FROM|JOIN|UPDATE|MERGE\s+INTO)\s+([\[`]\w+[\]`].)?[\[`]?\w+[\]`]?";
            Regex regex = new Regex(tablePattern, RegexOptions.IgnoreCase);
            MatchCollection mc = regex.Matches(sSql);
            foreach (Match item in mc)
            {
                string sValue = item.Value.Trim();
                int iIndexTable = sValue.LastIndexOf(" ");
                string sTableName = string.Empty;
                if(iIndexTable == -1)
                {
                    //没有空格
                    string[]  sArrTalbe = sValue.Split('\n');
                    //FROM和表名间使用换行符时
                    if (sArrTalbe.Length > 1)
                    {
                        sTableName = sArrTalbe[1];
                    }
                }
                else
                {
                    //有空格
                    sTableName = sValue.Substring(iIndexTable).Trim();
                }

                if (sTableName.Contains("."))
                {
                    sTableName = sTableName.Substring(sTableName.IndexOf(".") + 1);
                }
                //移除特殊的字符
                sTableName = sTableName.Replace("[", "").Replace("]", "").Replace("`", "");
                //增加查询表
                if (!listTable.Contains(sTableName))
                {
                    listTable.Add(sTableName);
                }
            }

            //针对FROM后面逗号分陋的表名：目前暂没考虑表名前加AS或数据库名称
            tablePattern = @"\s+(FROM)\s+([\[`]\w+[\]`].)?[\[`]?\w+[\]`]?(\s+\w+)*(\s*,\s*([\[`]\w+[\]`].)?[\[`]?\w+[\]`]?(\s+\w+)*)*";
            regex = new Regex(tablePattern, RegexOptions.IgnoreCase);
            mc = regex.Matches(sSql);
            foreach (Match item in mc)
            {
                string sValue = item.Value.Trim();
                int iIndexTable = sValue.LastIndexOf(" ");
                string sTableName = string.Empty;
                if (iIndexTable == -1)
                {
                    //没有空格
                    string[] sArrTalbe = sValue.Split('\n');
                    //FROM和表名间使用换行符时
                    if (sArrTalbe.Length > 1)
                    {
                        sTableName = sArrTalbe[1];
                    }
                }
                else
                {
                    //有空格
                    sTableName = sValue.Substring(iIndexTable).Trim();
                }
                if (sTableName.Contains("."))
                {
                    sTableName = sTableName.Substring(sTableName.IndexOf(".") + 1);
                }
                //移除特殊的字符
                sTableName = sTableName.Replace("[", "").Replace("]", "").Replace("`", "").Replace("'", "").Replace(",", "")
                    .Replace("(", "").Replace(")", "");
                //增加查询表
                if (!listTable.Contains(sTableName))
                {
                    listTable.Add(sTableName);
                }
            }


            return listTable;
        }
    }
}
