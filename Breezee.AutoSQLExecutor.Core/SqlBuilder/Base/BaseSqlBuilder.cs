using Breezee.Core.Interface;
using org.breezee.MyPeachNet;
using System.Data;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// SQL构造器基类
    /// </summary>
    public abstract class BaseSqlBuilder
    {
        #region 全局变量
        public DataBaseType UseDataBaseType { get; } = DataBaseType.SqlServer;

        /// <summary>
        /// Sql或配置路径:由SqlSourceType类型决定 
        /// </summary>
        public string SqlConfigPath;//Sql配置路径
        /// <summary>
        /// 源SQL
        /// </summary>
        public string SourceSql { get; private set; }
        /// <summary>
        /// 执行的SQL（参数化）
        /// </summary>
        public string ExecSql { get; private set; }

        public bool IsNeedAutoParam = false;//是否需要自动参数化

        public abstract SqlExecuteType SqlExecuteType { get; }

        public List<SqlSegment> ListSqlSegment { get; set; } = null;

        public IDataAccess DataAccess { get; set; }

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造SqlQueryParser的实例
        /// </summary>
        public BaseSqlBuilder()
        {
            //DataAccess = ContainerContext.Container.Resolve<IDataAccess>(CFGDataBase.DefaultDbName);//未实现？？？
            UseDataBaseType = DataAccess.DataBaseType;

            ListSqlSegment = new List<SqlSegment>();
        }

        /// <summary>
        /// 构造SqlQueryParser的实例
        /// </summary>
        public BaseSqlBuilder(IDataAccess iDataAccess)
        {
            DataAccess = iDataAccess;
            UseDataBaseType = iDataAccess.DataBaseType;

            ListSqlSegment = new List<SqlSegment>();
        }
        #endregion
        
        #region 设置DataAccess方法
        /// <summary>
        /// 设置DataAccess方法
        /// </summary>
        public BaseSqlBuilder SetDataAccess(IDataAccess iDataAccess)
        {
            DataAccess = iDataAccess;
            return this;
        }
        #endregion

        #region 获取SQL
        /// <summary>
        /// 增加带like参数的sql子句
        /// </summary>
        /// <param name="dicKey">键，对应字典键和绑定变量名字</param>
        /// <param name="sqlClause">sql子句</param>
        /// <param name="percentSignStyle">百分号显示方式，默认为Both</param>
        /// <returns>返回SqlQueryParser的实例</returns>
        public BaseSqlBuilder GetSqlByConfigPath(string sXPath, List<FuncParam> listParam = null)
        {
            string sNotParamSql = SqlConfig.GetGlobalConfigInfo(sXPath);
            IDictionary<string, string> sConditionsKeyValue = new Dictionary<string, string>();
            foreach (FuncParam item in listParam)
            {
                sConditionsKeyValue.Add(item.Code, item.Value.ToString());
            }
            // 参数化处理
            SqlParsers sqlParsers = new SqlParsers(new MyPeachNetProperties());
            ParserResult parserResult = sqlParsers.parse(SqlTypeEnum.SELECT, sNotParamSql, sConditionsKeyValue.ToObjectDict());
            if (parserResult.Code.equals("0"))
            {
                sNotParamSql = parserResult.Sql;
            }
            //移除无效的参数
            //listParam.RemoveAll(r => !dicParam.ContainsKey(r.Code));
            ListSqlSegment.Add(new SqlSegment(sNotParamSql));
            //将参数增加到自定义集合中去
            foreach (var item in listParam)
            {
                if (item.FuncParamType == FuncParamType.DateTime)
                {
                    Condition(item.Code, item.Value.ToString(), DbType.DateTime);
                }
                else
                {
                    Condition(item.Code, item.Value.ToString());
                }
            }

            return this;
        }
        #endregion

        #region Sql方法
        /// <summary>
        /// 增加不带参数的Sql子句.
        /// </summary>
        /// <param name="sqlClause">sql子句</param>
        /// <returns>返回SqlQueryParser的实例</returns>
        public BaseSqlBuilder Sql(string sqlClause)
        {
            ListSqlSegment.Add(new SqlSegment(sqlClause));
            return this;
        }

        /// <summary>
        /// 增加带参数的sql子句
        /// </summary>
        /// <param name="dicKey">键，对应字典键和绑定变量名字</param>
        /// <param name="sqlClause">sql子句</param>
        /// <param name="dbType">数据类型，默认为Varchar2</param>           
        /// <returns>返回SqlQueryParser的实例</returns>
        public BaseSqlBuilder Sql(string dicKey, string sqlClause, DbType dbType = DbType.String)
        {
            return Condition(dicKey, sqlClause, dbType);
        }

        /// <summary>
        /// 增加带参数的sql子句
        /// </summary>
        /// <param name="dicKey">键，对应字典键和绑定变量名字</param>
        /// <param name="sqlClause">sql子句</param>
        /// <param name="isLike">是否like类型的查询子句</param>   
        /// <param name="percentSignStyle">百分号显示方式，默认为Both</param>
        /// <returns>返回SqlQueryParser的实例</returns>
        public BaseSqlBuilder Sql(string dicKey, string sqlClause, bool isLike, PercentSignStyle percentSignStyle = PercentSignStyle.Both)
        {
            if (isLike)
            {
                return Like(dicKey, sqlClause, percentSignStyle);
            }
            else
            {
                return Condition(dicKey, sqlClause, DbType.String);
            }
        }
        #endregion

        #region Condition方法
        /// <summary>
        /// 增加带参数的sql子句
        /// </summary>
        /// <param name="dicKey">键，对应字典键和绑定变量名字</param>
        /// <param name="sqlClause">sql子句</param>
        /// <param name="dbType">数据类型，默认为Varchar2<</param>
        /// <returns>返回SqlQueryParser的实例</returns>
        public BaseSqlBuilder Condition(string dicKey, string sqlClause, DbType dbType = DbType.String)
        {
            ListSqlSegment.Add(new BaseSqlSegment(dicKey, sqlClause, dbType));
            return this;
        }
        #endregion

        #region Like方法
        /// <summary>
        /// 增加带like参数的sql子句
        /// </summary>
        /// <param name="dicKey">键，对应字典键和绑定变量名字</param>
        /// <param name="sqlClause">sql子句</param>
        /// <param name="percentSignStyle">百分号显示方式，默认为Both</param>
        /// <returns>返回SqlQueryParser的实例</returns>
        public BaseSqlBuilder Like(string dicKey, string sqlClause, PercentSignStyle percentSignStyle = PercentSignStyle.Both)
        {
            ListSqlSegment.Add(new LikeSqlSegment(dicKey, sqlClause, DbType.String, percentSignStyle));
            return this;
        }
        #endregion
       
        #region 清除查询语句
        /// <summary>
        /// 清除查询语句
        /// </summary>
        public void Clear()
        {
            ListSqlSegment.Clear();
        }
        #endregion

        #region 返回SqlQueryParser实例的字符表示
        /// <summary>
        /// 返回SqlQueryParser实例的字符表示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            List<SqlParam> lstParam = new List<SqlParam>();

            foreach (SqlSegment segment in ListSqlSegment)
            {
                //对于参数数，只有oracle、postgreSql使用:作为参数前缀，SqlServer、MySql、SQLite使用@作为参数前缀
                if (UseDataBaseType == DataBaseType.Oracle || UseDataBaseType == DataBaseType.PostgreSql)
                {
                    builder.Append(segment.SqlClause.Replace("@",":"));//转换参数前缀
                }
                else
                {
                    builder.Append(segment.SqlClause.Replace(":", "@"));//转换参数前缀
                }
                
                builder.Append(" ");
                builder.AppendLine();
            }

            return builder.ToString();
        }
        #endregion

        #region 调试用的方法
        [System.Diagnostics.Conditional("DEBUG")]
        private void DebugSql(string sql, Dictionary<string, SqlParam> lstParam)
        {
            try
            {
                string ret = sql;
                foreach (SqlParam parameter in lstParam.Values)
                {
                    //ret = ret.Replace("@" + parameter.Name, "'" + parameter.Value + "'");

                    //改正SQL调试替换参数值方式 by pansq
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("(@" + parameter.Name + "\\b)", System.Text.RegularExpressions.RegexOptions.Multiline);
                    ret = regex.Replace(ret, "'" + parameter.Value + "'");
                }

                System.Diagnostics.Debug.WriteLine("--绑定变量替换调试Sql begin--");
                System.Diagnostics.Debug.WriteLine(ret);
                System.Diagnostics.Debug.WriteLine("--绑定变量替换调试Sql end --");
            }
            catch
            {
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void DebugSql(string sql, IDictionary<string, string> dic)
        {
            try
            {
                string ret = sql;
                foreach (KeyValuePair<string, string> pair in dic)
                {
                    //ret = ret.Replace("@" + pair.Key, "'" + pair.Value + "'");

                    //改正SQL调试替换参数值方式 by pansq
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("(@" + pair.Key + "\\b)", System.Text.RegularExpressions.RegexOptions.Multiline);
                    ret = regex.Replace(ret, "'" + pair.Value + "'");
                }

                System.Diagnostics.Debug.WriteLine("--绑定变量替换调试Sql begin--");
                System.Diagnostics.Debug.WriteLine(ret);
                System.Diagnostics.Debug.WriteLine("--绑定变量替换调试Sql end --");
            }
            catch
            {
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void DebugSql(string sql, IDictionary<string, object> dic)
        {
            try
            {
                string ret = sql;
                foreach (KeyValuePair<string, object> pair in dic)
                {
                    //ret = ret.Replace("@" + pair.Key, "'" + pair.Value.ToString() + "'");

                    //改正SQL调试替换参数值方式 by pansq
                    System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("(@" + pair.Key + "\\b)", System.Text.RegularExpressions.RegexOptions.Multiline);
                    ret = regex.Replace(ret, "'" + pair.Value + "'");
                }

                System.Diagnostics.Debug.WriteLine("--绑定变量替换调试Sql begin--");
                System.Diagnostics.Debug.WriteLine(ret);
                System.Diagnostics.Debug.WriteLine("--绑定变量替换调试Sql end --");
            }
            catch
            {
            }
        }
        #endregion

        #region 获取SQL和参数
        /// <summary>
        /// 获取SQL和参数
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public void GetSqlParam(List<FuncParam> listParam, out string sSql,out List<FuncParam> realParam)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                IDictionary<string, string> dicParam = new Dictionary<string, string>();
                if (listParam != null)
                {
                    foreach (var para in listParam)
                    {
                        dicParam[para.Code] = para.Value.ToString();
                    }
                }
                //调用下面方法
                GetSqlParam(dicParam, out sSql, out realParam);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 获取SQL和参数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dicParam"></param>
        /// <param name="sSql"></param>
        /// <param name="realParam"></param>
        public void GetSqlParam(IDictionary<string, string> dicParam, out string sSql, out List<FuncParam> realParam)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                Dictionary<string, SqlParam> lstParam = new Dictionary<string, SqlParam>();

                foreach (SqlSegment segment in ListSqlSegment)
                {
                    segment.Parse(UseDataBaseType,dicParam, builder, lstParam, false);
                }

                realParam = new List<FuncParam>();
                foreach (var sp in lstParam.Values)
                {
                    if (sp.DataType == DbType.DateTime)
                    {
                        realParam.Add(new FuncParam(sp.Name, FuncParamType.DateTime, FuncParamInputType.Option, sp.Value));
                    }
                    else if (sp.DataType == DbType.Date)
                    {
                        realParam.Add(new FuncParam(sp.Name, FuncParamType.DateTime, FuncParamInputType.Option, sp.Value));
                    }
                    else
                    {
                        realParam.Add(new FuncParam(sp.Name, FuncParamType.String, FuncParamInputType.Option, sp.Value));
                    }
                }

                sSql = builder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }

    /// <summary>
    /// SQL执行类型
    /// </summary>
    public enum SqlExecuteType
    {
        /// <summary>
        /// 查询
        /// </summary>
        Query,
        /// <summary>
        /// 分页查询
        /// </summary>
        QueryPage,
        /// <summary>
        /// 非查询
        /// </summary>
        ExecuteNonQuery,
        /// <summary>
        /// 调用存储过程
        /// </summary>
        CallStoreProcedure
    }

}
