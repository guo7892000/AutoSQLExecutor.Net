using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Breezee.AutoSQLExecutor.Core
{
    #region 基本的SQL段
    /// <summary>
    /// 基本的SQL段
    /// </summary>
    public class BaseSqlSegment : SqlSegment
    {
        public string Key { private set; get; }

        public DbType DBType { get; set; }

        public BaseSqlSegment(string dicKey, string sqlClause, DbType dbType) : base(sqlClause)
        {
            this.Key = dicKey;
            this.DBType = dbType;
        }

        #region 转换为SQL语句
        public override void Parse(DataBaseType dbt, IDictionary<string, object> dicParam, StringBuilder builder, Dictionary<string, SqlParam> sqlParas, bool update)
        {
            bool isIncludeCondition = false;
            //加上前后#号的键
            string strJinHaoKey = "#" + Key + "#";
            if (update)
            {
                #region 修改
                if (dicParam.ContainsKey(Key))
                {
                    sqlParas[Key] = new SqlParam(Key, DBType, dicParam[Key]);
                    isIncludeCondition = true;
                }
                else if (dicParam.ContainsKey(strJinHaoKey))
                {
                    //增加对#参数#键的支持
                    sqlParas[Key] = new SqlParam(Key, DBType, dicParam[strJinHaoKey]);
                    isIncludeCondition = true;
                }
                #endregion
            }
            else
            {
                #region 新增
                if (dicParam.ContainsKey(Key) && dicParam[Key] != null && dicParam[Key].ToString().Length > 0)
                {
                    sqlParas[Key] = new SqlParam(Key, DBType, dicParam[Key]);
                    isIncludeCondition = true;
                }
                else if (dicParam.ContainsKey(strJinHaoKey) && dicParam[strJinHaoKey] != null && dicParam[strJinHaoKey].ToString().Length > 0)
                {
                    //增加对#参数#键的支持
                    sqlParas[Key] = new SqlParam(Key, DBType, dicParam[strJinHaoKey]);
                    isIncludeCondition = true;
                }
                #endregion
            }

            if (isIncludeCondition)
            {
                //对于参数数，只有oracle、postgreSql使用:作为参数前缀，SqlServer、MySql、SQLite使用@作为参数前缀
                if (dbt == DataBaseType.Oracle || dbt == DataBaseType.PostgreSql)
                {
                    builder.Append(this.SqlClause.Replace("@", ":"));//转换参数前缀
                }
                else
                {
                    builder.Append(this.SqlClause.Replace(":", "@"));//转换参数前缀
                }
                builder.Append(" ");
            }
        }

        public override void Parse(DataBaseType dbt, IDictionary<string, string> dicParam, StringBuilder builder, Dictionary<string, SqlParam> sqlParas, bool update)
        {
            Parse(dbt, dicParam.ToObjectDict(), builder, sqlParas, update);
        } 
        #endregion
    }
    #endregion
}
