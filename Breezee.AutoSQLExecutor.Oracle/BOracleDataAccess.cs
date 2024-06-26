﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Data.Common;
using System.Xml;
using Breezee.AutoSQLExecutor.Core;
using Breezee.Core.Interface;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;
using org.breezee.MyPeachNet;
using System.Diagnostics;

namespace Breezee.AutoSQLExecutor.Oracle
{
    /// <summary>
    /// Oracle数据访问实现层
    ///  注：使用Oracle公司的驱动。
    /// </summary>
    public class BOracleDataAccess : IDataAccess
    {
        #region 属性
        public override DataBaseType DataBaseType
        {
            get { return DataBaseType.Oracle; }
        }

        private string _ConnectionString;
        public override string ConnectionString
        {
            get { return _ConnectionString; }
            protected set { _ConnectionString = value; }
        }

        ISqlDifferent _SqlDiff = new BOracleSqlDifferent();
        public override ISqlDifferent SqlDiff { get => _SqlDiff; protected set => _SqlDiff = value; }

        public override List<string> CharLengthTypes { get => (new string[] { OracleColumnType.Text.Char, OracleColumnType.Text.nvarchar2, OracleColumnType.Text.varchar2 }).ToList(); }
        public override List<string> PrecisonTypes { get => (new string[] { OracleColumnType.Precision.number }).ToList(); }
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sConstr">连接字符串：注名字要跟Main.config配置文件中的连接字符串配置字符保持一致</param>
        public BOracleDataAccess(string sConstr) : base(sConstr)
        {
            _ConnectionString = sConstr;
            SqlParsers.properties.ParamPrefix = ":"; //注：Oracle是使用冒号作为参数前缀
        }
        public BOracleDataAccess(DbServerInfo server) : base(server)
        {
            SqlParsers.properties.ParamPrefix = ":"; //注：Oracle是使用冒号作为参数前缀
        }
        #endregion

        #region 创建数据库连接
        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <returns></returns>
        public override DbConnection GetCurrentConnection()
        {
            return new OracleConnection(_ConnectionString);
        }
        #endregion

        #region 修改连接字符串
        /// <summary>
        /// 修改连接字符串
        /// 为了支持一些能随意连接各种类型数据库的功能
        /// </summary>
        /// <param name="server"></param>
        public override void ModifyConnectString(DbServerInfo server)
        {
            /*注：请保证可执行文件生成的全路径不要包括“括号”，否则会报“ORA - 12154: TNS: 无法解析指定的连接标识符”错误！
             * Data Source有三种配置：
             * 1、使用TNS名称：这种最简单，但有时也会报“ORA - 12154: TNS: 无法解析指定的连接标识符”错误。需要使用其他两种方式
             * 2、//localhost:1521/orcl：使用IP、端口号及TNS名称
             * 3、(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)))：TNS配置名称，这里是使用配置内容。注意需要写在一行中，不能换行。
             * 连接字符串示例：Data Source = HUI; User ID = test01; Password = test01;*/
            _ConnectionString = server.UseConnString ? server.ConnString : string.Format("Data Source={0};User ID={1};Password={2};", server.ServerName, server.UserName, server.Password);
        }
        #endregion

        #region 实现字典转换为DB服务器方法
        protected override DbServerInfo Dic2DbServer(Dictionary<string, string> dic)
        {
            DbServerInfo server = new DbServerInfo();
            server.DatabaseType = DataBaseType.Oracle;
            foreach (var item in dic)
            {
                //注意:键是小写字符
                if (item.Key.Equals("data source"))
                {
                    server.ServerName = item.Value;
                }
                else if (item.Key.Equals("user id"))
                {
                    server.UserName = item.Value;
                    server.SchemaName = item.Value.ToUpper();
                }
                else if (item.Key.Equals("password"))
                {
                    server.Password = item.Value;
                }
                else
                {

                }
            }
            DbServerInfo.ResetConnKey(server);
            return server;
        }
        #endregion

        #region 查询已参数化SQL数据
        /// <summary>
        /// 查询已参数化SQL数据
        /// </summary>
        /// <param name="sHadParaSql">SQL语句</param>
        /// <param name="sParamKeyValue">参数值字典</param>
        /// <param name="conn">连接</param>
        /// <returns>表</returns>
        public override DataTable QueryHadParamSqlData(string sHadParaSql, List<FuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            //开始计时
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                //数据库连接是否为空
                if (conn == null)
                {
                    if (dbTran == null)
                    {
                        conn = GetCurrentConnection();
                    }
                    else
                    {
                        conn = dbTran.Connection;
                    }
                }
                //构造命令
                OracleCommand sqlCommon = new OracleCommand(sHadParaSql, (OracleConnection)conn);
                if (dbTran != null)
                {
                    sqlCommon.Transaction = (OracleTransaction)dbTran;
                }

                //构造适配器
                OracleDataAdapter adapter = new OracleDataAdapter(sqlCommon);

                if (listParam != null)
                {
                    foreach (FuncParam item in listParam)
                    {
                        OracleParameter sp = new OracleParameter(item.Code, item.Value);
                        if (item.FuncParamType == FuncParamType.DateTime)
                        {
                            sp.DbType = DbType.DateTime;
                        }
                        adapter.SelectCommand.Parameters.Add(sp);
                    }
                }
                
                //查询数据并返回
                DataTable dt = new DataTable();
                adapter.SelectCommand.CommandTimeout = 60 * 60 * 10;
                adapter.Fill(dt);
                dt.TableName = Guid.NewGuid().ToString("N");
                stopwatch.Stop(); //结束计时
                //写SQL日志
                LogSql(SqlLogType.Normal, sHadParaSql, listParam, stopwatch.ElapsedMilliseconds);
                return dt;
            }
            catch (Exception ex)
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
                //写SQL日志
                LogSql(SqlLogType.Error, sHadParaSql, listParam, stopwatch.ElapsedMilliseconds, ex);
                throw ex;
            }
        }
        #endregion

        #region 获取分页后的SQL
        public override string GetPageSql(string sHadParaSql, PageParam pParam, int beginRow, int endRow)
        {
            string pageDataSql = "SELECT ROWNUM AS ROWNUM, myTable.*" + " FROM (" + sHadParaSql + ") MYTABLE where ROWNUM<=" + endRow.ToString();
            pageDataSql = "SELECT * FROM (" + pageDataSql + ") where ROWNUM>=" + beginRow.ToString();
            return pageDataSql;
        }
        #endregion

        #region 执行非查询类SQL
        /// <summary>
        /// 执行更新数据SQL（只返回影响记录条数）
        /// </summary>
        /// <param name="strSql">要执行的SQL</param>
        /// <returns>返回影响记录条数</returns>
        public override int ExecuteNonQueryHadParamSql(string sHadParaSql, List<FuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            //开始计时
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                //数据库连接是否为空
                if (conn == null)
                {
                    if (dbTran == null)
                    {
                        conn = GetCurrentConnection();
                    }
                    else
                    {
                        conn = dbTran.Connection;
                    }
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                }
                //构造命令
                OracleCommand sqlCommon = new OracleCommand(sHadParaSql, (OracleConnection)conn);
                if (dbTran != null)
                {
                    sqlCommon.Transaction = (OracleTransaction)dbTran;
                }

                if (listParam != null)
                {
                    foreach (FuncParam item in listParam)
                    {
                        OracleParameter sp = new OracleParameter(item.Code, item.Value);
                        if (item.FuncParamType == FuncParamType.DateTime)
                        {
                            sp.DbType = DbType.DateTime;
                        }
                        sqlCommon.Parameters.Add(sp);
                    }
                }

                //执行SQL
                int iAff = sqlCommon.ExecuteNonQuery();
                stopwatch.Stop(); //结束计时
                LogSql(SqlLogType.Normal, sHadParaSql, listParam, stopwatch.ElapsedMilliseconds);//写SQL日志
                return iAff;
            }
            catch (Exception ex)
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
                LogSql(SqlLogType.Error, sHadParaSql, listParam, stopwatch.ElapsedMilliseconds, ex); //写SQL日志
                throw ex;
            }
        }
        #endregion

        #region 【重要】更新单表方法
        /// <summary>
        /// 【重要】更新单表方法
        /// </summary>
        /// <param name="conn">连接</param>
        /// <param name="dbTran">事务</param>
        /// <param name="dt">要更新的单表</param>
        /// <returns>更新后的表</returns>
        public override DataTable SaveTable(DataTable dt, DbConnection conn, DbTransaction dbTran)
        {
            try
            {
                StringBuilder strInsertPre = new StringBuilder();
                StringBuilder strInsertEnd = new StringBuilder();
                StringBuilder strUpdate = new StringBuilder();
                StringBuilder strWhere = new StringBuilder();
                StringBuilder strDelete = new StringBuilder();

                /*注：Oracle的InsertCommand命令中的值必须全部使用参数，不能像SQL那样指定为一个函数，
                  否则会报“ORA-01036: 非法的变量名/编号”错误。 */
                string strOraclePartPre = ":p";
                string strOraclePartPreP = "p";
                strInsertPre.Append("INSERT INTO " + dt.TableName + "(");
                strInsertEnd.Append("VALUES(");
                strUpdate.Append("UPDATE " + dt.TableName + " SET ");
                strDelete.Append("DELETE FROM " + dt.TableName);
                //表列信息表
                DataTable dtTableInfo = GetSchemaTableColumns(dt.TableName);
                DataRow[] dcPKList = dtTableInfo.Select(DBColumnEntity.SqlString.KeyType + "='PK'");
                bool isFirstUpdateColumnFind = false;
                string sDouHao = "";
                string sUpdateDouHao = ",";

                foreach (DataColumn dc in dt.Columns)
                {
                    #region 命令SQL构造
                    if (!isFirstUpdateColumnFind)
                    {
                        sUpdateDouHao = "";
                    }
                    else
                    {
                        sUpdateDouHao = ",";
                    }
                    //处理INSERT
                    strInsertPre.Append(sDouHao + dc.ColumnName);
                    strInsertEnd.Append(sDouHao + strOraclePartPre + dc.ColumnName);//全部使用参数形式

                    #region 动态固定值处理
                    if (dc.ExtendedProperties[AutoSQLCoreStaticConstant.FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE] != null)
                    {
                        DbDefaultValueType tcy;
                        try
                        {
                            tcy = (DbDefaultValueType)dc.ExtendedProperties[AutoSQLCoreStaticConstant.FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE];
                        }
                        catch (Exception exTans)
                        {
                            throw new Exception("请保证表列的扩展属性“动态固定值”为TableCoulnmDefaultType枚举类型！" + exTans.Message);
                        }
                        DataTable dtFixValue = new DataTable();
                        if (tcy == DbDefaultValueType.DateTime)
                        {
                            dtFixValue = QueryHadParamSqlData("SELECT SYSDATE FROM DUAL", new Dictionary<string, string>());
                        }
                        else if (tcy == DbDefaultValueType.TimeStamp)
                        {
                            dtFixValue = QueryHadParamSqlData("SELECT SYSTIMESTAMP FROM DUAL", new Dictionary<string, string>());
                        }
                        else if (tcy == DbDefaultValueType.Guid)
                        {
                            dtFixValue = QueryHadParamSqlData("SELECT TO_SINGLE_BYTE(SYS_GUID()) FROM DUAL", new Dictionary<string, string>());
                        }
                        else
                        {
                            throw new Exception("未处理的“动态固定值”枚举类型！");
                        }
                        isFirstUpdateColumnFind = true;
                        //注：因为Oracle的更新语句中不支持函数，所以才采用查询后赋值的方法
                        foreach (DataRow iDr in dt.Rows)
                        {
                            iDr[dc.ColumnName] = dtFixValue.Rows[0][0];
                        }
                    }
                    #endregion

                    //处理UPDATE、DELETE
                    if (dcPKList.Length > 0 && dcPKList[0][DBColumnEntity.SqlString.Name].ToString() != dc.ColumnName)
                    {
                        strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=" + strOraclePartPre + dc.ColumnName);
                        isFirstUpdateColumnFind = true;
                    }
                    else if (dcPKList.Length > 0 && dcPKList[0][DBColumnEntity.SqlString.Name].ToString() == dc.ColumnName)
                    {
                        strWhere.Append(" WHERE  ");
                        for (int i = 0; i < dcPKList.Length; i++)
                        {
                            if (i == 0)
                            {
                                strWhere.Append(dc.ColumnName + "=" + strOraclePartPre + dc.ColumnName);
                            }
                            else
                            {
                                strWhere.Append(" AND " + dc.ColumnName + "=" + strOraclePartPre + dc.ColumnName);
                            }
                        }
                    }
                    #endregion

                    sDouHao = ",";
                }

                strInsertPre.Append(") ");
                strInsertEnd.Append(") ");

                OracleDataAdapter adapter = new OracleDataAdapter();
                OracleConnection con = (OracleConnection)conn;
                //示例："INSERT INTO dbo.t6( com_id ,usr_id ) VALUES( @com_id ,@usr_id)"
                string strInsert = strInsertPre.ToString() + strInsertEnd.ToString();
                adapter.InsertCommand = new OracleCommand(strInsert, con);
                adapter.InsertCommand.Transaction = (OracleTransaction)dbTran;
                adapter.InsertCommand.CommandType = CommandType.Text;

                string strUpdateSql = strUpdate.ToString() + strWhere.ToString();
                adapter.UpdateCommand = new OracleCommand(strUpdateSql, con); //"update t6 set usr_id=@usr_id where com_id=@com_idand usr_id=@usr_id1"
                adapter.UpdateCommand.Transaction = (OracleTransaction)dbTran; ;
                adapter.UpdateCommand.CommandType = CommandType.Text;

                string strDeleteSql = strDelete.ToString() + strWhere.ToString();
                adapter.DeleteCommand = new OracleCommand(strDeleteSql, con); //"delete from t6 where com_id=@com_idand usr_id=@usr_id"
                adapter.DeleteCommand.Transaction = (OracleTransaction)dbTran;
                adapter.DeleteCommand.CommandType = CommandType.Text;
                //先清空命令参数
                adapter.InsertCommand.Parameters.Clear();
                adapter.UpdateCommand.Parameters.Clear();
                adapter.DeleteCommand.Parameters.Clear();
                //构造命令参数
                foreach (DataColumn dc in dt.Columns)
                {
                    DataRow[] drCol = dtTableInfo.Select(DBColumnEntity.SqlString.Name + "='" + dc.ColumnName + "'");
                    Int32 iLen = 0;
                    if (!string.IsNullOrEmpty(drCol[0][DBColumnEntity.SqlString.DataLength].ToString()))
                    {
                        iLen = Int32.Parse(drCol[0][DBColumnEntity.SqlString.DataLength].ToString());
                    }
                    OracleDbType ot = TransToOracleType(drCol[0][DBColumnEntity.SqlString.DataType].ToString());
                    adapter.InsertCommand.Parameters.Add(strOraclePartPreP + dc.ColumnName, ot, iLen, dc.ColumnName);
                    adapter.UpdateCommand.Parameters.Add(strOraclePartPreP + dc.ColumnName, ot, iLen, dc.ColumnName);
                    adapter.DeleteCommand.Parameters.Add(strOraclePartPreP + dc.ColumnName, ot, iLen, dc.ColumnName);
                }
                //主键版本处理
                foreach (DataRow dr in dcPKList)
                {
                    adapter.UpdateCommand.Parameters[strOraclePartPreP + dr[DBColumnEntity.SqlString.Name].ToString()].SourceVersion = DataRowVersion.Original;
                    adapter.DeleteCommand.Parameters[strOraclePartPreP + dr[DBColumnEntity.SqlString.Name].ToString()].SourceVersion = DataRowVersion.Original;
                }
                //更新表
                adapter.Update(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 将数据中的表类型转为OracleType
        /// <summary>
        /// 将数据中的表类型转为OracleType
        /// </summary>
        /// <param name="strOracleDbType">数据库中的类型名称</param>
        /// <returns>OracleType</returns>
        public OracleDbType TransToOracleType(string strOracleDbType)
        {

            /*Oracle数据库类型：
             INTEGER,NUMBER,VARCHAR2,NVARCHAR2,DATE,TIMESTAMP,CHAR,LONG,BINARY_DOUBLE,
             BINARY_FLOAT,BLOB,CLOB,INTERVAL DAY TO SECOND,INTERVAL YEAR TO MONTH,LONG RAW,NCLOB,RAW,
             TIMESTAMP WITH LOCAL TIME ZONE,TIMESTAMP WITH TIME ZONe
             */
            OracleDbType dType = OracleDbType.NVarchar2;
            strOracleDbType = strOracleDbType.ToUpper(); //注：要转换为大写来判断
            #region 文本型
            if (strOracleDbType.ToUpper() == "NVARCHAR2")
            {
                return OracleDbType.NVarchar2;
            }
            else if (strOracleDbType.ToUpper() == "VARCHAR2")
            {
                return OracleDbType.Varchar2;
            }
            else if (strOracleDbType.ToUpper() == "CHAR")
            {
                return OracleDbType.Char;
            }
            #endregion

            #region 数值类
            else if (strOracleDbType.ToUpper() == "NUMBER")
            {
                return OracleDbType.Decimal;
            }
            else if (strOracleDbType.ToUpper() == "INTEGER")
            {
                return OracleDbType.Int32;
            }
            else if (strOracleDbType.ToUpper() == "LONG")
            {
                return OracleDbType.Long;
            }
            else if (strOracleDbType.ToUpper() == "BLOB")
            {
                return OracleDbType.Blob;
            }
            else if (strOracleDbType.ToUpper() == "CLOB")
            {
                return OracleDbType.Clob;
            }
            else if (strOracleDbType.ToUpper() == "INTERVAL DAY TO SECOND")
            {
                return OracleDbType.IntervalDS;
            }
            #endregion

            #region 日期时间类
            else if (strOracleDbType.ToUpper() == "DATE")
            {
                return OracleDbType.Date;
            }
            else if (strOracleDbType.ToUpper() == "TIMESTAMP")
            {
                return OracleDbType.TimeStamp;
            }

            #endregion

            #region 其他
            else if (strOracleDbType.ToUpper() == "INTERVAL YEAR TO MONTH")
            {
                return OracleDbType.IntervalYM;
            }
            else if (strOracleDbType.ToUpper() == "NCLOB")
            {
                return OracleDbType.NClob;
            }
            else if (strOracleDbType.ToUpper() == "RAW")
            {
                return OracleDbType.Raw;
            }
            #endregion

            return dType;
        }
        #endregion     

        #region 生成单号
        public override string GetOrderCode(DbConnection con, DbTransaction tran, string strRuleCode, string strOrgID)
        {
            try
            {
                string sReturnCode = "";

                var ps = new StoreProcedureSqlBuilder(this, "P_COMM_GET_FORM_CODE");
                ps.ListPara.Add(new ProcedureParam(1, "V_ORG_ID", strOrgID, SqlDbType.VarChar, 50));
                ps.ListPara.Add(new ProcedureParam(2, "V_RULE_CODE", strRuleCode, SqlDbType.VarChar, 50));
                ps.ListPara.Add(new ProcedureParam(3, "V_RETURN_CODE", sReturnCode, SqlDbType.VarChar, 50, ProcedureParaInOutType.OutPut));

                object[] objReturn = ps.CallStoredProcedure(con, tran);

                return objReturn[0].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 类型转换
        public OracleDbType DbTypeToOracleType(DbType pSourceType)
        {
            OracleParameter paraConver = new OracleParameter();
            paraConver.DbType = pSourceType;
            return paraConver.OracleDbType;
        }

        public DbType OracleTypeToDbType(OracleDbType pSourceType)
        {
            OracleParameter paraConver = new OracleParameter();
            paraConver.OracleDbType = pSourceType;
            return paraConver.DbType;
        }
        #endregion

        #region 增加Oracle参数
        public override DbParameter AddParam(string[] sParameterArr, DbCommand dbCommand, DbParameter oracleParameter, int i, string[] sProperties, string parameterCode, string executeType, string parameterType)
        {
            OracleCommand sqlCommand = dbCommand as OracleCommand;
            if (parameterType == "NUMBER") // 数字类型
            {
                oracleParameter = sqlCommand.Parameters.Add(parameterCode, OracleDbType.Decimal);
                oracleParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "VARCHAR2") // 字符串类型
            {
                oracleParameter = sqlCommand.Parameters.Add(parameterCode, OracleDbType.Varchar2, int.Parse(sProperties[3]));
                oracleParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "NVARCHAR2")
            {
                oracleParameter = sqlCommand.Parameters.Add(parameterCode, OracleDbType.NVarchar2, int.Parse(sProperties[3]));
                oracleParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "DATETIME") // 时间类型
            {
                oracleParameter = sqlCommand.Parameters.Add(parameterCode, OracleDbType.Date);
                oracleParameter.Value = DateTime.Parse(sParameterArr[i]);
            }
            else if (parameterType == "CURSOR")
            {
                oracleParameter = sqlCommand.Parameters.Add(parameterCode, OracleDbType.RefCursor);
            }
            else
            {
                throw new Exception("未定义的存储过程的参数类型：" + executeType);
            }
            return oracleParameter;
        }
        #endregion

        #region 设置存储过程命令
        public override void SetProdureCommond(string sSql, DbConnection con, DbTransaction tran, out DbCommand dbCommand, out DbDataAdapter adpater)
        {
            dbCommand = new OracleCommand(sSql, (OracleConnection)con);

            if (tran != null)
            {
                dbCommand.Transaction = (OracleTransaction)tran;
            }

            adpater = new OracleDataAdapter();
        }
        #endregion

        #region 通过连接对象获取数据库元数据信息
        /// <summary>
        /// 获取数据库架构信息
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="restrictionValues"></param>
        /// <returns></returns>
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            using (OracleConnection con = (OracleConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable schema = con.GetSchema(collectionName, restrictionValues);
                return schema;
            }
        }

        /// <summary>
        /// 获取数据库清单（用户清单）
        /// </summary>
        /// <returns></returns>
        public override DataTable GetDataBases()
        {
            using (OracleConnection con = (OracleConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                //Oracle没有DBSchemaString.Databases；所以后面直接使用用户名作为树的根节点
                //DataTable dtSource = con.GetSchema(DBSchemaString.Databases, null);
                //返回标准的结果表
                DataTable dtReturn = DT_DataBase;
                DataRow dr = dtReturn.NewRow();
                dr[DBDataBaseEntity.SqlString.Name] = DbServer.UserName;
                dtReturn.Rows.Add(dr);
                return dtReturn;
            }
        }

        /// <summary>
        /// 获取用户表清单
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTables(string sTableName = null, string sSchema = null)
        {
            using (OracleConnection con = (OracleConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable res = con.GetSchema(DBSchemaString.Restrictions);//查询GetSchema第二个参数的含义说明
                DataTable dtTables = con.GetSchema(DBSchemaString.Tables, new string[] { sSchema, sTableName });
                DataRow[] arArr = dtTables.Select("TYPE='User'");
                //返回标准的结果表
                DataTable dtReturn = DT_SchemaTable;
                foreach (DataRow drS in arArr)
                {
                    DataRow dr = dtReturn.NewRow();
                    //dr[DBTableEntity.SqlString.Schema] = drS["OWNER"];
                    dr[DBTableEntity.SqlString.Name] = drS[OracleSchemaString.Table.TableName];
                    dr[DBTableEntity.SqlString.NameUpper] = drS[OracleSchemaString.Table.TableName].ToString().FirstLetterUpper();
                    dr[DBTableEntity.SqlString.NameLower] = drS[OracleSchemaString.Table.TableName].ToString().FirstLetterUpper(false);
                    //SqlSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
                    dr[DBTableEntity.SqlString.Owner] = drS[OracleSchemaString.Table.Owner];

                    dtReturn.Rows.Add(dr);
                }
                return dtReturn;
            }
        }

        /// <summary>
        /// 查询DB表元数据信息
        /// </summary>
        /// <param name="sTableName">表名</param>
        /// <returns></returns>
        public override DataTable GetSchemaTableColumns(string sTableName)
        {
            using (OracleConnection con = (OracleConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                //单独处理主键
                string sSql = @"SELECT A.CONSTRAINT_NAME,A.COLUMN_NAME 
                FROM USER_CONS_COLUMNS A JOIN USER_CONSTRAINTS B 
                     ON A.CONSTRAINT_NAME = B.CONSTRAINT_NAME 
                WHERE B.CONSTRAINT_TYPE = 'P' AND A.TABLE_NAME ='#TABLE_NAME#'
                ";
                IDictionary<string, string> dic = new Dictionary<string, string>();
                dic["TABLE_NAME"] = sTableName;
                DataTable dtPK = QueryAutoParamSqlData(sSql, dic);
                bool isPKOK = false;

                DataTable dtSource = con.GetSchema(DBSchemaString.Columns, new string[] { null, sTableName, null });//使用通用的获取架构方法
                DataTable dtReturn = DT_SchemaTableColumn;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBColumnEntity.SqlString.TableSchema] = drS[OracleSchemaString.Column.TableSchema];//Schema跟数据库名称一样
                    dr[DBColumnEntity.SqlString.TableName] = drS[OracleSchemaString.Column.TableName];
                    dr[DBColumnEntity.SqlString.TableNameUpper] = drS[OracleSchemaString.Column.TableName].ToString().FirstLetterUpper();
                    dr[DBColumnEntity.SqlString.TableNameLower] = drS[OracleSchemaString.Column.TableName].ToString().FirstLetterUpper(false);
                    dr[DBColumnEntity.SqlString.SortNum] = drS[OracleSchemaString.Column.OrdinalPosition];
                    dr[DBColumnEntity.SqlString.Name] = drS[OracleSchemaString.Column.ColumnName];
                    dr[DBColumnEntity.SqlString.NameUpper] = drS[OracleSchemaString.Column.ColumnName].ToString().FirstLetterUpper();
                    dr[DBColumnEntity.SqlString.NameLower] = drS[OracleSchemaString.Column.ColumnName].ToString().FirstLetterUpper(false);
                    //dr[SqlColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"];
                    //dr[SqlColumnEntity.SqlString.Default] = drS["COLUMN_DEFAULT"];
                    dr[DBColumnEntity.SqlString.NotNull] = drS[OracleSchemaString.Column.IsNullable].ToString().ToUpper().Equals("N") ? "1" : "";
                    dr[DBColumnEntity.SqlString.DataType] = drS[OracleSchemaString.Column.DataType];
                    dr[DBColumnEntity.SqlString.DataLength] = drS[OracleSchemaString.Column.CharacterMaximumLength];
                    dr[DBColumnEntity.SqlString.DataPrecision] = drS[OracleSchemaString.Column.Numeric_Precision];
                    dr[DBColumnEntity.SqlString.DataScale] = drS[OracleSchemaString.Column.Numeric_Scale];
                    //dr[SqlColumnEntity.SqlString.DataTypeFull] = drS["COLUMN_TYPE"];
                    //dr[SqlColumnEntity.SqlString.NameCN] = drS["COLUMN_CN"];
                    //dr[SqlColumnEntity.SqlString.Extra] = drS["COLUMN_EXTRA"];
                    if (!isPKOK)
                    {
                        DataRow[] arrPK = dtPK.Select("COLUMN_NAME='" + drS[OracleSchemaString.Column.ColumnName] + "'");
                        if (arrPK.Length > 0)
                        {
                            dr[DBColumnEntity.SqlString.KeyType] = "PK";//主键信息
                            isPKOK = true;
                        }
                    }
                    dtReturn.Rows.Add(dr);
                }
                return dtReturn;
            }
        }
        #endregion

        #region 通过SQL语句获取数据库元数据信息
        public override DataTable GetSqlSchemaTables(string sTableName = null, string sSchema = null)
        {
            if (string.IsNullOrEmpty(sSchema))
            {
                sSchema = DbServer.UserName; //TABLE_SCHEMA为登录的用户。登录的用户其创建的对象为其所有。
            }
            //注：即使创建表语句表名或字段名为小写，但Oracle自动会转换为大写。加上视图
            string sSql = @"SELECT A.OWNER AS TABLE_SCHEMA, A.TABLE_NAME,B.COMMENTS AS TABLE_COMMENT,A.TABLESPACE_NAME,'0' AS TABLE_IS_VIEW
                    FROM ALL_TABLES A
                    LEFT JOIN ALL_TAB_COMMENTS B
                      ON A.TABLE_NAME = B.TABLE_NAME AND A.OWNER=B.OWNER
                    WHERE 1=1
                    AND A.TABLE_NAME = '#TABLE_NAME#'
                    AND A.OWNER = '#TABLE_SCHEMA#'
                    UNION ALL
                    SELECT A.OWNER AS TABLE_SCHEMA, A.OBJECT_NAME AS TABLE_NAME,'' AS TABLE_COMMENT,'' AS TABLESPACE_NAME,'1' AS TABLE_IS_VIEW
                    FROM ALL_OBJECTS A
                    WHERE 1=1
                     AND A.OBJECT_TYPE = 'VIEW'
                     AND A.OWNER = '#TABLE_SCHEMA#'
                     AND A.OBJECT_NAME = '#TABLE_NAME#'
                ";

            IDictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(sSchema))
            {
                dic["TABLE_SCHEMA"] = sSchema.ToUpper();
            }
            if (!string.IsNullOrEmpty(sTableName))
            {
                dic["TABLE_NAME"] = sTableName.ToUpper();
            }

            DataTable dtSource = QueryAutoParamSqlData(sSql, dic);
            DataTable dtReturn = DT_SchemaTable;
            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();

                dr[DBTableEntity.SqlString.Name] = drS["TABLE_NAME"];
                dr[DBTableEntity.SqlString.NameUpper] = drS["TABLE_NAME"].ToString().FirstLetterUpper();
                dr[DBTableEntity.SqlString.NameLower] = drS["TABLE_NAME"].ToString().FirstLetterUpper(false);
                DBSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
                if (drS.ContainsColumn("TABLE_SCHEMA"))
                {
                    dr[DBTableEntity.SqlString.Schema] = drS["TABLE_SCHEMA"];
                    dr[DBTableEntity.SqlString.Owner] = drS["TABLE_SCHEMA"];
                    dr[DBTableEntity.SqlString.DBName] = drS["TABLE_SCHEMA"];
                }
                dr[DBTableEntity.SqlString.IsView] = drS["TABLE_IS_VIEW"]; //是否视图
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        public override DataTable GetSqlSchemaTableColumns(string sTableName, string sSchema = null)
        {
            List<string> listTableName = new List<string>
            {
                sTableName
            };
            return GetSqlSchemaTableColumns(listTableName, sSchema);
        }

        public override DataTable GetSqlSchemaTableColumns(List<string> listTableName, string sSchema = null)
        {

            /* 默认值不能直接取all_tab_columns.data_default (类型为LONG)，要用以下SQL取：
             *  CASE WHEN DEFAULT_LENGTH IS NULL THEN '' ELSE
             *  extractvalue(dbms_xmlgen.getxmltype( 'select data_default from user_tab_columns where table_name = ''' || A.table_name || ''' and column_name = ''' || A.column_name || '''' ), '//text()' )
             *       END AS COLUMN_DEFAULT,
             *  以上取法会导致查询很慢，且如默认值有&符号会报错。也可以改为下面方式：
             *  CASE WHEN DEFAULT_LENGTH IS NULL THEN '' ELSE
             *  dbms_xmlgen.getxmltype( 'select data_default from user_tab_columns where table_name = ''' || A.table_name || ''' and column_name = ''' || A.column_name || '''' ).getstringval()
             *       END AS COLUMN_DEFAULT,
             *  虽然不报错了，默认值字符需要进一步截取，但速度还是很慢，所以最终决定先不查默认值，等后面有需要再单独查默认值。 by BreezeeHui 2024-3-16    
             */
            string sSql = @"SELECT A.OWNER AS TABLE_SCHEMA,
                NVL(A.TABLE_NAME,V.OBJECT_NAME) AS TABLE_NAME,
                A.COLUMN_ID AS ORDINAL_POSITION,
                A.COLUMN_NAME,
                B.COMMENTS AS COLUMN_COMMENT,
                A.DATA_TYPE,
                A.DATA_LENGTH AS CHARACTER_MAXIMUM_LENGTH,
                A.DATA_PRECISION AS NUMERIC_PRECISION,
                A.DATA_SCALE AS NUMERIC_SCALE,
                A.NULLABLE AS IS_NULLABLE,
                '' AS COLUMN_DEFAULT,
                DECODE(PK.COLUMN_NAME,NULL,0,1)  AS COLUMN_KEY,
                C.COMMENTS AS TABLE_COMMENT,
                A.OWNER AS TABLE_OWNER
            FROM ALL_TAB_COLS A
            LEFT JOIN ALL_COL_COMMENTS B 
                ON A.TABLE_NAME=B.TABLE_NAME AND A.COLUMN_NAME=B.COLUMN_NAME AND A.OWNER=B.OWNER
            LEFT JOIN ALL_TABLES T
                ON A.TABLE_NAME = T.TABLE_NAME AND A.OWNER=T.OWNER  
            LEFT JOIN ALL_OBJECTS V
                ON V.OBJECT_TYPE = 'VIEW' AND V.OWNER = A.OWNER AND V.OBJECT_NAME = A.TABLE_NAME
            LEFT JOIN ALL_TAB_COMMENTS C
                ON A.TABLE_NAME = C.TABLE_NAME AND A.OWNER = C.OWNER
            LEFT JOIN (
                SELECT AA.OWNER,AA.TABLE_NAME,BB.COLUMN_NAME 
                FROM ALL_CONSTRAINTS AA
                LEFT JOIN ALL_CONS_COLUMNS BB ON AA.CONSTRAINT_NAME=BB.CONSTRAINT_NAME AND AA.OWNER=BB.OWNER
                WHERE AA.CONSTRAINT_TYPE='P' 
                ) PK ON PK.TABLE_NAME = A.TABLE_NAME AND PK.COLUMN_NAME = A.COLUMN_NAME AND PK.OWNER=A.OWNER    
            WHERE 1=1
                AND A.TABLE_NAME IN (#TABLE_NAME_LIST:LS#)
                AND A.TABLE_NAME = '#TABLE_NAME#'
                AND A.OWNER = '#TABLE_SCHEMA#'
            ORDER BY A.TABLE_NAME,A.COLUMN_ID
            ";
            //移除所有表名为空的
            listTableName.RemoveAll(t => string.IsNullOrEmpty(t));
            /*注：即使创建表语句表名或字段名为小写，但Oracle自动会转换为大写*/
            IDictionary<string, object> dic = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(sSchema))
            {
                sSchema = DbServer.UserName.ToUpper(); //注：oracle中只能查自己用户下的所有表。不加该条件会把系统表都会查出来
            }
            dic["TABLE_SCHEMA"] = sSchema;

            if (listTableName.Count == 0)
            {
                return GetColumnTable(sSql, dic);
            }
            else if (listTableName.Count == 1)
            {
                dic["TABLE_NAME"] = listTableName[0].ToUpper();
                return GetColumnTable(sSql, dic);
            }
            else if (listTableName.Count < MaxInStringCount)
            {
                //将所有表名转换为大写
                List<string> listTableNameNew = new List<string>();
                for (int i = 0; i < listTableName.Count; i++)
                {
                    listTableNameNew.Add(listTableName[i].ToUpper());
                }
                dic["TABLE_NAME_LIST"] = listTableNameNew;
                return GetColumnTable(sSql, dic);
            }
            else
            {
                //分段IN查询并合并
                List<string> listTableNameNew = new List<string>();
                DataTable dtReturn = DT_SchemaTableColumn;
                for (int i = 0; i < listTableName.Count; i++)
                {
                    listTableNameNew.Add(listTableName[i].ToUpper());
                    if (i % MaxInStringCount == 0)
                    {
                        dic["TABLE_NAME_LIST"] = listTableNameNew;
                        DataTable dtQuery = GetColumnTable(sSql, dic);
                        dtReturn.CopyExistColumnData(dtQuery);
                        listTableNameNew.Clear();
                    }
                }
                if (listTableNameNew.Count > 0)
                {
                    dic["TABLE_NAME_LIST"] = listTableNameNew;
                    DataTable dtQuery = GetColumnTable(sSql, dic);
                    dtReturn.CopyExistColumnData(dtQuery);
                }
                return dtReturn;
            }
        }

        private DataTable GetColumnTable(string sSql, IDictionary<string, object> dic)
        {
            DataTable dtSource = QueryAutoParamSqlData(sSql, dic);
            DataTable dtReturn = DT_SchemaTableColumn;
            //所有表相关字段的变量
            string sPreTableName = string.Empty;
            string sPreTableSchema = string.Empty;
            string sPreTableNameUpper = string.Empty;
            string sPreTableNameLower = string.Empty;
            string sPreTableRemark = string.Empty;
            string sPreTableExt = string.Empty;

            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();

                //先前表名为空，或先前表名跟现在行表名不一致时，才重新对表字段变量赋值
                if (string.IsNullOrEmpty(sPreTableName) || !sPreTableName.Equals(drS["TABLE_NAME"].ToString()))
                {
                    sPreTableName = drS["TABLE_NAME"].ToString();
                    sPreTableSchema = drS["TABLE_SCHEMA"].ToString();//Schema跟数据库名称一样
                    sPreTableNameUpper = drS["TABLE_NAME"].ToString().FirstLetterUpper();
                    sPreTableNameLower = drS["TABLE_NAME"].ToString().FirstLetterUpper(false);
                    DBSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
                    sPreTableRemark = dr[DBColumnEntity.SqlString.TableComments].ToString();
                    sPreTableExt = dr[DBColumnEntity.SqlString.TableExtra].ToString();
                }
                dr[DBColumnEntity.SqlString.TableSchema] = sPreTableSchema;
                dr[DBColumnEntity.SqlString.TableName] = sPreTableName;
                dr[DBColumnEntity.SqlString.TableNameUpper] = sPreTableNameUpper;
                dr[DBColumnEntity.SqlString.TableNameLower] = sPreTableNameLower;
                dr[DBColumnEntity.SqlString.TableComments] = sPreTableRemark;
                dr[DBColumnEntity.SqlString.TableExtra] = sPreTableExt;
                dr[DBColumnEntity.SqlString.Owner] = drS["TABLE_OWNER"]; //拥有者

                dr[DBColumnEntity.SqlString.SortNum] = drS["ORDINAL_POSITION"];
                dr[DBColumnEntity.SqlString.Name] = drS["COLUMN_NAME"];
                dr[DBColumnEntity.SqlString.NameUpper] = drS["COLUMN_NAME"].ToString().FirstLetterUpper();
                dr[DBColumnEntity.SqlString.NameLower] = drS["COLUMN_NAME"].ToString().FirstLetterUpper(false);
                dr[DBColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"];
                dr[DBColumnEntity.SqlString.Default] = drS["COLUMN_DEFAULT"].ToString();
                dr[DBColumnEntity.SqlString.NotNull] = drS["IS_NULLABLE"].ToString().ToUpper().Equals("N") ? "1" : "";
                dr[DBColumnEntity.SqlString.DataType] = drS["DATA_TYPE"];
                string sPrecision = drS["NUMERIC_PRECISION"].ToString();
                if (!string.IsNullOrEmpty(sPrecision))
                {
                    dr[DBColumnEntity.SqlString.DataLength] = sPrecision;
                    dr[DBColumnEntity.SqlString.DataPrecision] = sPrecision;
                }
                else
                {
                    dr[DBColumnEntity.SqlString.DataLength] = drS["CHARACTER_MAXIMUM_LENGTH"];
                    dr[DBColumnEntity.SqlString.DataPrecision] = drS["NUMERIC_PRECISION"];
                }
                dr[DBColumnEntity.SqlString.DataScale] = drS["NUMERIC_SCALE"];
                dr[DBColumnEntity.SqlString.KeyType] = "1".Equals(drS["COLUMN_KEY"].ToString().ToUpper()) ? "PK" : "";
                DBSchemaCommon.SetComment(dr, drS["COLUMN_COMMENT"].ToString(), false);
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        /// <summary>
        /// 查询表字段默认值
        /// </summary>
        /// <param name="listTableName"></param>
        /// <param name="sSchema"></param>
        /// <param name="sSql"></param>
        /// <returns></returns>
        public override DataTable GetSqlTableColumnsDefaultValue(List<string> listTableName, string sSchema = null)
        {
            //当默认值中有&号时，如'11 & 22',会报错而进入以下代码。
            /* 下面为SQL：dbms_xmlgen.getxmltype(xxSql).getStringVal()得到的数据示例：
             *   "<ROWSET><ROW><DATA_DEFAULT>sys_guid() </DATA_DEFAULT></ROW></ROWSET>"
             *   "<ROWSET> <ROW> <DATA_DEFAULT>'CHIAN'</DATA_DEFAULT></ROW></ROWSET>"
             *   "<ROWSET> <ROW>  <DATA_DEFAULT>2 </DATA_DEFAULT> </ROW></ROWSET>"
             *   "<ROWSET> <ROW>  <DATA_DEFAULT>'11 & 22'</DATA_DEFAULT> </ROW></ROWSET>"
             */
            string sSql = @"SELECT A.OWNER AS TABLE_SCHEMA,
    A.TABLE_NAME,
    A.COLUMN_NAME,
    dbms_xmlgen.getxmltype('select data_default from user_tab_columns where table_name = ''' || A.table_name || ''' and column_name = ''' || A.column_name || '''' ).getstringval() AS COLUMN_DEFAULT
FROM ALL_TAB_COLS A   
WHERE 1=1
      AND A.TABLE_NAME IN (#TABLE_NAME_LIST:LS#)
      AND A.TABLE_NAME = '#TABLE_NAME#'
      AND A.OWNER = '#TABLE_SCHEMA#'
      AND DEFAULT_LENGTH > 0
            ";
            //移除所有表名为空的
            listTableName.RemoveAll(t => string.IsNullOrEmpty(t));
            DataTable dtSource;
            /*注：即使创建表语句表名或字段名为小写，但Oracle自动会转换为大写*/
            IDictionary<string, object> dic = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(sSchema))
            {
                sSchema = DbServer.UserName.ToUpper(); //注：oracle中只能查自己用户下的所有表。不加该条件会把系统表都会查出来
            }
            dic["TABLE_SCHEMA"] = sSchema;

            if (listTableName.Count == 0)
            {
                dtSource = GetColumnDefault(sSql, dic);
            }
            else if (listTableName.Count == 1)
            {
                dic["TABLE_NAME"] = listTableName[0].ToUpper();
                dtSource = GetColumnDefault(sSql, dic);
            }
            else if (listTableName.Count < MaxInStringCount)
            {
                //将所有表名转换为大写
                List<string> listTableNameNew = new List<string>();
                for (int i = 0; i < listTableName.Count; i++)
                {
                    listTableNameNew.Add(listTableName[i].ToUpper());
                }
                dic["TABLE_NAME_LIST"] = listTableNameNew;
                dtSource = GetColumnDefault(sSql, dic);
            }
            else
            {
                //分段IN查询并合并
                List<string> listTableNameNew = new List<string>();
                DataTable dtReturn = DT_SchemaTableColumn;
                for (int i = 0; i < listTableName.Count; i++)
                {
                    listTableNameNew.Add(listTableName[i].ToUpper());
                    if (i % MaxInStringCount == 0)
                    {
                        dic["TABLE_NAME_LIST"] = listTableNameNew;
                        DataTable dtQuery = GetColumnDefault(sSql, dic);
                        dtReturn.CopyExistColumnData(dtQuery);
                        listTableNameNew.Clear();
                    }
                }
                if (listTableNameNew.Count > 0)
                {
                    dic["TABLE_NAME_LIST"] = listTableNameNew;
                    DataTable dtQuery = GetColumnDefault(sSql, dic);
                    dtReturn.CopyExistColumnData(dtQuery);
                }
                dtSource = dtReturn;
            }

            foreach (DataRow drS in dtSource.Rows)
            {
                string sDefaultXml = drS["COLUMN_DEFAULT"].ToString();
                //默认值的字符截取处理
                string sBegin = "<DATA_DEFAULT>";
                string sEnd = "</DATA_DEFAULT>";
                int iStart = sDefaultXml.IndexOf(sBegin);
                int iEnd = sDefaultXml.IndexOf(sEnd);
                string sDefault = string.Empty;
                if (iStart > 0)
                {
                    sDefault = sDefaultXml.Substring(iStart + sBegin.Length, iEnd - iStart - sBegin.Length).Trim();
                }
                drS[DBColumnEntity.SqlString.Default] = sDefault;
            }
            return dtSource;
        }

        /// <summary>
        /// 查询列的默认值信息
        /// </summary>
        /// <param name="listTableName"></param>
        /// <param name="sSchema"></param>
        /// <param name="sSql"></param>
        /// <returns></returns>
        private DataTable GetColumnDefault(string sSql, IDictionary<string, object> dic)
        {
            DataTable dtSource = QueryAutoParamSqlData(sSql, dic);
            return dtSource;
        }
        #endregion
    }
}
