using Breezee.Core.Interface;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    #region SQL段类
    /// <summary>
    /// SQL段类
    /// </summary>
    public class SqlSegment
    {
        public string SqlClause { private set; get; }

        public SqlSegment(string sqlClause)
        {
            this.SqlClause = sqlClause;
        }

        public override string ToString()
        {
            return this.SqlClause;
        }

        public virtual void Parse(DataBaseType dbt,IDictionary<string, object> dicParam, StringBuilder builder, Dictionary<string, SqlParam> sqlParas, bool update)
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

        public virtual void Parse(DataBaseType dbt, IDictionary<string, string> dicParam, StringBuilder builder, Dictionary<string, SqlParam> sqlParas, bool update)
        {
            Parse(dbt, dicParam.ToObjectDict(), builder, sqlParas, update);
        }
    }
    #endregion
}
