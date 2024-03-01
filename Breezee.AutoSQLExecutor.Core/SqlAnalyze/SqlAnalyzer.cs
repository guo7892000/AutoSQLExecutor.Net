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
            string tablePattern = @"(\s+(FROM|JOIN)|(\s*(,|UPDATE|(MERGE\s+INTO))))\s+\S+";
            Regex regex = new Regex(tablePattern, RegexOptions.IgnoreCase);
            MatchCollection mc = regex.Matches(sSql);
            foreach (Match item in mc)
            {
                string sValue = item.Value.Trim();
                string sTableName = sValue.Substring(sValue.LastIndexOf(" ")).Trim();
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
            return listTable;
        }
    }
}
