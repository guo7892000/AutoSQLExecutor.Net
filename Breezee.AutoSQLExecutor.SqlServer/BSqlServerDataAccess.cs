using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Data.Common;
using System.Xml;
using Breezee.AutoSQLExecutor.Core;
using Breezee.Core.Interface;
//using System.Data.SqlClient;   //net4引入此空间
using Microsoft.Data.SqlClient;   //net6引入此空间

namespace Breezee.AutoSQLExecutor.SqlServer
{
    /// <summary>
    /// SqlServer数据访问实现层
    /// </summary>
    public class BSqlServerDataAccess : IDataAccess
    {
        #region 属性
        public override DataBaseType DataBaseType
        {
            get { return DataBaseType.SqlServer; }
        }

        private string _ConnectionString;
        public override string ConnectionString
        {
            get { return _ConnectionString; }
            protected set { _ConnectionString = value; }
        }

        ISqlDifferent _SqlDiff = new BSqlServerSqlDifferent();
        public override ISqlDifferent SqlDiff { get => _SqlDiff; protected set => _SqlDiff = value; }

        public override List<string> CharLengthTypes { get => (new string[] { SqlServerColumnType.Text.Char, SqlServerColumnType.Text.nchar, SqlServerColumnType.Text.varchar,
        SqlServerColumnType.Text.nvarchar}).ToList(); }
        public override List<string> PrecisonTypes { get => (new string[] { SqlServerColumnType.Precision.Decimal, SqlServerColumnType.Precision.numeric }).ToList(); }
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sConstr">连接字符串：注名字要跟Main.config配置文件中的连接字符串配置字符保持一致</param>
        public BSqlServerDataAccess(string sConstr) : base(sConstr)
        {
            _ConnectionString = sConstr;
            SqlParsers.properties.ParamPrefix = "@"; //注：SqlServer是使用@作为参数前缀
        }
        public BSqlServerDataAccess(DbServerInfo server) : base(server)
        {
            SqlParsers.properties.ParamPrefix = "@"; //注：SqlServer是使用@作为参数前缀
        }
        #endregion

        #region 创建SQL Server连接
        /// <summary>
        /// 创建SQL Server连接
        /// </summary>
        /// <returns></returns>
        public override DbConnection GetCurrentConnection()
        {
            return new SqlConnection(_ConnectionString);
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
                else if (item.Key.Equals("initial catalog"))
                {
                    server.Database = item.Value;
                }
                else if (item.Key.Equals("user id"))
                {
                    server.UserName = item.Value;
                }
                else if (item.Key.Equals("password"))
                {
                    server.Password = item.Value;
                }
                else
                {
                    
                }
            }
            return server;
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
            /* data source为数据库实例名，initial catalog为数据库名，user id为用户名，password为密码。连接字符串示例：data source=.;initial catalog=AprilSpring;user id=sa;password=sa 
             * SqlServer好像不需要指定端口，即使后台修改了默认的1433端口，也可以连接成功
             * 针对【provider：SSL Provider，error：0 - 证书链是由不受信任的颁发机构颁发的】报错，要在连接中加上：Encrypt=True;TrustServerCertificate=True;
             */
            _ConnectionString = server.UseConnString ? server.ConnString : string.Format("data source={0};user id={1};password={2};Encrypt=True;TrustServerCertificate=True;", server.ServerName, server.UserName, server.Password);
            //if (!string.IsNullOrEmpty(server.PortNo))
            //{
            //    _ConnectionString = server.UseConnString ? server.ConnString : string.Format("data source={0};user id={1};password={2}", server.ServerName + "," + server.PortNo, server.UserName, server.Password);
            //}
            if (!server.UseConnString && !string.IsNullOrEmpty(server.Database))
            {
                _ConnectionString += string.Format("Initial Catalog={0};", server.Database);
            }
            this.DbServer = server;
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
                if (listParam == null)
                {
                    listParam = new List<FuncParam>();
                }
                //构造命令
                SqlCommand sqlCommon;
                if (dbTran == null)
                {
                    sqlCommon = new SqlCommand(sHadParaSql, (SqlConnection)conn);
                }
                else
                {
                    sqlCommon = new SqlCommand(sHadParaSql, (SqlConnection)conn, (SqlTransaction)dbTran);
                }
                //构造适配器
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommon);
                if (listParam != null)
                {
                    foreach (FuncParam item in listParam)
                    {
                        SqlParameter sp = new SqlParameter(item.Code, item.Value);
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
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 获取分页后的SQL
        public override string GetPageSql(string sHadParaSql, PageParam pParam, int beginRow, int endRow)
        {
            string pageDataSql = "SELECT ROW_NUMBER() OVER (ORDER BY MYTABLE." + pParam.PageOrderString + ") AS ROWNUM, * FROM ( " + sHadParaSql + " ) AS MYTABLE ";
            pageDataSql = "SELECT ROWNUM AS ROWNUM, MYTABLE.*" + " FROM (" + pageDataSql + ") MYTABLE WHERE ROWNUM BETWEEN " +
                       beginRow.ToString() + " AND " + endRow.ToString();
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
            SqlCommand sqlCommon;
            if (dbTran == null)
            {
                sqlCommon = new SqlCommand(sHadParaSql, (SqlConnection)conn);
            }
            else
            {
                sqlCommon = new SqlCommand(sHadParaSql, (SqlConnection)conn, (SqlTransaction)dbTran);
            }

            if (listParam != null)
            {
                foreach (FuncParam item in listParam)
                {
                    SqlParameter sp = new SqlParameter(item.Code, item.Value);
                    if (item.FuncParamType == FuncParamType.DateTime)
                    {
                        sp.DbType = DbType.DateTime;
                    }
                    sqlCommon.Parameters.Add(sp);
                }
            }
            
            return sqlCommon.ExecuteNonQuery();
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
        public override DataTable SaveTable(DataTable dt,DbConnection conn, DbTransaction dbTran)
        {
            try
            {
                StringBuilder strInsertPre = new StringBuilder();
                StringBuilder strInsertEnd = new StringBuilder();
                StringBuilder strUpdate = new StringBuilder();
                StringBuilder strWhere = new StringBuilder();
                StringBuilder strDelete = new StringBuilder();
                
                strInsertPre.Append("INSERT INTO " + dt.TableName + "(");
                strInsertEnd.Append("VALUES(");
                strUpdate.Append("UPDATE " + dt.TableName + " SET ");
                strDelete.Append("DELETE FROM " + dt.TableName);
                //查询数据中的表信息
                DataTable dtTableInfo = GetSchemaTableColumns(dt.TableName);
                DataRow[] dcPKList = dtTableInfo.Select(DBColumnEntity.SqlString.KeyType+"='PK'");
                string strSqlParaPre = _SqlDiff.ParamPrefix;
                bool isFirstUpdateColumnFind = false;
                string sDouHao = "";
                string sUpdateDouHao = ",";
                foreach (DataColumn dc in dt.Columns)
                {
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

                    #region 动态固定值处理
                    /*注：SQL Server处理InsertCommand命令SQL时，支持以函数代替参数。
                        * 即例如：@CREATE_TIME 可被 getdate() 替代*/
                    if (dc.ExtendedProperties[StaticConstant.FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE] != null)
                    {
                        DbDefaultValueType tcy;
                        try
                        {
                            tcy = (DbDefaultValueType)dc.ExtendedProperties[StaticConstant.FRA_TABLE_EXTEND_PROPERTY_COLUMNS_FIX_VALUE];
                        }
                        catch (Exception exTans)
                        {
                            throw new Exception("请保证表列的扩展属性“动态固定值”为TableCoulnmDefaultType枚举类型！" + exTans.Message);
                        }
                        if (tcy == DbDefaultValueType.DateTime)
                        {
                            strInsertEnd.Append(sDouHao + "getdate()");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=getdate()");
                        }
                        else if (tcy == DbDefaultValueType.TimeStamp)
                        {
                            strInsertEnd.Append(sDouHao + "@@DBTS");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=@@DBTS");
                        }
                        else if (tcy == DbDefaultValueType.Guid)
                        {
                            strInsertEnd.Append(sDouHao + "NEWID()");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=NEWID()");
                        }
                        else
                        {
                            throw new Exception("未处理的“动态固定值”枚举类型！");
                        }
                        isFirstUpdateColumnFind = true;
                    }
                    else
                    {
                        strInsertEnd.Append(sDouHao + strSqlParaPre + dc.ColumnName);
                        if (dcPKList.Length > 0 && dcPKList[0][DBColumnEntity.SqlString.Name].ToString() != dc.ColumnName)
                        {
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=" + strSqlParaPre + dc.ColumnName);
                            isFirstUpdateColumnFind = true;
                        }
                    }
                    #endregion

                    sDouHao = ",";
                }
                //新增语句加上右括号
                strInsertPre.Append(") ");
                strInsertEnd.Append(") ");
                //对WHERE 条件的处理
                if (dcPKList.Length > 0)
                {
                    strWhere.Append(" WHERE  ");
                    for (int i = 0; i < dcPKList.Length; i++)
                    {
                        string sPKColumn = dcPKList[i]["COLUMN_NAME"].ToString();
                        if (i == 0)
                        {
                            strWhere.Append(sPKColumn + "=" + strSqlParaPre + sPKColumn);
                        }
                        else
                        {
                            strWhere.Append(" AND " + sPKColumn + "=" + strSqlParaPre + sPKColumn);
                        }
                    }
                }
                else
                {
                    throw new Exception("表" + dt.TableName + "没有主键，不能保存！");
                }
                SqlDataAdapter adapter = new SqlDataAdapter();
                SqlConnection con = (SqlConnection)conn;
                //"INSERT INTO dbo.t6( com_id ,usr_id ) VALUES( @com_id ,@usr_id)"
                string strInsert = strInsertPre.ToString() + strInsertEnd.ToString();
                adapter.InsertCommand = new SqlCommand(strInsert, con, (SqlTransaction)dbTran);
                adapter.InsertCommand.CommandType = CommandType.Text;

                string strUpdateSql = strUpdate.ToString() + strWhere.ToString();
                adapter.UpdateCommand = new SqlCommand(strUpdateSql, con, (SqlTransaction)dbTran); //"update t6 set usr_id=@usr_id where com_id=@com_idand usr_id=@usr_id1"
                adapter.UpdateCommand.CommandType = CommandType.Text;

                string strDeleteSql = strDelete.ToString() + strWhere.ToString();
                adapter.DeleteCommand = new SqlCommand(strDeleteSql, con, (SqlTransaction)dbTran); //"delete from t6 where com_id=@com_idand usr_id=@usr_id"
                adapter.DeleteCommand.CommandType = CommandType.Text;

                foreach (DataColumn dc in dt.Columns)
                {
                    DataRow[] drCol = dtTableInfo.Select(DBColumnEntity.SqlString.Name + "='" + dc.ColumnName + "'");
                    Int32 iLen = 0;
                    if (!string.IsNullOrEmpty(drCol[0][DBColumnEntity.SqlString.DataLength].ToString()))
                    {
                        iLen = Int32.Parse(drCol[0][DBColumnEntity.SqlString.DataLength].ToString());
                    }
                    SqlDbType ot = TransToSqlType(drCol[0][DBColumnEntity.SqlString.DataType].ToString().ToUpper());
                    adapter.InsertCommand.Parameters.Add(strSqlParaPre + dc.ColumnName, ot, iLen, dc.ColumnName);
                    adapter.UpdateCommand.Parameters.Add(strSqlParaPre + dc.ColumnName, ot, iLen, dc.ColumnName);
                    adapter.DeleteCommand.Parameters.Add(strSqlParaPre + dc.ColumnName, ot, iLen, dc.ColumnName);
                }
                //主键版本处理
                foreach (DataRow dr in dcPKList)
                {
                    adapter.UpdateCommand.Parameters[strSqlParaPre + dr[DBColumnEntity.SqlString.Name].ToString()].SourceVersion = DataRowVersion.Original;
                    adapter.DeleteCommand.Parameters[strSqlParaPre + dr[DBColumnEntity.SqlString.Name].ToString()].SourceVersion = DataRowVersion.Original;
                }
                adapter.Update(dt);
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 
        #endregion

        #region 将数据中的表类型转为SqlDbType
        /// <summary>
        /// 将数据中的表类型转为SqlDbType
        /// </summary>
        /// <param name="strSqlDbType">数据库中的类型名称</param>
        /// <returns>DbType</returns>
        public SqlDbType TransToSqlType(string strSqlDbType)
        {
            /*SqlServer数据库类型：
             INT,BIGINT,BIT,VARCHAR,DATETIME,DECIMAL,NVARCHAR,CHAR,NCHAR,NUMERIC,TIMESTAMP,TINYINT,
             FLOAT,IMAGE,VARCHAR(MAX),NVARCHAR(MAX),MONEY,NTEXT,BINARY,REAL,SMALLDATETIME,
             SMALLINT,SMALLMONEY,SQL_VARIANT,TEXT,UNIQUEIDENTIFIER,VARBINARY,VARBINARY(MAX),XML
             */
            SqlDbType dType = SqlDbType.NVarChar;
            strSqlDbType = strSqlDbType.ToUpper(); //注：要转换为大写来判断
            if (strSqlDbType == "NVARCHAR" || strSqlDbType == "NVARCHAR(MAX)")
            {
                return dType;
            }
            else if (strSqlDbType == "VARCHAR" || strSqlDbType == "VARCHAR(MAX)")
            {
                return SqlDbType.VarChar;
            }
            else if (strSqlDbType == "CHAR")
            {
                return SqlDbType.Char;
            }
            else if (strSqlDbType == "NCHAR")
            {
                return SqlDbType.NChar;
            }
            else if (strSqlDbType == "IMAGE")
            {
                return SqlDbType.Image;
            }
            else if (strSqlDbType == "MONEY")
            {
                return SqlDbType.Money;
            }
            else if (strSqlDbType == "SMALLMONEY")
            {
                return SqlDbType.SmallMoney;
            }
            else if (strSqlDbType == "NTEXT")
            {
                return SqlDbType.NText;
            }
            else if (strSqlDbType == "TEXT")
            {
                return SqlDbType.Text;
            }
            else if (strSqlDbType == "BINARY")
            {
                return SqlDbType.Binary;
            }
            else if (strSqlDbType == "REAL")
            {
                return SqlDbType.Real;
            }
            else if (strSqlDbType == "SMALLDATETIME")
            {
                return SqlDbType.SmallDateTime;
            }
            else if (strSqlDbType == "XML")
            {
                return SqlDbType.Xml;
            }
            #region 数值类
            else if (strSqlDbType == "INT")
            {
                return SqlDbType.Int;
            }
            else if (strSqlDbType == "SMALLINT")
            {
                return SqlDbType.SmallInt;
            }
            else if (strSqlDbType == "BIGINT")
            {
                return SqlDbType.BigInt;
            }
            else if (strSqlDbType == "DECIMAL")
            {
                return SqlDbType.Decimal;
            }
            else if (strSqlDbType == "FLOAT")
            {
                return SqlDbType.Float;
            }
            else if (strSqlDbType == "BIT")
            {
                return SqlDbType.Bit;
            }
            else if (strSqlDbType == "TINYINT")
            {
                return SqlDbType.TinyInt;
            }
            #endregion

            #region 日期时间类
            else if (strSqlDbType == "DATETIME")
            {
                return SqlDbType.DateTime;
            }
            else if (strSqlDbType == "TIMESTAMP")
            {
                return SqlDbType.Timestamp;
            }
            else if (strSqlDbType == "System.DateTime2")
            {
                return SqlDbType.DateTime2;
            }
            else if (strSqlDbType == "System.Time")
            {
                return SqlDbType.Time;
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
        public SqlDbType DbTypeToSqlDbType(DbType pSourceType)
        {
            SqlParameter paraConver = new SqlParameter();
            paraConver.DbType = pSourceType;
            return paraConver.SqlDbType;
        }

        public DbType SqlDbTypeToDbType(SqlDbType pSourceType)
        {
            SqlParameter paraConver = new SqlParameter();
            paraConver.SqlDbType = pSourceType;
            return paraConver.DbType;
        }
        #endregion

        #region 增加SQL Server参数
        public override DbParameter AddParam(string[] sParameterArr, DbCommand dbCommand, DbParameter sqlParameter, int i, string[] sProperties, string parameterCode, string executeType, string parameterType)
        {
            SqlCommand sqlCommand = dbCommand as SqlCommand;
            if (parameterType == "BIGINT")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, SqlDbType.BigInt);
                sqlParameter.Value = Int64.Parse(sParameterArr[i]);
            }
            else if (parameterType == "INT")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, SqlDbType.Int);
                if (executeType != "RETURNVALUE")
                    sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "DECIMAL")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, SqlDbType.Decimal);
                sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "VARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, SqlDbType.VarChar, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "NVARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, SqlDbType.NVarChar, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "DATETIME")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, SqlDbType.DateTime);
                sqlParameter.Value = DateTime.Parse(sParameterArr[i]);
            }
            else
            {
                throw new Exception("未定义的存储过程的参数类型：" + executeType);
            }
            return sqlParameter;
        }
        #endregion

        #region 设置存储过程命令
        public override void SetProdureCommond(string sSql, DbConnection con, DbTransaction tran, out DbCommand dbCommand, out DbDataAdapter adpater)
        {
            if (tran == null)
            {
                dbCommand = new SqlCommand(sSql, (SqlConnection)con);
            }
            else
            {
                dbCommand = new SqlCommand(sSql, (SqlConnection)con, (SqlTransaction)tran);
            }

            adpater = new SqlDataAdapter();
        }
        #endregion

        #region 通过连接对象获取数据库元数据信息
        /// <summary>
        /// 获取数据库架构信息
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="restrictionValues">针对SQL Server 数据库，restrictionValues的长度为4，其中restrictionValues[0]为Catalog（数据库名），restrictionValues[1]为Owner（所有者），restrictionValues[2]为Table（表名），restrictionValues[3]为Column（列名）</param>
        /// <returns></returns>
        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            using (SqlConnection con = (SqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable schema = con.GetSchema(collectionName, restrictionValues);
                return schema;
            }
        }

        /// <summary>
        /// 获取数据库清单
        /// </summary>
        /// <returns></returns>
        public override DataTable GetDataBases()
        {
            using (SqlConnection con = (SqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable dtSource = con.GetSchema(DBSchemaString.Databases, null);
                //返回标准的结果表
                DataTable dtReturn = DT_DataBase;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBDataBaseEntity.SqlString.Name] = drS[SqlServerSchemaString.DataBase.Name];
                    dtReturn.Rows.Add(dr);
                }
                return dtReturn;
            }
        }

        /// <summary>
        /// 获取用户表清单
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTables(string sTableName = null, string sSchema = null)
        {
            using (SqlConnection con = (SqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable res = con.GetSchema(DBSchemaString.Restrictions);//查询GetSchema第二个参数的含义说明
                DataTable dtSource = con.GetSchema(DBSchemaString.Tables);//没有表备注信息
                //返回标准的结果表
                DataTable dtReturn = DT_SchemaTable;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBTableEntity.SqlString.Schema] = drS[SqlServerSchemaString.Table.TableSchema];
                    dr[DBTableEntity.SqlString.Name] = drS[SqlServerSchemaString.Table.TableName];
                    dr[DBColumnEntity.SqlString.TableNameUpper] = drS[SqlServerSchemaString.Table.TableName].ToString().FirstLetterUpper();
                    dr[DBColumnEntity.SqlString.TableNameLower] = drS[SqlServerSchemaString.Table.TableName].ToString().FirstLetterUpper(false);
                    //SqlSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
                    //dr[DBTableEntity.SqlString.Owner] = drS["TABLE_SCHEMA"];
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
            sTableName = sTableName.ToUpper();

            using (SqlConnection con = (SqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();

                //单独处理主键
                string sSql = @"SELECT TABLE_NAME,COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME='#TABLE_NAME#'";
                IDictionary<string, string> dic = new Dictionary<string, string>();
                dic["TABLE_NAME"] = sTableName;
                DataTable dtPK = QueryAutoParamSqlData(sSql, dic);
                bool isPKOK = false;

                DataTable dtSource = con.GetSchema(DBSchemaString.Columns, new string[] { null, null, sTableName });//使用通用的获取架构方法
                DataTable dtReturn = DT_SchemaTableColumn;
                foreach (DataRow drS in dtSource.Select("", SqlServerSchemaString.Column.OrdinalPosition + " asc"))
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBColumnEntity.SqlString.TableSchema] = drS[SqlServerSchemaString.Column.TableSchema];//Schema跟数据库名称一样
                    dr[DBColumnEntity.SqlString.TableName] = drS[SqlServerSchemaString.Column.TableName];
                    dr[DBColumnEntity.SqlString.TableNameUpper] = drS[SqlServerSchemaString.Column.TableName].ToString().FirstLetterUpper();
                    dr[DBColumnEntity.SqlString.TableNameLower] = drS[SqlServerSchemaString.Column.TableName].ToString().FirstLetterUpper(false);
                    dr[DBColumnEntity.SqlString.SortNum] = drS[SqlServerSchemaString.Column.OrdinalPosition];
                    dr[DBColumnEntity.SqlString.Name] = drS[SqlServerSchemaString.Column.ColumnName];
                    dr[DBColumnEntity.SqlString.NameUpper] = drS[SqlServerSchemaString.Column.ColumnName].ToString().FirstLetterUpper();
                    dr[DBColumnEntity.SqlString.NameLower] = drS[SqlServerSchemaString.Column.ColumnName].ToString().FirstLetterUpper(false);
                    //dr[SqlColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"];
                    dr[DBColumnEntity.SqlString.Default] = drS[SqlServerSchemaString.Column.ColumnDefault];
                    dr[DBColumnEntity.SqlString.NotNull] = drS[SqlServerSchemaString.Column.IsNullable].ToString().ToUpper().Equals("NO")?"1":"";
                    dr[DBColumnEntity.SqlString.DataType] = drS[SqlServerSchemaString.Column.DataType];
                    dr[DBColumnEntity.SqlString.DataLength] = drS[SqlServerSchemaString.Column.CharacterMaximumLength];
                    dr[DBColumnEntity.SqlString.DataPrecision] = drS[SqlServerSchemaString.Column.Numeric_Precision];
                    dr[DBColumnEntity.SqlString.DataScale] = drS[SqlServerSchemaString.Column.Numeric_Scale];
                    //dr[SqlColumnEntity.SqlString.DataTypeFull] = drS["COLUMN_TYPE"];
                    //dr[SqlColumnEntity.SqlString.NameCN] = drS["COLUMN_CN"];
                    //dr[SqlColumnEntity.SqlString.Extra] = drS["COLUMN_EXTRA"];
                    if (!isPKOK)
                    {
                        DataRow[] arrPK = dtPK.Select("COLUMN_NAME='" + drS[SqlServerSchemaString.Column.ColumnName] + "'");
                        if(arrPK.Length>0)
                        {
                            dr[DBColumnEntity.SqlString.KeyType] = "PK";
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
            IDictionary<string, string> dic;
            //一般TABLE_SCHEMA为dbo
            if (string.IsNullOrEmpty(sSchema))
            {
                sSchema = "dbo";
            }
            else
            {
                dic = new Dictionary<string, string>();
                dic["SCHEMA_NAME"] = sSchema;
                string sDBSql = "SELECT NAME,DEFAULT_SCHEMA_NAME FROM SYS.DATABASE_PRINCIPALS WHERE NAME=@SCHEMA_NAME";
                DataTable dtDBList = QueryHadParamSqlData(sDBSql, dic);
                if (dtDBList.Rows.Count==0)
                {
                    throw new Exception(string.Format("GetSqlSchemaTables方法传入的Schema（{0}）不存在！", sSchema));
                }
            }           

            //SqlServer表名区分大小写
            string sSql = @"SELECT  B.NAME AS TABLE_SCHEMA ,
                    A.NAME AS TABLE_NAME,
                    C.VALUE AS TABLE_COMMENT
            FROM    SYS.OBJECTS A
            JOIN SYS.SCHEMAS B ON A.SCHEMA_ID = B.SCHEMA_ID
            LEFT JOIN SYS.EXTENDED_PROPERTIES C ON C.MAJOR_ID=A.OBJECT_ID AND C.MINOR_ID=0
            WHERE   A.TYPE = 'U'
                    AND A.NAME = '#TABLE_NAME#'
                    AND B.NAME= '#TABLE_SCHEMA#'
             ORDER BY A.Name
            ";

            dic = new Dictionary<string, string>();
            dic["TABLE_SCHEMA"] = sSchema;
            dic["TABLE_NAME"] = sTableName;//区分大小写，这里没转换为大写
            DataTable dtSource = QueryAutoParamSqlData(sSql, dic);
            DataTable dtReturn = DT_SchemaTable;
            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();
                dr[DBTableEntity.SqlString.Schema] = drS["TABLE_SCHEMA"];
                dr[DBTableEntity.SqlString.Name] = drS["TABLE_NAME"];
                dr[DBTableEntity.SqlString.NameUpper] = drS["TABLE_NAME"].ToString().FirstLetterUpper();
                dr[DBTableEntity.SqlString.NameLower] = drS["TABLE_NAME"].ToString().FirstLetterUpper(false);
                DBSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
                dr[DBTableEntity.SqlString.DBName] = DbServer.Database;//数据库名
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
            //SqlServer的字段名区分大小写
            string sSql = @"SELECT
				S.NAME AS TABLE_SCHEMA ,
                TABLE_NAME = D.NAME ,
                TABLE_COMMENT = F.VALUE,
                ORDINAL_POSITION = A.COLORDER,
                COLUMN_NAME = A.NAME,
                IS_IDENTITY = case when COLUMNPROPERTY(a.id, a.name, 'IsIdentity')= 1 then '1'else '' end,
                COLUMN_KEY = CASE WHEN EXISTS(SELECT 1 FROM SYSOBJECTS WHERE XTYPE = 'PK' AND PARENT_OBJ = A.ID AND NAME IN (
                           SELECT NAME FROM SYSINDEXES WHERE INDID IN(SELECT INDID FROM SYSINDEXKEYS WHERE ID = A.ID AND COLID = A.COLID))) THEN 'PK' ELSE '' END,
                DATA_TYPE = B.NAME,
                CHARACTER_MAXIMUM_LENGTH = A.LENGTH,
                NUMERIC_PRECISION = COLUMNPROPERTY(A.ID, A.NAME, 'PRECISION'),
                NUMERIC_SCALE = ISNULL(COLUMNPROPERTY(A.ID, A.NAME, 'SCALE'), 0),
                IS_NULLABLE = A.ISNULLABLE,
                COLUMN_DEFAULT = ISNULL(E.TEXT, ''),
                COLUMN_COMMENT = ISNULL(G.[VALUE], '')
            FROM SYSCOLUMNS A
            LEFT JOIN SYSTYPES B
	            ON A.XUSERTYPE = B.XUSERTYPE
            JOIN SYS.OBJECTS D
	            ON A.ID = D.object_id  AND D.TYPE = 'U' AND D.NAME <> 'DTPROPERTIES'
			JOIN SYS.SCHEMAS S ON D.SCHEMA_ID = S.SCHEMA_ID
            LEFT JOIN SYSCOMMENTS E
	            ON A.CDEFAULT = E.ID
            LEFT JOIN SYS.EXTENDED_PROPERTIES G
	            ON A.ID = G.MAJOR_ID AND A.COLID = G.MINOR_ID
            LEFT JOIN SYS.EXTENDED_PROPERTIES F
	            ON D.object_id = F.MAJOR_ID AND F.MINOR_ID = 0
            WHERE 1=1
             AND D.NAME = '#TABLE_NAME#'
             AND D.NAME IN (#TABLE_NAME_LIST:LS#)
            ORDER BY D.NAME,A.COLORDER
            ";

            IDictionary<string, object> dic = new Dictionary<string, object>();
            if (listTableName.Count == 0)
            {
                return GetColumnTable(sSql, dic);
            }
            else if(listTableName.Count == 1)
            {
                dic["TABLE_NAME"] = listTableName[0];
                return GetColumnTable(sSql, dic);
            }
            else if (listTableName.Count < MaxInStringCount)
            {
                dic["TABLE_NAME_LIST"] = listTableName;
                return GetColumnTable(sSql, dic);
            }
            else
            {
                List<string> listTableNameNew = new List<string>();
                DataTable dtReturn = DT_SchemaTableColumn;
                for (int i = 0; i < listTableName.Count; i++)
                {
                    listTableNameNew.Add(listTableName[i]);
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
            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();
                dr[DBColumnEntity.SqlString.TableSchema] = drS["TABLE_SCHEMA"];//Schema跟数据库名称一样
                dr[DBColumnEntity.SqlString.TableName] = drS["TABLE_NAME"];
                dr[DBColumnEntity.SqlString.TableNameUpper] = drS["TABLE_NAME"].ToString().FirstLetterUpper();
                dr[DBColumnEntity.SqlString.TableNameLower] = drS["TABLE_NAME"].ToString().FirstLetterUpper(false);
                DBSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());

                dr[DBColumnEntity.SqlString.SortNum] = drS["ORDINAL_POSITION"];
                dr[DBColumnEntity.SqlString.Name] = drS["COLUMN_NAME"];//区分大小写，这里没转换为大写
                dr[DBColumnEntity.SqlString.NameUpper] = drS["COLUMN_NAME"].ToString().FirstLetterUpper();
                dr[DBColumnEntity.SqlString.NameLower] = drS["COLUMN_NAME"].ToString().FirstLetterUpper(false);
                dr[DBColumnEntity.SqlString.Default] = drS["COLUMN_DEFAULT"];
                dr[DBColumnEntity.SqlString.NotNull] = drS["IS_NULLABLE"].ToString().Equals("0") ? "1" : "";
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
                dr[DBColumnEntity.SqlString.KeyType] = drS["COLUMN_KEY"];
                DBSchemaCommon.SetComment(dr, drS["COLUMN_COMMENT"].ToString(), false);
                if (CharLengthTypes.Where(s => s.Equals(drS["DATA_TYPE"].ToString(), StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
                {
                    dr[DBColumnEntity.SqlString.DataTypeFull] = string.Format("{0}({1})", drS["DATA_TYPE"], drS["CHARACTER_MAXIMUM_LENGTH"]);
                }
                else if (PrecisonTypes.Where(s => s.Equals(drS["DATA_TYPE"].ToString(), StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
                {
                    dr[DBColumnEntity.SqlString.DataPrecision] = drS["NUMERIC_PRECISION"];
                    dr[DBColumnEntity.SqlString.DataScale] = drS["NUMERIC_SCALE"];
                    dr[DBColumnEntity.SqlString.DataTypeFull] = string.Format("{0}({1},{2})", drS["DATA_TYPE"], drS["NUMERIC_PRECISION"], drS["NUMERIC_SCALE"]);
                }
                else
                {
                    dr[DBColumnEntity.SqlString.DataTypeFull] = drS["DATA_TYPE"];
                }
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
        #endregion
    }
}
