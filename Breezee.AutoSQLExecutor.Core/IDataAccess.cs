using System.Text;
using System.Data.Common;
using System.Data;
using System.Collections;
using org.breezee.MyPeachNet;
using Breezee.Core.Interface;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

/*********************************************************************
 * 对象名称：数据访问接口
 * 对象类别：接口
 * 创建作者：黄国辉
 * 创建日期：2022-07-05
 * 对象说明：主要提供数据访问组件的接口定义
 * 修改历史：
 *      V1.0 新建 hgh 2022-07-05
 * ******************************************************************/
namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 数据访问基类
    /// </summary>
    public abstract class IDataAccess
    {
        #region 变量       
        public abstract DataBaseType DataBaseType { get; }
        public abstract string ConnectionString { get; protected set; }
        public abstract ISqlDifferent SqlDiff { get; protected set; }
        public DbConnection Connection { get { return GetCurrentConnection(); } }

        public DbServerInfo DbServer { get; protected set; }

        public SqlParsers SqlParsers { get; protected set; }

        /// <summary>
        /// 字符长度类型集合
        /// </summary>
        public abstract List<string> CharLengthTypes { get; }
        /// <summary>
        /// 精度类型集合
        /// </summary>
        public abstract List<string> PrecisonTypes { get; }
        #endregion

        #region 子类可用属性和方法
        protected DataTable DT_DataBase { get { return DBDataBaseEntity.GetTableStruct(); } }
        /// <summary>
        /// 通过SQL查询得到的架构表信息
        /// </summary>
        protected DataTable DT_SchemaTable { get { return DBTableEntity.GetTableStruct(); } } 
        protected DataTable DT_SchemaTableColumn { get { return DBColumnEntity.GetTableStruct(); } }

        /// <summary>
        /// 【抽象方法】字典转换为DB服务器信息
        /// </summary>
        /// <param name="dicR"></param>
        /// <returns></returns>
        protected abstract DbServerInfo Dic2DbServer(Dictionary<string, string> dic);

        /// <summary>
        /// 连接字符串转换为DB服务器信息
        /// </summary>
        /// <param name="sConstr"></param>
        /// <returns></returns>
        protected DbServerInfo ConnString2Server(string sConstr)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] arrAttr = sConstr.Split(';');
            foreach (string attr in arrAttr)
            {
                string[] arrKeyValue = attr.Split('=');
                if (arrKeyValue.Length > 1)
                {
                    dic[arrKeyValue[0].Trim().ToLower()] = arrKeyValue[1].Trim().ToLower();
                }
            }
            return Dic2DbServer(dic);
        }
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sConstr">连接字符串：注名字要跟Main.config配置文件中的连接字符串配置字符保持一致</param>
        public IDataAccess(string sConstr)
        {
            ConnectionString = sConstr;
            DbServer = ConnString2Server(sConstr);
            SqlParsers = new SqlParsers(new MyPeachNetProperties());          
        }

        public IDataAccess(DbServerInfo server)
        {
            ModifyConnectString(server);
            DbServer = server;
            SqlParsers = new SqlParsers(new MyPeachNetProperties());
        }
        #endregion

        #region 【抽象方法】数据库连接
        /// <summary>
        /// 【抽象方法】获取当前连接
        /// </summary>
        /// <returns></returns>
        public abstract DbConnection GetCurrentConnection();

        /// <summary>
        /// 【抽象方法】修改连接字符串
        /// 为了支持一些能随意连接各种类型数据库的功能
        /// </summary>
        /// <param name="server"></param>
        public abstract void ModifyConnectString(DbServerInfo server);
        #endregion

        #region 常用语句
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="sNotParamSql">SQL语句或SQL语句配置路径</param>
        /// <param name="sConditionsKeyValue">条件字典</param>
        /// <param name="sqlStringType">SQL字符枚举</param>
        /// <param name="conn">数据库连接</param>
        /// <param name="dbTran">事务</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public DataTable QueryData(string sNotParamSql, IDictionary<string, object> sConditionsKeyValue, SqlStringType sqlStringType = SqlStringType.SqlNoParamed, DbConnection conn = null, DbTransaction dbTran = null)
        {
            switch (sqlStringType)
            {
                case SqlStringType.SqlNoParamed:
                    return QueryAutoParamSqlData(sNotParamSql, sConditionsKeyValue, null, null);
                case SqlStringType.SqlParamed:
                    return QueryHadParamSqlData(sNotParamSql, sConditionsKeyValue, null, null);
                case SqlStringType.ConfigPathNoParamed:
                    return QueryAutoParamConfigPathData(sNotParamSql, sConditionsKeyValue, null, null);
                case SqlStringType.ConfigPathParamed:
                    return QueryHadParamConfigPathData(sNotParamSql, sConditionsKeyValue, null, null);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="sNotParamSql">SQL语句或SQL语句配置路径</param>
        /// <param name="sConditionsKeyValue">条件字典</param>
        /// <param name="pParam">分页参数</param>
        /// <param name="TotalString">合计字符，如SUM(QTY),SUM(PRICE_AMOUT)</param>
        /// <param name="sqlStringType">SQL字符枚举</param>
        /// <param name="conn">数据库连接</param>
        /// <param name="dbTran">事务</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public IDictionary<string, object> QueryPageData(string sNotParamSql, IDictionary<string, object> sConditionsKeyValue, PageParam pParam, string TotalString = null, SqlStringType sqlStringType = SqlStringType.SqlNoParamed, DbConnection conn = null, DbTransaction dbTran = null)
        {
            // 配置文件的sql语句
            string sSql = sNotParamSql;
            ParserResult parserResult = null;
            List<FuncParam> listParam = new List<FuncParam>();
            switch (sqlStringType)
            {
                case SqlStringType.SqlNoParamed:
                    parserResult = SqlParsers.parse(SqlTypeEnum.SELECT, sSql, sConditionsKeyValue);// 参数化处理
                    if (parserResult.Code.equals("0"))
                    {
                        sSql = parserResult.Sql;
                        foreach (string sKey in parserResult.ObjectQuery.Keys)
                        {
                            //新参数
                            var paramNew = new FuncParam(sKey);
                            paramNew.Value = parserResult.ObjectQuery[sKey];
                            SetParamType(parserResult.ObjectQuery[sKey], paramNew);
                            //添加到集合
                            listParam.Add(paramNew);
                        }
                    }
                    else
                    {
                        throw new Exception(parserResult.Message);
                    }
                    break;
                case SqlStringType.SqlParamed:
                    foreach (string sKey in sConditionsKeyValue.Keys)
                    {
                        //新参数
                        var paramNew = new FuncParam(sKey);
                        paramNew.Value = sConditionsKeyValue[sKey];
                        SetParamType(sConditionsKeyValue[sKey], paramNew);
                        //添加到集合
                        listParam.Add(paramNew);
                    }
                    break;
                case SqlStringType.ConfigPathNoParamed:
                    sSql = SqlConfig.GetGlobalConfigInfo(sNotParamSql);
                    parserResult = SqlParsers.parse(SqlTypeEnum.SELECT, sSql, sConditionsKeyValue);// 参数化处理
                    if (parserResult.Code.equals("0"))
                    {
                        sSql = parserResult.Sql;
                        foreach (string sKey in parserResult.ObjectQuery.Keys)
                        {
                            //新参数
                            var paramNew = new FuncParam(sKey);
                            paramNew.Value = parserResult.ObjectQuery[sKey];
                            SetParamType(parserResult.ObjectQuery[sKey], paramNew);
                            //添加到集合
                            listParam.Add(paramNew);
                        }
                    }
                    else
                    {
                        throw new Exception(parserResult.Message);
                    }
                    break;
                case SqlStringType.ConfigPathParamed:
                    sSql = SqlConfig.GetGlobalConfigInfo(sNotParamSql);
                    foreach (string sKey in sConditionsKeyValue.Keys)
                    {
                        //新参数
                        var paramNew = new FuncParam(sKey);
                        paramNew.Value = sConditionsKeyValue[sKey];
                        SetParamType(sConditionsKeyValue[sKey], paramNew);
                        //添加到集合
                        listParam.Add(paramNew);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return QueryPageHadParamSqlData(sSql, pParam, listParam, TotalString, conn, dbTran);
        }

        /// <summary>
        /// 执行非查询
        /// </summary>
        /// <param name="sNotParamSql">SQL语句或SQL语句配置路径</param>
        /// <param name="sConditionsKeyValue">条件字典</param>
        /// <param name="sqlStringType">SQL字符枚举</param>
        /// <param name="conn">数据库连接</param>
        /// <param name="dbTran">事务</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int ExecuteNonQuery(string sNotParamSql, IDictionary<string, object> sConditionsKeyValue = null, SqlStringType sqlStringType = SqlStringType.SqlNoParamed, DbConnection conn = null, DbTransaction dbTran = null)
        {
            string sSql = "";
            List<FuncParam> listParam = new List<FuncParam>();
            switch (sqlStringType)
            {
                case SqlStringType.SqlNoParamed:
                    return ExecuteNonQueryAutoParamSql(sNotParamSql, sConditionsKeyValue, null, null);
                case SqlStringType.SqlParamed:
                    foreach (string item in sConditionsKeyValue.Keys)
                    {
                        //新参数
                        var paramNew = new FuncParam(item);

                        paramNew.Value = sConditionsKeyValue[item];
                        SetParamType(sConditionsKeyValue[item], paramNew);
                        //添加到集合
                        listParam.Add(paramNew);
                    }
                    return ExecuteNonQueryHadParamSql(sNotParamSql, listParam, null, null);
                case SqlStringType.ConfigPathNoParamed:
                    // 配置文件的sql语句
                    sSql = SqlConfig.GetGlobalConfigInfo(sNotParamSql);
                    //调用上面的方法查询
                    return ExecuteNonQueryAutoParamSql(sSql, sConditionsKeyValue, conn, dbTran);
                case SqlStringType.ConfigPathParamed:
                    // 配置文件的sql语句
                    sSql = SqlConfig.GetGlobalConfigInfo(sNotParamSql);

                    foreach (string item in sConditionsKeyValue.Keys)
                    {
                        //新参数
                        var paramNew = new FuncParam(item);

                        paramNew.Value = sConditionsKeyValue[item];
                        SetParamType(sConditionsKeyValue[item], paramNew);
                        //添加到集合
                        listParam.Add(paramNew);
                    }
                    return ExecuteNonQueryHadParamSql(sSql, listParam, conn, dbTran);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 存储过程调用(支持游标返回)
        /// 标准格式是: 存储过程名-参数定义-[FILL | EXECUTENONQUERY]
        ///   其中参数定义（多个参数以逗号隔开）格式为：参数编码:INPUT或OUTPUT：类型：长度（对文本为必填）
        /// </summary>
        /// <param name="sStoredProcedure">存储过程名及参数</param>
        /// <param name="sParameterArr">对应存储过程调用的参数列表</param>
        /// <param name="conn">数据库连接</param>
        /// <param name="tran">事务</p    aram>
        /// <returns></returns>
        public object[] CallStoredProcedure(string sProduceName, string[] sParamArr, DbConnection con, DbTransaction tran)
        {
            try
            {
                // 执行查询,调用存储过程
                ConnectionState cs = con.State;
                string sStoredProcedure = sProduceName;// ps.ToString();
                string[] sParameterArr = sParamArr;// ps.ParaValueArray;
                //
                if (string.IsNullOrEmpty(sStoredProcedure))
                {
                    throw new Exception("存储过程不能为空！");
                }
                string[] strArr = sStoredProcedure.Split('-'); // 以'-'为分隔符,分离字符串

                //长度与格式合法性检查
                if (strArr.Length != 3)
                {
                    throw new Exception("存储过程配置错误！标准格式是: 存储过程名-参数定义-[FILL | EXECUTENONQUERY]");
                }
                //存储过程名称
                string procName = strArr[0].Trim(); // 存储过程名称
                DbCommand sqlCommand;
                DbParameter sqlParameter;
                DbDataAdapter adapter;
                //得到命令和适配器
                SetProdureCommond(procName, con, tran, out sqlCommand, out adapter);

                #region 数据库处理

                sqlCommand.CommandTimeout = 60 * 60 * 10;
                sqlCommand.CommandType = CommandType.StoredProcedure; // 设置类型为存储过程

                #region 参数分析
                if (strArr[1].Trim() != "") // 存储过程没有参数
                {
                    if (strArr[1].EndsWith(","))
                    {
                        strArr[1] = strArr[1].Substring(0, strArr[1].Length - 1); //去掉最后一个逗号
                    }

                    string[] sParaArr = strArr[1].Split(',');
                    if (sParameterArr.Length != sParaArr.Length)
                    {
                        throw new Exception("传入参数的个数和配置中的存储过程中的参数个数不一致！");
                    }

                    for (int i = 0; i < sParaArr.Length; i++)
                    {
                        string[] sProperties = sParaArr[i].Split(':');
                        //TODO: 请在运行前先行判断数组必要长度
                        if (sProperties.Length < 3)
                        {
                            throw new Exception(string.Format("解析存储过程 {0} 失败, 参数描述少于3项！", procName));
                        }

                        sqlParameter = null;
                        //对各种数据类型的拆分
                        string parameterCode = sProperties[0];//参数名
                        string executeType = sProperties[1].ToUpper().Trim();//参数类型：INPUT为输入参数，OUTPUT为输出参数
                        string parameterType = sProperties[2].ToUpper().Trim();
                        //增加参数
                        sqlParameter = AddParam(sParameterArr, sqlCommand, sqlParameter, i, sProperties, parameterCode, executeType, parameterType);
                        // 设置参数类型
                        if (executeType == "INPUT")
                        {
                            sqlParameter.Direction = ParameterDirection.Input;
                        }
                        else if (executeType == "INPUTOUTPUT")
                        {
                            sqlParameter.Direction = ParameterDirection.InputOutput;
                        }
                        else if (executeType == "OUTPUT")
                        {
                            sqlParameter.Direction = ParameterDirection.Output;
                        }
                        else if (executeType == "RETURNVALUE")
                        {
                            sqlParameter.Direction = ParameterDirection.ReturnValue;
                        }
                        else
                        {
                            throw new Exception("未定义的存储过程参数的输入输出方向类型：" + executeType);
                        }
                    }
                }
                #endregion

                #region 执行与返回值
                if (strArr[2].ToUpper().Trim() == "FILL")
                {
                    // 取得数据的DataTable集合
                    DataTable dt = new DataTable();
                    adapter.SelectCommand = sqlCommand;
                    adapter.Fill(dt);

                    return new object[] { dt };
                }
                else if (strArr[2].ToUpper().Trim() == "EXECUTENONQUERY")
                {
                    try
                    {
                        if (cs == ConnectionState.Closed)  // 判断连接是否打开
                        {
                            sqlCommand.Connection.Open();
                        }

                        sqlCommand.ExecuteNonQuery();

                        if (cs == ConnectionState.Closed)
                        {
                            sqlCommand.Connection.Close();
                        }
                        //
                        List<object> objList = new List<object>();
                        for (int i = 0; i < sqlCommand.Parameters.Count; i++)
                        {
                            if (sqlCommand.Parameters[i].Direction != ParameterDirection.Input)
                            {
                                objList.Add(sqlCommand.Parameters[i].Value);
                            }
                        }

                        object[] objArr = new object[objList.Count]; // 返回参数集合
                        for (int i = 0; i < objList.Count; i++)
                        {
                            objArr[i] = objList[i];
                        }


                        return objArr;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                        if (cs == ConnectionState.Closed)
                        {
                            sqlCommand.Connection.Close();
                        }
                        throw ex;
                    }
                }
                else
                {
                    throw new Exception("未定义的存储过程执行类型：" + strArr[2].ToUpper().Trim());
                }
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region 查询未参数化的SQL语句
        /// <summary>
        /// 查询未参数化的SQL语句
        /// </summary>
        /// <param name="sSql">SQL语句</param>
        /// <param name="sKeyValue">查询条件</param>
        /// <param name="conn">连接</param>
        /// 
        /// <returns>表</returns>
        public DataTable QueryAutoParamSqlData(string sNotParamSql, List<FuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            IDictionary<string, string> sConditionsKeyValue = new Dictionary<string, string>();
            if (listParam != null)
            {
                foreach (FuncParam item in listParam)
                {
                    sConditionsKeyValue.Add(item.Code, item.Value.ToString());
                }
            }
            // 参数化处理
            ParserResult parserResult = SqlParsers.parse(SqlTypeEnum.SELECT, sNotParamSql, sConditionsKeyValue.ToObjectDict());
            if (parserResult.Code.equals("0"))
            {
                return QueryHadParamSqlData(parserResult.Sql, listParam, conn, dbTran);
            }
            throw new Exception(parserResult.Message);
        }

        public DataTable QueryAutoParamSqlData(string sNotParamSql, IDictionary<string, object> sConditionsKeyValue, DbConnection conn = null, DbTransaction dbTran = null)
        {
            // 参数化处理
            ParserResult parserResult = SqlParsers.parse(SqlTypeEnum.SELECT, sNotParamSql, sConditionsKeyValue);
            if (parserResult.Code.equals("0"))
            {
                var listParam = new List<FuncParam>();
                foreach (SqlKeyValueEntity item in parserResult.entityQuery.Values)
                {
                    //新参数
                    var paramNew = new FuncParam(item.KeyName);
                    paramNew.Value = item.KeyValue;
                    SetParamType(item.KeyValue, paramNew);
                    //添加到集合
                    listParam.Add(paramNew);
                }

                return QueryHadParamSqlData(parserResult.Sql, listParam, conn, dbTran);
            }
            throw new Exception(parserResult.Message);
        }

        /// <summary>
        /// 查询未参数化的SQL语句
        /// </summary>
        /// <param name="sNotParamSql">SQL语句</param>
        /// <param name="sConditionsKeyValue">查询条件</param>
        /// <param name="dicParamType">特殊参数类型</param>
        /// <param name="conn">连接</param>
        /// <param name="dbTran">事务</param>
        /// <returns>表</returns>
        public DataTable QueryAutoParamSqlData(string sNotParamSql, IDictionary<string, string> sConditionsKeyValue, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            // 参数化处理
            ParserResult parserResult = SqlParsers.parse(SqlTypeEnum.SELECT, sNotParamSql, sConditionsKeyValue.ToObjectDict());
            if (parserResult.Code.equals("0"))
            {
                var listParam = new List<FuncParam>();
                foreach (SqlKeyValueEntity item in parserResult.entityQuery.Values)
                {
                    //新参数
                    var paramNew = new FuncParam(item.KeyName);
                    paramNew.Value = item.KeyValue;
                    //设置参数类型
                    SetParamType(item.KeyValue, paramNew);
                    //添加到集合
                    listParam.Add(paramNew);
                }

                return QueryHadParamSqlData(parserResult.Sql, listParam, conn, dbTran);
            }
            throw new Exception(parserResult.Message);
        }
        #endregion

        #region 查询已参数化的SQL语句
        public DataTable QueryHadParamSqlData(string sHadParaSql, IDictionary<string, object> sParamKeyValue = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            var listParam = new List<FuncParam>();
            foreach (string sKey in sParamKeyValue.Keys)
            {
                //新参数
                var paramNew = new FuncParam(sKey);

                paramNew.Value = sParamKeyValue[sKey];
                //设置参数类型
                SetParamType(sParamKeyValue[sKey], paramNew);
                //添加到集合
                listParam.Add(paramNew);
            }

            return QueryHadParamSqlData(sHadParaSql, listParam, conn, dbTran);
        }

        /// <summary>
        /// 查询已参数化的SQL语句
        /// </summary>
        /// <param name="sHadParaSql">SQL语句</param>
        /// <param name="sParamKeyValue">参数值字典</param>
        /// <param name="conn">连接</param>
        /// <returns>表</returns>
        public DataTable QueryHadParamSqlData(string sHadParaSql, IDictionary<string, string> sParamKeyValue = null, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            var listParam = new List<FuncParam>();
            foreach (string item in sParamKeyValue.Keys)
            {
                //新参数
                var paramNew = new FuncParam(item);

                paramNew.Value = sParamKeyValue[item];
                if (dicParamType != null && dicParamType.ContainsKey(item))
                {
                    if (dicParamType[item] == SqlParamType.DateTime)
                    {
                        paramNew.FuncParamType = FuncParamType.DateTime;
                    }
                }
                //添加到集合
                listParam.Add(paramNew);
            }

            return QueryHadParamSqlData(sHadParaSql, listParam, conn, dbTran);
        }

        /// <summary>
        /// 【抽象方法】查询已参数化的SQL语句
        /// </summary>
        /// <param name="sParaSql">已经参数化的SQL</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <param name="conn">数据库连接</param>
        /// <returns></returns>
        public abstract DataTable QueryHadParamSqlData(string sHadParaSql, List<FuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null);

        #endregion

        #region 根据配置文件读取SQL查询
        public DataTable QueryAutoParamConfigPathData(string sXPath, IDictionary<string, object> sKeyValue, DbConnection conn = null, DbTransaction dbTran = null)
        {
            string sSql = string.Empty;
            try
            {
                // 配置文件的sql语句
                sSql = SqlConfig.GetGlobalConfigInfo(sXPath);
                //调用上面的方法查询
                DataTable dt = QueryAutoParamSqlData(sSql, sKeyValue, conn, dbTran);
                dt.TableName = sXPath;
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据配置文件读取SQL查询
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public DataTable QueryAutoParamConfigPathData(string sXPath, IDictionary<string, string> sKeyValue, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            string sSql = string.Empty;
            try
            {
                // 配置文件的sql语句
                sSql = SqlConfig.GetGlobalConfigInfo(sXPath);
                //调用上面的方法查询
                DataTable dt = QueryAutoParamSqlData(sSql, sKeyValue, dicParamType, conn, dbTran);
                dt.TableName = sXPath;
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据配置文件读取SQL查询
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public DataTable QueryAutoParamConfigPathData(string sXPath, List<FuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            string sSql = string.Empty;
            try
            {
                // 配置文件的sql语句
                sSql = SqlConfig.GetGlobalConfigInfo(sXPath);
                //调用上面的方法查询
                DataTable dt = QueryAutoParamSqlData(sSql, listParam, conn, dbTran);
                dt.TableName = sXPath;
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable QueryHadParamConfigPathData(string sXPath, IDictionary<string, object> sParamKeyValue = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                // 配置文件的sql语句
                string sHadParamSql = SqlConfig.GetGlobalConfigInfo(sXPath);

                return QueryHadParamSqlData(sHadParamSql, sParamKeyValue, conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 根据配置文件读取已参数化SQL查询
        /// </summary>
        /// <param name="sHadParaSql">SQL语句</param>
        /// <param name="sParamKeyValue">参数值字典</param>
        /// <param name="conn">连接</param>
        /// <returns>表</returns>
        public DataTable QueryHadParamConfigPathData(string sXPath, IDictionary<string, string> sParamKeyValue = null, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                // 配置文件的sql语句
                string sHadParamSql = SqlConfig.GetGlobalConfigInfo(sXPath);

                return QueryHadParamSqlData(sHadParamSql, sParamKeyValue, dicParamType, conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 查询已参数化的分页SQL语句
        /// <summary>
        /// 查询已参数化的分页SQL语句
        /// </summary>
        /// <param name="sHadParaSql">SQL语句</param>
        /// <param name="sParamKeyValue">参数值字典</param>
        /// <param name="conn">连接</param>
        /// <returns>表</returns>
        public IDictionary<string, object> QueryPageHadParamSqlData(string sHadParaSql, PageParam pParam, List<FuncParam> listParam = null, string TotalString = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                if (pParam == null || string.IsNullOrEmpty(pParam.PageOrderString))
                {
                    throw new Exception("PageParam和其排序字段不能为空！");
                }

                int pageSize = pParam.PageSize;
                int pageNo = pParam.PageNO;
                IDictionary<string, object> dicRes = new Dictionary<string, object>();
                bool IsCalOtherTotal = false; //是否计算其他统计项
                if (!string.IsNullOrEmpty(TotalString))
                {
                    IsCalOtherTotal = true;
                }
                //1.查询总记录数
                string pageCountSql = "SELECT COUNT(1) FROM (" + sHadParaSql + ") RALL";
                string[] parms = null;
                if (IsCalOtherTotal)
                {
                    parms = TotalString.Split(new string[] { "," }, StringSplitOptions.None);
                    pageCountSql = "SELECT COUNT(1)," + TotalString + " FROM (" + sHadParaSql + ") RALL";
                }
                //查询
                DataTable dt = QueryHadParamSqlData(pageCountSql, listParam, conn, dbTran);

                int totalRowCount = dt.Rows.Count > 0 ? int.Parse(dt.Rows[0][0].ToString()) : 0;

                if (IsCalOtherTotal)
                {
                    for (int i = 0; i < parms.Length; i++)
                    {
                        dicRes[parms[i]] = dt.Rows.Count > 0 ? dt.Rows[0][i + 1].ToString() : "0";
                    }
                }

                //2.查询分页数据
                int pageCount = 0;
                // 计算总页数
                if (totalRowCount % pageSize == 0)// 整除
                {
                    pageCount = totalRowCount / pageSize;
                }
                else // 不整除，总页数加1
                {
                    pageCount = totalRowCount / pageSize + 1;
                }
                int beginRow = (pageNo - 1) * pageSize + 1;
                int endRow = pageNo * pageSize;

                //获取公页后的SQL
                string pageDataSql = GetPageSql(sHadParaSql, pParam, beginRow, endRow);

                DataTable dtData = QueryHadParamSqlData(pageDataSql, listParam, conn, dbTran);

                //3.返回分页结果                
                dicRes[StaticConstant.TOTAL_COUNT] = totalRowCount; //总记录数
                dicRes[StaticConstant.PAGE_SIZE] = pageCount; //总页数
                dicRes[StaticConstant.FRA_QUERY_RESULT] = dtData;

                dicRes[StaticConstant.FRA_RETURN_FLAG] = "1";
                dicRes[StaticConstant.FRA_USER_MSG] = "查询成功！";
                return dicRes;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// 获取分页SQL
        /// </summary>
        /// <param name="sHadParaSql"></param>
        /// <returns></returns>
        public abstract string GetPageSql(string sHadParaSql, PageParam pParam, int beginRow, int endRow);
        #endregion       

        #region 执行非查询类SQL（只返回影响记录条数）
        /// <summary>
        /// 【抽象方法】执行已参数化的SQL语句
        /// </summary>
        /// <param name="strSql">要执行的SQL</param>
        /// <returns>返回影响记录条数</returns>
        public abstract int ExecuteNonQueryHadParamSql(string sHadParaSql, List<FuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null);

        /// <summary>
        /// 执行已参数化的SQL语句
        /// 为兼容旧的方法使用
        /// </summary>
        /// <param name="sHadParaSql"></param>
        /// <param name="dicParam"></param>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="keys">参数清单</param>
        /// <returns></returns>
        public int ExecuteNonQueryHadParamSql(string sHadParaSql, IDictionary<string, string> dicParam, DbConnection conn, DbTransaction tran, params string[] keys)
        {
            IDictionary<string, string> dicParamReal = new Dictionary<string, string>();
            if (dicParam != null && dicParam.Keys.Count > 0)
            {
                IEnumerable<string> lstKeys = null;
                string strNoExistsKey = "";
                if (keys == null || keys.Count() == 0)
                {
                    lstKeys = dicParam.Keys.Where(x => x != StaticConstant.UNIQUE_FLAG);
                }
                else
                {
                    lstKeys = keys;
                }

                foreach (string key in lstKeys)
                {
                    //增加键不存在时，一次抛出提示，以方便修改 hgh2014-10-29
                    if (!dicParam.ContainsKey(key))
                    {
                        strNoExistsKey = strNoExistsKey + key + ",";
                        continue;
                    }
                    //有效的参数
                    dicParamReal[key] = dicParam[key];
                }
                if (!string.IsNullOrEmpty(strNoExistsKey))
                {
                    throw new ArgumentNullException("以下键未传入：" + strNoExistsKey.Substring(0, strNoExistsKey.Length - 1));
                }
            }
            //调用执行参数化方法
            return ExecuteNonQueryHadParamSql(sHadParaSql, dicParamReal, null, conn, tran);
        }

        /// <summary>
        /// 执行已参数化的SQL语句
        /// </summary>
        /// <param name="sHadParaSql">SQL语句</param>
        /// <param name="sParamKeyValue">参数值字典</param>
        /// <param name="conn">连接</param>
        /// <returns>表</returns>
        public int ExecuteNonQueryHadParamSql(string sHadParaSql, IDictionary<string, string> sParamKeyValue = null, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            var listParam = new List<FuncParam>();
            foreach (string item in sParamKeyValue.Keys)
            {
                //新参数
                var paramNew = new FuncParam(item);

                paramNew.Value = sParamKeyValue[item];
                if (dicParamType != null && dicParamType.ContainsKey(item))
                {
                    if (dicParamType[item] == SqlParamType.DateTime)
                    {
                        paramNew.FuncParamType = FuncParamType.DateTime;
                    }
                }
                //添加到集合
                listParam.Add(paramNew);
            }

            return ExecuteNonQueryHadParamSql(sHadParaSql, listParam, conn, dbTran);
        }

        public int ExecuteNonQueryAutoParamSql(string sNotParamSql, IDictionary<string, object> dicQuery = null,  DbConnection conn = null, DbTransaction dbTran = null)
        {
            if (dicQuery == null)
            {
                dicQuery = new Dictionary<string, object>();
            }
            // 参数化处理
            ParserResult parserResult = SqlParsers.parse(SqlTypeEnum.SELECT, sNotParamSql, dicQuery);
            if (parserResult.Code.equals("0"))
            {
                var listParam = new List<FuncParam>();
                foreach (SqlKeyValueEntity item in parserResult.entityQuery.Values)
                {
                    //新参数
                    var paramNew = new FuncParam(item.KeyName);

                    paramNew.Value = item.KeyValue;
                    SetParamType(item.KeyValue, paramNew);
                    //添加到集合
                    listParam.Add(paramNew);
                }
                return ExecuteNonQueryHadParamSql(sNotParamSql, listParam, conn, dbTran);
            }
            throw new Exception(parserResult.Message);
        }

        /// <summary>
        /// 执行未参数化的SQL语句
        /// </summary>
        /// <param name="strSql">要执行的SQL</param>
        /// <returns>返回影响记录条数</returns>
        public int ExecuteNonQueryAutoParamSql(string sNotParamSql, IDictionary<string, string> dicQuery = null, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            if (dicQuery == null)
            {
                dicQuery = new Dictionary<string, string>();
            }
            // 参数化处理
            ParserResult parserResult = SqlParsers.parse(SqlTypeEnum.SELECT, sNotParamSql, dicQuery.ToObjectDict());
            if (parserResult.Code.equals("0"))
            {
                var listParam = new List<FuncParam>();
                foreach (SqlKeyValueEntity item in parserResult.entityQuery.Values)
                {
                    //新参数
                    var paramNew = new FuncParam(item.KeyName);

                    paramNew.Value = item.KeyValue;
                    if (dicParamType != null && dicParamType.ContainsKey(item.KeyName))
                    {
                        if (dicParamType[item.KeyName] == SqlParamType.DateTime)
                        {
                            paramNew.FuncParamType = FuncParamType.DateTime;
                        }
                    }
                    //添加到集合
                    listParam.Add(paramNew);
                }
                return ExecuteNonQueryHadParamSql(parserResult.Sql, listParam, conn, dbTran);
            }
            throw new Exception(parserResult.Message);
        }

        /// <summary>
        /// 执行未参数化的SQL语句【XML配置路径】
        /// </summary>
        /// <param name="strSql">要执行的SQL</param>
        /// <returns>返回影响记录条数</returns>
        public int ExecuteNonQueryConfigPath(string sXPath, IDictionary<string, string> dicQuery = null, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            string sSql = string.Empty;
            try
            {
                // 配置文件的sql语句
                sSql = SqlConfig.GetGlobalConfigInfo(sXPath);
                //调用上面的方法查询
                return ExecuteNonQueryAutoParamSql(sSql, dicQuery, dicParamType,conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 执行已参数化的SQL语句【XML配置路径】
        /// </summary>
        /// <param name="sXPath"></param>
        /// <param name="sParamKeyValue"></param>
        /// <param name="dicParamType"></param>
        /// <param name="conn"></param>
        /// <param name="dbTran"></param>
        /// <returns></returns>
        public int ExecuteNonQueryHadParamConfigPath(string sXPath, IDictionary<string, string> sParamKeyValue = null, IDictionary<string, SqlParamType> dicParamType = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            // 配置文件的sql语句
            string sHadParamSql = SqlConfig.GetGlobalConfigInfo(sXPath);
            return ExecuteNonQueryHadParamSql(sHadParamSql, sParamKeyValue, dicParamType,conn, dbTran);
        }
        #endregion

        #region 存储过程调用
        /// <summary>
        /// 【抽象方法】增加参数
        /// </summary>
        /// <param name="sParameterArr"></param>
        /// <param name="dbCommand"></param>
        /// <param name="oracleParameter"></param>
        /// <param name="i"></param>
        /// <param name="sProperties"></param>
        /// <param name="parameterCode"></param>
        /// <param name="executeType"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public abstract DbParameter AddParam(string[] sParameterArr, DbCommand dbCommand, DbParameter oracleParameter, int i, string[] sProperties, string parameterCode, string executeType, string parameterType);

        /// <summary>
        /// 【抽象方法】设置存储过程命令
        /// </summary>
        /// <param name="sSql"></param>
        /// <param name="con"></param>
        /// <param name="tran"></param>
        /// <param name="dbCommand"></param>
        /// <param name="adpater"></param>
        public abstract void SetProdureCommond(string sSql, DbConnection con, DbTransaction tran, out DbCommand dbCommand, out DbDataAdapter adpater);
        #endregion

        #region 保存表方法
        /// <summary>
        /// 【抽象方法】更新单表方法
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="dbTran">事务</param>
        /// <param name="dt">要更新的单表</param>
        /// <returns>更新后的表</returns>
        public abstract DataTable SaveTable(DataTable dt, DbConnection conn, DbTransaction dbTran);

        /// <summary>
        /// 保存多表方法
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="dbTran">事务</param>
        /// <param name="dt">要更新的表</param>
        /// <returns>更新后的表</returns>
        public DataTable[] SaveTable(DataTable[] dtArr, DbConnection conn, DbTransaction dbTran)
        {
            foreach (DataTable dt in dtArr)
            {
                SaveTable(dt, conn, dbTran);
            }
            return dtArr;
        }

        /// <summary>
        /// 保存多表方法
        /// </summary>
        /// <param name="dtIn">传入表数组</param>
        /// <returns></returns>
        public DataTable[] SaveTable(DataTable[] dtInArr, IDataAccess dac)
        {
            DbConnection con = dac.GetCurrentConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            DbTransaction DbTran = con.BeginTransaction();
            foreach (DataTable dtIn in dtInArr)
            {
                dac.SaveTable(dtIn, con, DbTran);
            }
            DbTran.Commit();
            return dtInArr;
        }

        /// <summary>
        /// 保存多表方法
        /// </summary>
        /// <param name="dtIn">传入表</param>
        /// <returns></returns>
        public DataTable SaveTable(params DataTable[] dtIn)
        {
            DataTable dtReturn = new DataTable();
            DbConnection con = GetCurrentConnection();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            DbTransaction DbTran = con.BeginTransaction();
            foreach (DataTable dtOne in dtIn)
            {
                dtReturn = SaveTable(dtOne, con, DbTran);
            }
            
            DbTran.Commit();
            return dtReturn;
        }
        #endregion

        #region 动态SQL操作表
        #region 查询数据单表数据
        /// <summary>
        /// 查询数据单表数据
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="strTableName">数据库中的表名</param>
        /// <param name="sKeyValue">条件</param>
        /// <returns>指定条件所有列的表</returns>
        public DataTable QuerySingleTableData(string strTableName, IDictionary<string, string> sKeyValue, DbConnection conn = null)
        {
            DataTable dtDetail = GetSchemaTableColumns(strTableName);
            if (dtDetail == null || dtDetail.Rows.Count == 0)
            {
                throw new Exception(string.Format("表{0}不存在！", strTableName));
            }

            if (conn == null)
            {
                GetCurrentConnection();
            }

            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("SELECT * FROM " + strTableName);
            IEnumerator itor = sKeyValue.GetEnumerator();
            while (itor.MoveNext())
            {
                string sKey = ((KeyValuePair<string, string>)itor.Current).Key;
                sbSql.Append(" AND " + sKey + "=" + SqlDiff.ParamPrefix + sKey);
            }
            //调用查询参数SQL方法
            return QueryHadParamSqlData(sbSql.ToString(), sKeyValue, null, conn);
        }
        #endregion

        #region 获取一个表结构
        /// <summary>
        /// 获取一个表结构
        ///     注：没有数据
        /// </summary>
        /// <param name="strTableName">表名</param>
        /// <param name="columnsList">列清单，为空时查全部列</param>
        /// <param name="isSetCommonColumnsValue">是否设置通用列默认值</param>
        /// <param name="loginUser">当前登录用户，为空时创建人、修改人、组织ID为空</param>
        /// <returns></returns>
        public DataTable GetTableConstruct(string strTableName, List<string> columnsList = null)
        {
            DataTable dtDetail = GetSchemaTableColumns(strTableName);
            if (dtDetail == null || dtDetail.Rows.Count == 0)
            {
                throw new Exception(string.Format("表{0}不存在！", strTableName));
            }

            string strSql = string.Format(@"SELECT * FROM {0} where 1=2", strTableName);
            DataTable dtReturn = new DataTable();
            using (DbConnection con = GetCurrentConnection())
            {
                dtReturn = QueryHadParamSqlData(strSql, new Dictionary<string, string>(), null, con);
                dtReturn.TableName = strTableName;
                if (columnsList == null || columnsList.Count == 0)
                {
                    return dtReturn;
                }
                else
                {
                    StringBuilder sbCol = new StringBuilder();
                    string strLast = columnsList.Last<string>();
                    foreach (string col in columnsList)
                    {
                        if (!dtReturn.Columns.Contains(col))
                        {
                            throw new Exception(strTableName + "表没有包括“" + col + "”列，请修正或删除该字段。");
                        }
                        sbCol.Append(col);
                        if (col != strLast)
                        {
                            sbCol.Append(",");
                        }
                    }
                    strSql = string.Format(@"SELECT {0} FROM {1} where 1=2", sbCol.ToString(), strTableName);
                    dtReturn = QueryHadParamSqlData(strSql, new Dictionary<string, string>(), null, con);
                    dtReturn.TableName = strTableName;
                }
                return dtReturn;
            }
        }
        #endregion

        #region 通过主表ID获取明细表数据
        /// <summary>
        /// 名称：通过主表ID获取明细表数据
        /// 作者：黄国辉
        /// 日期：2013-12-04
        /// </summary>
        /// <param name="strDetailTableName">明细表名</param>
        /// <param name="strMainKeyName">明细表中的主表ID列名</param>
        /// <param name="strValue">主表ID值</param>
        /// <param name="isHaveEnptyAll">是否含空白项</param>
        /// <returns></returns>
        public DataTable GetDetailByMainID(string strDetailTableName, string strMainKeyName, string strValue, bool isHaveEnptyAll)
        {
            DataTable dtDetail = GetSchemaTableColumns(strDetailTableName);
            if (dtDetail == null || dtDetail.Rows.Count == 0)
            {
                throw new Exception(string.Format("表{0}不存在！", strDetailTableName));
            }
            string strSql = string.Format(@"SELECT * FROM {0} where {1} = @{1}", strDetailTableName, strMainKeyName);
            DataTable dtReturn = new DataTable();
            using (DbConnection con = GetCurrentConnection())
            {
                IDictionary<string, string> dic = new Dictionary<string, string>();
                dic[strMainKeyName] = strValue;

                dtReturn = QueryHadParamSqlData(strSql, dic, null, con);
                if (isHaveEnptyAll)
                {
                    dtReturn.Rows.InsertAt(dtReturn.NewRow(), 0);
                }
                return dtReturn;
            }
        }
        #endregion

        #region 删除表数据
        /// <summary>
        /// 删除表数据
        /// </summary>
        /// <param name="strTableName"></param>
        /// <param name="dicSave"></param>
        /// <param name="DbTran"></param>
        /// <returns></returns>
        public bool DeleteTableData(IBaseEntity strTableName, IDictionary<string, string> dicSave, DbConnection conn = null, DbTransaction DbTran = null)
        {
            if (dicSave.Keys.Count == 0)
            {
                throw new Exception("删除失败，条件为空。请在字段中传入删除条件的列名及其值！");
            }

            DataTable dtDetail = GetSchemaTableColumns(strTableName.DBTableName);
            if (dtDetail == null || dtDetail.Rows.Count == 0)
            {
                throw new Exception(string.Format("表{0}不存在！", strTableName.DBTableName));
            }

            StringBuilder sbCondition = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in dicSave)
            {
                sbCondition.Append(" AND " + kvp.Key + " = " + SqlDiff.ParamPrefix + kvp.Value);
            }
            //删除记录
            string strSql = "DELETE FROM " + strTableName.DBTableName + " WHERE 1=1 " + sbCondition.ToString();
            //执行已参数化的非查询
            ExecuteNonQueryHadParamSql(strSql, dicSave, null, conn, DbTran);
            return true;
        }
        #endregion 
        #endregion

        #region 【抽象方法】获取单据编号
        /// <summary>
        /// 【抽象方法】获取单据编号
        /// </summary>
        /// <author>黄国辉</author>
        /// <param type="DbConnection" name="con">连接</param>
        /// <param type="DbTransaction" name="tran">事务</param>
        /// <param type="string" name="strRuleCode">单据规则配置编码</param>
        /// <param type="string" name="strOrgID">组织ID</param>
        /// <returns>返回新单据编号</returns>
        public abstract string GetOrderCode(DbConnection con, DbTransaction tran, string strRuleCode, string strOrgID);
        #endregion

        #region 【抽象方法】通过连接对象获取数据库元数据信息
        /// <summary>
        /// 获取数据库架构信息
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="restrictionValues">参数restrictionValues为请求架构的一组限制值，对于不同的架构集类型，有不同的写法。要具体了解，可以调用GetSchema("Restrictions") 方法。</param>
        /// <returns></returns>
        public abstract DataTable GetSchema(string collectionName, string[] restrictionValues);

        /// <summary>
        /// 获取数据库清单
        /// </summary>
        /// <returns></returns>
        public abstract DataTable GetDataBases();

        /// <summary>
        /// 获取用户表清单
        /// </summary>
        /// <returns></returns>
        public abstract DataTable GetSchemaTables(string sTableName = null, string sSchema = null);
        
        /// <summary>
        /// 获取数据库表列信息
        /// </summary>
        /// <author>黄国辉</author>
        /// <param type="string" name="sTableName">表名</param>
        /// <returns>返回新单据编号</returns>
        public abstract DataTable GetSchemaTableColumns(string sTableName);
        #endregion

        #region 【抽象方法】通过SQL获取数据库元数据信息
        /// <summary>
        /// 【抽象方法】获取表清单
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="schema">架构名</param>
        /// <returns></returns>
        public abstract DataTable GetSqlSchemaTables(string sTableName = null, string sSchema = null);
        /// <summary>
        /// 【抽象方法】获取某表的列清单
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public abstract DataTable GetSqlSchemaTableColumns(string sTableName, string sSchema = null);
        #endregion

        private static void SetParamType(object item, FuncParam paramNew)
        {
            if (item is DateTime)
            {
                paramNew.FuncParamType = FuncParamType.DateTime;
            }
            else if (item is int)
            {
                paramNew.FuncParamType = FuncParamType.Int;
            }
            else if (item is string)
            {
                paramNew.FuncParamType = FuncParamType.String;
            }
        }
    }


}
