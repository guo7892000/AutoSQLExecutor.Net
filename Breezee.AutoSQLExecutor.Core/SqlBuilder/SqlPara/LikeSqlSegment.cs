using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// Like的SQL段
    /// </summary>
    public class LikeSqlSegment : BaseSqlSegment
    {
        private PercentSignStyle percentSignStyle = PercentSignStyle.Both;
        public LikeSqlSegment(string dicKey, string sqlClause, DbType dbType, PercentSignStyle percentSignStyle)
            : base(dicKey, sqlClause, dbType)
        {
            this.percentSignStyle = percentSignStyle;
        }

        public PercentSignStyle SignStyle
        {
            get { return percentSignStyle; }
            set { percentSignStyle = value; }
        }

        public override void Parse(DataBaseType dbt, IDictionary<string, object> dicParam, StringBuilder builder, Dictionary<string, SqlParam> sqlParas, bool update)
        {
            bool isIncludeCondition = false;
            //加上前后#号的键
            string strJinHaoKey = "#" + Key + "#";
            if (dicParam.ContainsKey(Key) && dicParam[Key] != null && dicParam[Key].ToString().Length > 0)
            {
                isIncludeCondition = true;
                sqlParas[Key] = new SqlParam(Key, DBType, AddPercentSign(dicParam[Key].ToString()));
            }
            else if (dicParam.ContainsKey(strJinHaoKey) && dicParam[strJinHaoKey] != null && dicParam[strJinHaoKey].ToString().Length > 0)
            {
                //增加对#参数#键的支持
                isIncludeCondition = true;
                sqlParas[Key] = new SqlParam(Key, DBType, AddPercentSign(dicParam[strJinHaoKey].ToString()));
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

        private string AddPercentSign(string val)
        {
            string retVal = string.Empty;
            switch (this.SignStyle)
            {
                case PercentSignStyle.Both:
                    retVal = "%" + val + "%";
                    break;
                case PercentSignStyle.Left:
                    retVal = "%" + val;
                    break;
                case PercentSignStyle.Right:
                    retVal = val + "%";
                    break;
                case PercentSignStyle.None:
                default:
                    retVal = val;
                    break;
            }

            return retVal;
        }
    }
}
