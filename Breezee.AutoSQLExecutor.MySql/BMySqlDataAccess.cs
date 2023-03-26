using Breezee.AutoSQLExecutor.Core;
using Breezee.Core.Interface;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.MySql
{
    /// <summary>
    /// MySql的数据访问层
    /// </summary>
    public class BMySqlDataAccess: IDataAccess
    {
        #region 属性
        public override DataBaseType DataBaseType
        {
            get { return DataBaseType.MySql; }
        }

        private string _ConnectionString;
        public override string ConnectionString
        {
            get { return _ConnectionString; }
            protected set { _ConnectionString = value; }
        }

        ISqlDifferent _SqlDiff = new BMySqlSqlDifferent();
        public override ISqlDifferent SqlDiff { get => _SqlDiff; protected set => _SqlDiff = value; }

        public override List<string> CharLengthTypes { get => (new string[] { MySqlColumnType.Text.Char, MySqlColumnType.Text.varchar}).ToList(); }
        public override List<string> PrecisonTypes { get => (new string[] { MySqlColumnType.Precision.Decimal, MySqlColumnType.Precision.Double,
            MySqlColumnType.Precision.Float, MySqlColumnType.Precision.numeric, MySqlColumnType.Precision.real }).ToList(); }
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sConstr">连接字符串：注名字要跟Main.config配置文件中的连接字符串配置字符保持一致</param>
        public BMySqlDataAccess(string sConstr) : base(sConstr)
        {
            _ConnectionString = sConstr;
            SqlParsers.properties.ParamPrefix = "@"; //注：MySql是使用@作为参数前缀
        }

        public BMySqlDataAccess(DbServerInfo server):base(server)
        {
            SqlParsers.properties.ParamPrefix = "@"; //注：MySql是使用@作为参数前缀
        }
        #endregion

        #region 创建连接
        /// <summary>
        /// 创建SQL Server连接
        /// </summary>
        /// <returns></returns>
        public override DbConnection GetCurrentConnection()
        {
            return new MySqlConnection(_ConnectionString);
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
            DbServer = server;
            _ConnectionString = server.UseConnString ? server.ConnString : string.Format("Server={0};;Port={1};Database={2};Uid={3};Pwd={4};Charset=utf8;AllowUserVariables=true;", server.ServerName, server.PortNo, server.Database, server.UserName, server.Password);
        }
        #endregion

        #region 实现字典转换为DB服务器方法
        protected override DbServerInfo Dic2DbServer(Dictionary<string, string> dic)
        {
            DbServerInfo server = new DbServerInfo();
            server.DatabaseType = DataBaseType.MySql;
            foreach (var item in dic)
            {
                //注意:键是小写字符
                if (item.Key.Equals("server"))
                {
                    server.ServerName = item.Value;
                }
                else if (item.Key.Equals("database"))
                {
                    server.Database = item.Value;
                }
                else if (item.Key.Equals("uid"))
                {
                    server.UserName = item.Value;
                }
                else if (item.Key.Equals("pwd"))
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
                MySqlCommand sqlCommon;
                if (dbTran == null)
                {
                    sqlCommon = new MySqlCommand(sHadParaSql, (MySqlConnection)conn);
                }
                else
                {
                    sqlCommon = new MySqlCommand(sHadParaSql, (MySqlConnection)conn, (MySqlTransaction)dbTran);
                }
                //构造适配器
                MySqlDataAdapter adapter = new MySqlDataAdapter(sqlCommon);
                if (listParam != null)
                {
                    foreach (FuncParam item in listParam)
                    {
                        MySqlParameter sp = new MySqlParameter(item.Code, item.Value);
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
            /*语句3：select * from studnet limit (pageNumber-1)*pageSize,pageSize
              语句4：select * from student limit pageSize offset (pageNumber-1)*pageSize
             */
            string pageDataSql = "SELECT @rownum:=@rownum+1 AS ROWNUM, MYTABLE.* FROM ( " + sHadParaSql + " ) AS MYTABLE,(SELECT @rownum:=0) r ";
            pageDataSql = "SELECT MYTABLE.*" + " FROM (" + pageDataSql + ") MYTABLE WHERE 1=1 LIMIT " +
                       pParam.PageSize + " offset " + ((pParam.PageNO-1)* pParam.PageSize).ToString();
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
            }
            //构造命令
            MySqlCommand sqlCommon;
            if (dbTran == null)
            {
                sqlCommon = new MySqlCommand(sHadParaSql, (MySqlConnection)conn);
            }
            else
            {
                sqlCommon = new MySqlCommand(sHadParaSql, (MySqlConnection)conn, (MySqlTransaction)dbTran);
            }

            if (listParam != null)
            {
                foreach (FuncParam item in listParam)
                {
                    MySqlParameter sp = new MySqlParameter(item.Code, item.Value);
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
        public override DataTable SaveTable(DataTable dt, DbConnection conn, DbTransaction dbTran)
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
                DataRow[] dcPKList = dtTableInfo.Select(DBColumnEntity.SqlString.KeyType + "= 'PK'");
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
                            strInsertEnd.Append(sDouHao + "NOW()");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=NOW()");
                        }
                        else if (tcy == DbDefaultValueType.TimeStamp)
                        {
                            strInsertEnd.Append(sDouHao + "@@DBTS");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=@@DBTS");
                        }
                        else if (tcy == DbDefaultValueType.Guid)
                        {
                            strInsertEnd.Append(sDouHao + "UUID()");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=UUID()");
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
                        string sPKColumn = dcPKList[i][DBColumnEntity.SqlString.Name].ToString();
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
                MySqlDataAdapter adapter = new MySqlDataAdapter();
                MySqlConnection con = (MySqlConnection)conn;
                //"INSERT INTO dbo.t6( com_id ,usr_id ) VALUES( @com_id ,@usr_id)"
                string strInsert = strInsertPre.ToString() + strInsertEnd.ToString();
                adapter.InsertCommand = new MySqlCommand(strInsert, con, (MySqlTransaction)dbTran);
                adapter.InsertCommand.CommandType = CommandType.Text;

                string strUpdateSql = strUpdate.ToString() + strWhere.ToString();
                adapter.UpdateCommand = new MySqlCommand(strUpdateSql, con, (MySqlTransaction)dbTran); //"update t6 set usr_id=@usr_id where com_id=@com_idand usr_id=@usr_id1"
                adapter.UpdateCommand.CommandType = CommandType.Text;

                string strDeleteSql = strDelete.ToString() + strWhere.ToString();
                adapter.DeleteCommand = new MySqlCommand(strDeleteSql, con, (MySqlTransaction)dbTran); //"delete from t6 where com_id=@com_idand usr_id=@usr_id"
                adapter.DeleteCommand.CommandType = CommandType.Text;

                foreach (DataColumn dc in dt.Columns)
                {
                    DataRow[] drCol = dtTableInfo.Select(DBColumnEntity.SqlString.Name + "='" + dc.ColumnName + "'");
                    Int32 iLen = 0;
                    if (!string.IsNullOrEmpty(drCol[0][DBColumnEntity.SqlString.DataLength].ToString()))
                    {
                        iLen = Int32.Parse(drCol[0][DBColumnEntity.SqlString.DataLength].ToString());
                    }
                    else if (!string.IsNullOrEmpty(drCol[0][DBColumnEntity.SqlString.DataPrecision].ToString()))
                    {
                        iLen = Int32.Parse(drCol[0][DBColumnEntity.SqlString.DataPrecision].ToString());
                    }
                    MySqlDbType ot = TransToSqlType(drCol[0][DBColumnEntity.SqlString.DataType].ToString().ToUpper());
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
        public MySqlDbType TransToSqlType(string strSqlDbType)
        {
            /*INT,BIGINT,BIT,VARCHAR,DATETIME,DECIMAL,NVARCHAR,CHAR,NCHAR,NUMERIC,TIMESTAMP,TINYINT,
             * FLOAT,IMAGE,VARCHAR(MAX),NVARCHAR(MAX),MONEY,NTEXT,BINARY,REAL,SMALLDATETIME,
             * SMALLINT,SMALLMONEY,SQL_VARIANT,TEXT,UNIQUEIDENTIFIER,VARBINARY,VARBINARY(MAX),XML*/
            MySqlDbType dType = MySqlDbType.VarChar;
            strSqlDbType = strSqlDbType.ToUpper(); //注：要转换为大写来判断
            if (strSqlDbType == "NVARCHAR" || strSqlDbType == "NVARCHAR(MAX)")
            {
                return dType;
            }
            else if (strSqlDbType == "VARCHAR" || strSqlDbType == "VARCHAR(MAX)")
            {
                return MySqlDbType.VarChar;
            }
            else if (strSqlDbType == "CHAR")
            {
                return MySqlDbType.VarChar;
            }
            else if (strSqlDbType == "NCHAR")
            {
                return MySqlDbType.VarChar;
            }
            //else if (strSqlDbType == "IMAGE")
            //{
            //    return MySqlDbType.Image;
            //}
            //else if (strSqlDbType == "MONEY")
            //{
            //    return MySqlDbType.Money;
            //}
            //else if (strSqlDbType == "SMALLMONEY")
            //{
            //    return MySqlDbType.SmallMoney;
            //}
            //else if (strSqlDbType == "NTEXT")
            //{
            //    return MySqlDbType.NText;
            //}
            else if (strSqlDbType == "TEXT")
            {
                return MySqlDbType.Text;
            }
            else if (strSqlDbType == "BINARY")
            {
                return MySqlDbType.Binary;
            }
            //else if (strSqlDbType == "REAL")
            //{
            //    return MySqlDbType.Real;
            //}
            //else if (strSqlDbType == "SMALLDATETIME")
            //{
            //    return MySqlDbType.SmallDateTime;
            //}
            //else if (strSqlDbType == "XML")
            //{
            //    return MySqlDbType.Xml;
            //}
            #region 数值类
            else if (strSqlDbType == "INT")
            {
                return MySqlDbType.Int32;
            }
            else if (strSqlDbType == "SMALLINT")
            {
                return MySqlDbType.Int24;
            }
            else if (strSqlDbType == "BIGINT")
            {
                return MySqlDbType.Int64;
            }
            else if (strSqlDbType == "DECIMAL")
            {
                return MySqlDbType.Decimal;
            }
            else if (strSqlDbType == "FLOAT")
            {
                return MySqlDbType.Float;
            }
            else if (strSqlDbType == "BIT")
            {
                return MySqlDbType.Bit;
            }
            else if (strSqlDbType == "TINYINT")
            {
                return MySqlDbType.Int16;
            }
            #endregion

            #region 日期时间类
            else if (strSqlDbType == "DATETIME")
            {
                return MySqlDbType.DateTime;
            }
            else if (strSqlDbType == "TIMESTAMP")
            {
                return MySqlDbType.Timestamp;
            }
            else if (strSqlDbType == "System.DateTime2")
            {
                return MySqlDbType.DateTime;
            }
            else if (strSqlDbType == "System.Time")
            {
                return MySqlDbType.Time;
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
        public MySqlDbType DbTypeToSqlDbType(DbType pSourceType)
        {
            MySqlParameter paraConver = new MySqlParameter();
            paraConver.DbType = pSourceType;
            return paraConver.MySqlDbType;
        }

        public DbType SqlDbTypeToDbType(MySqlDbType pSourceType)
        {
            MySqlParameter paraConver = new MySqlParameter();
            paraConver.MySqlDbType = pSourceType;
            return paraConver.DbType;
        }
        #endregion

        #region 增加SQL参数
        public override DbParameter AddParam(string[] sParameterArr, DbCommand dbCommand, DbParameter sqlParameter, int i, string[] sProperties, string parameterCode, string executeType, string parameterType)
        {
            MySqlCommand sqlCommand = dbCommand as MySqlCommand;
            if (parameterType == "BIGINT")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, MySqlDbType.Int64);
                sqlParameter.Value = Int64.Parse(sParameterArr[i]);
            }
            else if (parameterType == "INT")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, MySqlDbType.Int32);
                if (executeType != "RETURNVALUE")
                    sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "DECIMAL")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, MySqlDbType.Decimal);
                sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "VARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, MySqlDbType.VarChar, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "NVARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, MySqlDbType.VarChar, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "DATETIME")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, MySqlDbType.DateTime);
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
                dbCommand = new MySqlCommand(sSql, (MySqlConnection)con);
            }
            else
            {
                dbCommand = new MySqlCommand(sSql, (MySqlConnection)con, (MySqlTransaction)tran);
            }

            adpater = new MySqlDataAdapter();
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
            using (MySqlConnection con = (MySqlConnection)GetCurrentConnection())
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
            using (MySqlConnection con = (MySqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable dtSource = con.GetSchema(DBSchemaString.Databases, null);
                //返回标准的结果表
                DataTable dtReturn = DT_DataBase;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBDataBaseEntity.SqlString.Name] = drS[MySqlSchemaString.DataBase.Name];
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
            using (MySqlConnection con = (MySqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable res = con.GetSchema(DBSchemaString.Restrictions);//查询GetSchema第二个参数的含义说明
                //Tables:Database,Schema,Table,TableType
                DataTable dtSource = con.GetSchema(DBSchemaString.Tables,new string[] { null,sSchema,sTableName,null});
                //返回标准的结果表
                DataTable dtReturn = DT_SchemaTable;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBTableEntity.SqlString.Schema] = drS[MySqlSchemaString.Table.TableSchema];
                    dr[DBTableEntity.SqlString.Name] = drS[MySqlSchemaString.Table.TableName];
                    DBSchemaCommon.SetComment(dr, drS[MySqlSchemaString.Table.TableComment].ToString());
                    //dr[SqlTableEntity.SqlString.Owner] = drS["OWNER"];
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
            using (MySqlConnection con = (MySqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                //Columns:Database,Schema,Table,Column
                DataTable dtSource = con.GetSchema(DBSchemaString.Columns, new string[] { null, null, sTableName });//使用通用的获取架构方法
                DataTable dtReturn = DT_SchemaTableColumn;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBColumnEntity.SqlString.TableSchema] = drS[MySqlSchemaString.Column.TableSchema];//Schema跟数据库名称一样
                    dr[DBColumnEntity.SqlString.TableName] = drS[MySqlSchemaString.Column.TableName];
                    dr[DBColumnEntity.SqlString.SortNum] = drS[MySqlSchemaString.Column.OrdinalPosition];
                    dr[DBColumnEntity.SqlString.Name] = drS[MySqlSchemaString.Column.ColumnName];
                    //dr[DBColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"];
                    dr[DBColumnEntity.SqlString.Default] = drS[MySqlSchemaString.Column.ColumnDefault];
                    dr[DBColumnEntity.SqlString.NotNull] = drS[MySqlSchemaString.Column.IsNullable].ToString().ToUpper().Equals("NO") ? "1" : "";
                    dr[DBColumnEntity.SqlString.DataType] = drS[MySqlSchemaString.Column.DataType];
                    dr[DBColumnEntity.SqlString.DataLength] = drS[MySqlSchemaString.Column.CharacterMaximumLength];
                    dr[DBColumnEntity.SqlString.DataPrecision] = drS[MySqlSchemaString.Column.Numeric_Precision];
                    dr[DBColumnEntity.SqlString.DataScale] = drS[MySqlSchemaString.Column.Numeric_Scale];
                    dr[DBColumnEntity.SqlString.DataTypeFull] = drS[MySqlSchemaString.Column.ColumnType];
                    dr[DBColumnEntity.SqlString.KeyType] = drS[MySqlSchemaString.Column.KeyType].ToString().ToUpper().Equals("PRI")?"PK":"";
                    DBSchemaCommon.SetComment(dr, drS[MySqlSchemaString.Column.ColumnComment].ToString());
                    //dr[SqlColumnEntity.SqlString.NameCN] = drS["COLUMN_CN"];
                    //dr[SqlColumnEntity.SqlString.Extra] = drS["COLUMN_EXTRA"];

                    dtReturn.Rows.Add(dr);
                }
                return dtReturn;
            }
        }
        #endregion

        #region 通过SQL语句获取数据库元数据信息
        public override DataTable GetSqlSchemaTables(string sTableName = null, string sSchema = null)
        {
            if (string.IsNullOrEmpty(sSchema)) sSchema = DbServer.Database.ToLower();
            //TABLE_SCHEMA为数据库存名。TABLE_SCHEMA,TABLE_NAME,TABLE_COMMENT,`ENGINE`
            string sSql = @"SELECT TABLE_SCHEMA,TABLE_NAME,TABLE_COMMENT
                    FROM INFORMATION_SCHEMA.`TABLES`
                    WHERE 1=1
                    AND TABLE_SCHEMA = '#TABLE_SCHEMA#'
                    AND TABLE_NAME = '#TABLE_NAME#'
            ";

            IDictionary<string, string> dic = new Dictionary<string, string>();
            dic["TABLE_SCHEMA"] = sSchema;
            dic["TABLE_NAME"] = sTableName;
            DataTable dtSource = QueryAutoParamSqlData(sSql, dic);
            DataTable dtReturn = DT_SchemaTable;
            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();
                dr[DBTableEntity.SqlString.Schema] = drS["TABLE_SCHEMA"];//Schema跟数据库名称一样
                dr[DBTableEntity.SqlString.Name] = drS["TABLE_NAME"];
                DBSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
                dr[DBTableEntity.SqlString.DBName] = DbServer.Database;//数据库名
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        public override DataTable GetSqlSchemaTableColumns(string sTableName, string sSchema = null)
        {
            string sSql = @"SELECT TABLE_SCHEMA,
	            TABLE_NAME,
	            ORDINAL_POSITION,
	            COLUMN_NAME,
	            COLUMN_COMMENT,
	            COLUMN_DEFAULT,
	            IS_NULLABLE,
	            DATA_TYPE,
	            CHARACTER_MAXIMUM_LENGTH,
	            NUMERIC_PRECISION,
	            NUMERIC_SCALE,
	            COLUMN_TYPE,
	            COLUMN_KEY,
	            SUBSTRING_INDEX(COLUMN_COMMENT,':',1) AS COLUMN_CN,
	            SUBSTRING_INDEX(COLUMN_COMMENT,':',-1) AS COLUMN_EXTRA
            FROM information_schema.`COLUMNS`
            WHERE 1=1
            AND TABLE_NAME = '#TABLE_NAME#'
            AND TABLE_SCHEMA = '#TABLE_SCHEMA#'
            ";

            IDictionary<string, string> dic = new Dictionary<string, string>();
            dic["TABLE_NAME"] = sTableName;
            dic["TABLE_SCHEMA"] = sSchema;
            DataTable dtSource = QueryAutoParamSqlData(sSql, dic);
            DataTable dtReturn = DT_SchemaTableColumn;
            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();
                dr[DBColumnEntity.SqlString.TableSchema] = drS["TABLE_SCHEMA"];//Schema跟数据库名称一样
                dr[DBColumnEntity.SqlString.TableName] = drS["TABLE_NAME"];
                dr[DBColumnEntity.SqlString.SortNum] = drS["ORDINAL_POSITION"];
                dr[DBColumnEntity.SqlString.Name] = drS["COLUMN_NAME"];
                dr[DBColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"];
                dr[DBColumnEntity.SqlString.Default] = drS["COLUMN_DEFAULT"];
                dr[DBColumnEntity.SqlString.NotNull] = drS["IS_NULLABLE"].ToString().ToUpper().Equals("NO") ? "1" : "";
                dr[DBColumnEntity.SqlString.DataType] = drS["DATA_TYPE"];
                dr[DBColumnEntity.SqlString.DataLength] = drS["CHARACTER_MAXIMUM_LENGTH"];
                dr[DBColumnEntity.SqlString.DataPrecision] = drS["NUMERIC_PRECISION"];
                dr[DBColumnEntity.SqlString.DataScale] = drS["NUMERIC_SCALE"];
                dr[DBColumnEntity.SqlString.DataTypeFull] = drS["COLUMN_TYPE"];
                dr[DBColumnEntity.SqlString.KeyType] = drS["COLUMN_KEY"].ToString().ToUpper().Equals("PRI") ? "PK" : "";
                dr[DBColumnEntity.SqlString.NameCN] = drS["COLUMN_CN"];
                dr[DBColumnEntity.SqlString.Extra] = drS["COLUMN_EXTRA"];

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
        #endregion




    }
}
