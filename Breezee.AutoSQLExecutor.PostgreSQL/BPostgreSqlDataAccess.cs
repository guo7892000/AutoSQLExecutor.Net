using Breezee.AutoSQLExecutor.Core;
using Breezee.Core.Interface;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.PostgreSQL
{
    public class BPostgreSqlDataAccess : IDataAccess
    {
        #region 属性
        public override DataBaseType DataBaseType
        {
            get { return DataBaseType.PostgreSql; }
        }

        private string _ConnectionString;
        public override string ConnectionString
        {
            get { return _ConnectionString; }
            protected set { _ConnectionString = value; }
        }

        ISqlDifferent _SqlDiff = new BPostgreSqlSqlDifferent();
        public override ISqlDifferent SqlDiff { get => _SqlDiff; protected set => _SqlDiff = value; }

        List<string> _typeListCharLength = new List<string>();
        List<string> _typeListPrecison = new List<string>();
        public override List<string> CharLengthTypes { get => (new string[] { PostgreSqlColumnType.Text.character, PostgreSqlColumnType.Text.Char, PostgreSqlColumnType.Text.characterVarying }).ToList(); }
        public override List<string> PrecisonTypes { get => (new string[] { PostgreSqlColumnType.Precision.numeric, PostgreSqlColumnType.Precision.doublePrecision, PostgreSqlColumnType.Precision.real }).ToList(); }


        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sConstr">连接字符串：注名字要跟Main.config配置文件中的连接字符串配置字符保持一致</param>
        public BPostgreSqlDataAccess(string sConstr) : base(sConstr)
        {
            _ConnectionString = sConstr;
            SqlParsers.properties.ParamPrefix = ":"; //注：PostgreSql是使用冒号作为参数前缀
        }
        public BPostgreSqlDataAccess(DbServerInfo server) : base(server)
        {
            SqlParsers.properties.ParamPrefix = ":"; //注：PostgreSql是使用冒号作为参数前缀
        }
        #endregion

        #region 创建SQL Server连接
        /// <summary>
        /// 创建SQL Server连接
        /// </summary>
        /// <returns></returns>
        public override DbConnection GetCurrentConnection()
        {
            return new NpgsqlConnection(_ConnectionString);
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
            //连接字符串示例：User ID=postgres;Password=sa;Host=localhost;Port=5432;Database=AprilSpring;Pooling=true
            _ConnectionString = server.UseConnString ? server.ConnString : string.Format("Host={0};Port={1};User ID={2};Password={3};Database={4};Pooling=true", server.ServerName, server.PortNo, server.UserName, server.Password, server.Database);
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
                if (item.Key.Equals("host"))
                {
                    server.ServerName = item.Value;
                }
                else if (item.Key.Equals("port"))
                {
                    server.PortNo = item.Value;
                }
                else if (item.Key.Equals("database"))
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
                NpgsqlCommand sqlCommon;
                if (dbTran == null)
                {
                    sqlCommon = new NpgsqlCommand(sHadParaSql, (NpgsqlConnection)conn);
                }
                else
                {
                    sqlCommon = new NpgsqlCommand(sHadParaSql, (NpgsqlConnection)conn, (NpgsqlTransaction)dbTran);
                }
                //构造适配器
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlCommon);
                if (listParam != null)
                {
                    foreach (FuncParam item in listParam)
                    {
                        NpgsqlParameter sp = new NpgsqlParameter(item.Code, item.Value);
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
            pageDataSql = string.Format("SELECT ROWNUM AS ROWNUM, MYTABLE.*" + " FROM ({0}) MYTABLE ORDER BY ROWNUM LIMIT {1} OFFSET {2} ",
                pageDataSql, endRow, beginRow);
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
            NpgsqlCommand sqlCommon;
            if (dbTran == null)
            {
                sqlCommon = new NpgsqlCommand(sHadParaSql, (NpgsqlConnection)conn);
            }
            else
            {
                sqlCommon = new NpgsqlCommand(sHadParaSql, (NpgsqlConnection)conn, (NpgsqlTransaction)dbTran);
            }

            if (listParam != null)
            {
                foreach (FuncParam item in listParam)
                {
                    NpgsqlParameter sp = new NpgsqlParameter(item.Code, item.Value);
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
                DataRow[] dcPKList = dtTableInfo.Select(DBColumnEntity.SqlString.KeyType + "='PK'");


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
                            strInsertEnd.Append(sDouHao + "now()");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "= now()");
                        }
                        else if (tcy == DbDefaultValueType.TimeStamp)
                        {
                            strInsertEnd.Append(sDouHao + "current_timestamp");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "= current_timestamp");
                        }
                        else if (tcy == DbDefaultValueType.Guid)
                        {
                            string sGuid = Guid.NewGuid().ToString("N");
                            strInsertEnd.Append(sDouHao + "'"+ sGuid + "'");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "= '" + sGuid + "'");
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
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                NpgsqlConnection con = (NpgsqlConnection)conn;
                //"INSERT INTO dbo.t6( com_id ,usr_id ) VALUES( @com_id ,@usr_id)"
                string strInsert = strInsertPre.ToString() + strInsertEnd.ToString();
                adapter.InsertCommand = new NpgsqlCommand(strInsert, con, (NpgsqlTransaction)dbTran);
                adapter.InsertCommand.CommandType = CommandType.Text;

                string strUpdateSql = strUpdate.ToString() + strWhere.ToString();
                adapter.UpdateCommand = new NpgsqlCommand(strUpdateSql, con, (NpgsqlTransaction)dbTran); //"update t6 set usr_id=@usr_id where com_id=@com_id and usr_id=@usr_id1"
                adapter.UpdateCommand.CommandType = CommandType.Text;

                string strDeleteSql = strDelete.ToString() + strWhere.ToString();
                adapter.DeleteCommand = new NpgsqlCommand(strDeleteSql, con, (NpgsqlTransaction)dbTran); //"delete from t6 where com_id=@com_id and usr_id=@usr_id"
                adapter.DeleteCommand.CommandType = CommandType.Text;

                foreach (DataColumn dc in dt.Columns)
                {
                    DataRow[] drCol = dtTableInfo.Select(DBColumnEntity.SqlString.Name + "='" + dc.ColumnName + "'");
                    Int32 iLen = 0;
                    if (!string.IsNullOrEmpty(drCol[0][DBColumnEntity.SqlString.DataLength].ToString()))
                    {
                        iLen = Int32.Parse(drCol[0][DBColumnEntity.SqlString.DataLength].ToString());
                    }
                    NpgsqlDbType ot = TransToSqlType(drCol[0][DBColumnEntity.SqlString.DataType].ToString().ToUpper());
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
        public NpgsqlDbType TransToSqlType(string strSqlDbType)
        {
            /*PostgreSql数据库类型：
            BIGINT、BIGSERIAL、BIT、BIT VARYING、BOOLEAN、BOX、BYTEA、
            CHARACTER VARYING、CHARACTER、CIDR、CIRCLE、DATE、
            DOUBLE PRECISION、INET、INTEGER、INTERVAL、LINE、LSEG、
            MACADDR、MONEY、NUMERIC、PATH、POINT、POLYGON、
            REAL、SMALLINT、SERIAL、TEXT、TIME、TIMESTAMP
            */
            NpgsqlDbType dType = NpgsqlDbType.Varchar;
            strSqlDbType = strSqlDbType.ToUpper(); //注：要转换为大写来判断
            if (strSqlDbType == "NVARCHAR" || strSqlDbType == "CHARACTER VARYING")
            {
                return dType;
            }
            else if (strSqlDbType == "VARCHAR" || strSqlDbType == "CHARACTER")
            {
                return NpgsqlDbType.Varchar;
            }
            else if (strSqlDbType == "CHAR")
            {
                return NpgsqlDbType.Char;
            }
            else if (strSqlDbType == "NCHAR")
            {
                return NpgsqlDbType.Char;
            }
            else if (strSqlDbType == "MONEY")
            {
                return NpgsqlDbType.Money;
            }
            else if (strSqlDbType == "TEXT")
            {
                return NpgsqlDbType.Text;
            }
            else if (strSqlDbType == "REAL")
            {
                return NpgsqlDbType.Real;
            }
            else if (strSqlDbType == "XML")
            {
                return NpgsqlDbType.Xml;
            }
            #region 数值类
            else if (strSqlDbType == "INTEGER")
            {
                return NpgsqlDbType.Integer;
            }
            else if (strSqlDbType == "SMALLINT")
            {
                return NpgsqlDbType.Integer;
            }
            else if (strSqlDbType == "BIGINT")
            {
                return NpgsqlDbType.Bigint;
            }
            else if (strSqlDbType == "NUMERIC")
            {
                return NpgsqlDbType.Numeric;
            }
            else if (strSqlDbType == "FLOAT")
            {
                return NpgsqlDbType.Numeric;
            }
            if (strSqlDbType == "TINYINT")
            {
                return NpgsqlDbType.Integer;
            }
            #endregion

            #region 日期时间类
            else if (strSqlDbType == "DATE")
            {
                return NpgsqlDbType.Date;
            }
            else if (strSqlDbType == "TIMESTAMP")
            {
                return NpgsqlDbType.Timestamp;
            }
            else if (strSqlDbType == "TIME")
            {
                return NpgsqlDbType.Time;
            }
            else if (strSqlDbType == "System.Time")
            {
                return NpgsqlDbType.Time;
            }
            #endregion

            else if (strSqlDbType == "BIT" || strSqlDbType == "BIT VARYING")
            {
                return NpgsqlDbType.Boolean;
            }
            else if (strSqlDbType == "INTERVAL")
            {
                return NpgsqlDbType.Interval;
            }
            else if (strSqlDbType == "BOX")
            {
                return NpgsqlDbType.Box;
            }
            else if (strSqlDbType == "BYTEA")
            {
                return NpgsqlDbType.Bytea;
            }
            else if (strSqlDbType == "CIDR")
            {
                return NpgsqlDbType.Cidr;
            }
            else if (strSqlDbType == "CIRCLE")
            {
                return NpgsqlDbType.Circle;
            }
            else if (strSqlDbType == "INET")
            {
                return NpgsqlDbType.Inet;
            }
            else if (strSqlDbType == "PATH")
            {
                return NpgsqlDbType.Path;
            }
            else if (strSqlDbType == "POINT")
            {
                return NpgsqlDbType.Point;
            }
            else if (strSqlDbType == "POLYGON")
            {
                return NpgsqlDbType.Polygon;
            }
            else if (strSqlDbType == "SERIAL")
            {
                return NpgsqlDbType.Range;
            }
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
        public NpgsqlDbType DbTypeToSqlDbType(DbType pSourceType)
        {
            NpgsqlParameter paraConver = new NpgsqlParameter();
            paraConver.DbType = pSourceType;
            return paraConver.NpgsqlDbType;
        }

        public DbType SqlDbTypeToDbType(NpgsqlDbType pSourceType)
        {
            NpgsqlParameter paraConver = new NpgsqlParameter();
            paraConver.NpgsqlDbType = pSourceType;
            return paraConver.DbType;
        }
        #endregion

        #region 增加参数
        public override DbParameter AddParam(string[] sParameterArr, DbCommand dbCommand, DbParameter sqlParameter, int i, string[] sProperties, string parameterCode, string executeType, string parameterType)
        {
            NpgsqlCommand sqlCommand = dbCommand as NpgsqlCommand;
            if (parameterType == "BIGINT")
            {
                sqlParameter = sqlCommand.Parameters.Add(_SqlDiff.ParamPrefix + parameterCode, NpgsqlDbType.Bigint);
                sqlParameter.Value = Int64.Parse(sParameterArr[i]);
            }
            else if (parameterType == "INT")
            {
                sqlParameter = sqlCommand.Parameters.Add(_SqlDiff.ParamPrefix + parameterCode, NpgsqlDbType.Integer);
                if (executeType != "RETURNVALUE")
                    sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "DECIMAL")
            {
                sqlParameter = sqlCommand.Parameters.Add(_SqlDiff.ParamPrefix + parameterCode, NpgsqlDbType.Numeric);
                sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "VARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add(_SqlDiff.ParamPrefix + parameterCode, NpgsqlDbType.Varchar, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "NVARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add(_SqlDiff.ParamPrefix + parameterCode, NpgsqlDbType.Varchar, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "DATETIME")
            {
                sqlParameter = sqlCommand.Parameters.Add(_SqlDiff.ParamPrefix + parameterCode, NpgsqlDbType.Date);
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
                dbCommand = new NpgsqlCommand(sSql, (NpgsqlConnection)con);
            }
            else
            {
                dbCommand = new NpgsqlCommand(sSql, (NpgsqlConnection)con, (NpgsqlTransaction)tran);
            }

            adpater = new NpgsqlDataAdapter();
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
            using (NpgsqlConnection con = (NpgsqlConnection)GetCurrentConnection())
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
            using (NpgsqlConnection con = (NpgsqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable dtSource = con.GetSchema(DBSchemaString.Databases, null);
                //返回标准的结果表
                DataTable dtReturn = DT_DataBase;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBDataBaseEntity.SqlString.Name] = drS[PostgreSqlSchemaString.DataBase.Name];
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
            using (NpgsqlConnection con = (NpgsqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                DataTable res = con.GetSchema(DBSchemaString.Restrictions);//查询GetSchema第二个参数的含义说明
                //Tables:Catalog,Schema,Table,TableType
                DataTable dtSource = con.GetSchema(DBSchemaString.Tables,new string[] { null,sSchema,sTableName,null});
                //返回标准的结果表
                DataTable dtReturn = DT_SchemaTable;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBTableEntity.SqlString.Schema] = drS[PostgreSqlSchemaString.Table.TableSchema];
                    dr[DBTableEntity.SqlString.Name] = drS[PostgreSqlSchemaString.Table.TableName];
                    //SqlSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
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
            using (NpgsqlConnection con = (NpgsqlConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                //单独处理主键
                string sSql = @"SELECT A.CONNAME AS PK_NAME,C.ATTNAME AS COLUMN_NAME,D.TYPNAME AS DATA_TYPE 
                FROM PG_CONSTRAINT A
                JOIN PG_CLASS B
	                ON A.CONRELID = B.OID 
                JOIN PG_ATTRIBUTE C 
	                ON C.ATTRELID = B.OID AND  C.ATTNUM = A.CONKEY[1]
                JOIN PG_TYPE D ON D.OID = C.ATTTYPID
                WHERE UPPER(A.CONTYPE)='P'
                AND B.RELNAME ='#TABLE_NAME#'";
                IDictionary<string, string> dic = new Dictionary<string, string>();
                dic["TABLE_NAME"] = sTableName;
                DataTable dtPK = QueryAutoParamSqlData(sSql, dic);
                bool isPKOK = false;

                DataTable dtSource = con.GetSchema(DBSchemaString.Columns, new string[] { null, null, sTableName });//使用通用的获取架构方法
                DataTable dtReturn = DT_SchemaTableColumn;
                foreach (DataRow drS in dtSource.Select("", "ORDINAL_POSITION asc"))
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBColumnEntity.SqlString.TableSchema] = drS[PostgreSqlSchemaString.Column.TableSchema];//Schema跟数据库名称一样
                    dr[DBColumnEntity.SqlString.TableName] = drS[PostgreSqlSchemaString.Column.TableName];
                    dr[DBColumnEntity.SqlString.SortNum] = drS[PostgreSqlSchemaString.Column.OrdinalPosition];
                    dr[DBColumnEntity.SqlString.Name] = drS[PostgreSqlSchemaString.Column.ColumnName];                    
                    dr[DBColumnEntity.SqlString.Default] = drS[PostgreSqlSchemaString.Column.ColumnDefault];
                    dr[DBColumnEntity.SqlString.NotNull] = drS[PostgreSqlSchemaString.Column.IsNullable].ToString().ToUpper().Equals("NO") ? "1" : "";
                    dr[DBColumnEntity.SqlString.DataType] = drS[PostgreSqlSchemaString.Column.DataType];
                    dr[DBColumnEntity.SqlString.DataLength] = drS[PostgreSqlSchemaString.Column.CharacterMaximumLength];
                    dr[DBColumnEntity.SqlString.DataPrecision] = drS[PostgreSqlSchemaString.Column.Numeric_Precision];
                    dr[DBColumnEntity.SqlString.DataScale] = drS[PostgreSqlSchemaString.Column.Numeric_Scale];
                    //dr[SqlColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"];
                    //dr[SqlColumnEntity.SqlString.DataTypeFull] = drS["COLUMN_TYPE"];
                    //dr[SqlColumnEntity.SqlString.NameCN] = drS["COLUMN_CN"];
                    //dr[SqlColumnEntity.SqlString.Extra] = drS["COLUMN_EXTRA"];
                    if (!isPKOK)
                    {
                        DataRow[] arrPK = dtPK.Select("COLUMN_NAME='" + drS[PostgreSqlSchemaString.Column.ColumnName].ToString() + "'");
                        if (arrPK.Length > 0)
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
            IDictionary<string, string> dic = new Dictionary<string, string>();

            //SCHEMANAME一般为public。tablename,SCHEMANAME,TABLENAME,TABLEOWNER,TABLESPACE,HASINDEXES,HASRULES,HASTRIGGERS,ROWSECURITY
            if (string.IsNullOrEmpty(sSchema)) sSchema = "public";

            string sSql = @"SELECT A.TABLENAME AS TABLE_NAME,A.SCHEMANAME AS TABLE_SCHEMA,A.TABLEOWNER AS TABLE_OWNER,
                    CAST(OBJ_DESCRIPTION(RELFILENODE,'PG_CLASS') AS VARCHAR) AS TABLE_COMMENT
                FROM PG_TABLES A
                LEFT JOIN PG_CLASS B
                  ON A.TABLENAME = B.RELNAME
                WHERE 1=1 AND POSITION('_2' IN A.TABLENAME)=0
                AND SCHEMANAME = '#TABLE_SCHEMA#'
                AND TABLENAME = '#TABLE_NAME#'
            ";

            
            dic["TABLE_SCHEMA"] = sSchema;
            dic["TABLE_NAME"] = sTableName;
            DataTable dtSource = QueryAutoParamSqlData(sSql, dic);
            DataTable dtReturn = DT_SchemaTable;
            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();
                dr[DBTableEntity.SqlString.Schema] = drS["TABLE_SCHEMA"];//一般为public
                dr[DBTableEntity.SqlString.Name] = drS["TABLE_NAME"];
                DBSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
                dr[DBTableEntity.SqlString.Owner] = drS["TABLE_OWNER"];//拥有者
                dr[DBTableEntity.SqlString.DBName] = DbServer.Database;//数据库名
                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        public override DataTable GetSqlSchemaTableColumns(string sTableName, string sSchema = null)
        {
            string sSql = @"SELECT A.TABLE_NAME,
				A.TABLE_SCHEMA,
				A.ORDINAL_POSITION,
	            A.COLUMN_NAME,
	            A.DATA_TYPE,
	            A.CHARACTER_MAXIMUM_LENGTH,
	            A.NUMERIC_PRECISION,
	            A.NUMERIC_SCALE,
	            A.IS_NULLABLE,
	            A.COLUMN_DEFAULT,
	            CASE WHEN POSITION('NEXTVAL' IN COLUMN_DEFAULT)>0 THEN 1 ELSE 0 END AS IS_IDENTITY,
	            (SELECT 'PK'
	            FROM pg_constraint AA
	            INNER JOIN pg_class BB ON AA.conrelid = BB.oid
	            INNER JOIN pg_attribute CC ON CC.attrelid = BB.oid
		            AND CC.attnum = AA.conkey[1]
	            INNER JOIN pg_type DD ON DD.oid = CC.atttypid
	            WHERE  BB.relname = A.TABLE_NAME
	            AND AA.contype = 'p' AND CC.attname = A.COLUMN_NAME
	            ) AS COLUMN_KEY,
	            B.COLUMN_COMMENT,
	            B.COLUMN_TYPE,
                split_part(B.COLUMN_COMMENT,':',1) AS COLUMN_CN,
                split_part(B.COLUMN_COMMENT,':',2) AS COLUMN_EXTRA
            FROM INFORMATION_SCHEMA.COLUMNS A
            LEFT JOIN (select c.relname,a.attname,d.description COLUMN_COMMENT,
		               concat_ws('',t.typname,SUBSTRING(format_type(a.atttypid,a.atttypmod) from '\(.*\)')) as COLUMN_TYPE 
	            from pg_class c,pg_attribute a,pg_type t,pg_description d
	            where a.attnum>0 and a.attrelid=c.oid and a.atttypid=t.oid and d.objoid=a.attrelid and d.objsubid=a.attnum
	            and c.relname in (select tablename from pg_tables where schemaname='public' and position('_2' in tablename)=0) 
             ) B ON A.TABLE_NAME = B.relname AND A.COLUMN_NAME = B.attname
            WHERE 1=1 
            AND upper(A.TABLE_SCHEMA)='PUBLIC' 
            AND A.TABLE_NAME = '#TABLE_NAME#'
            ORDER BY A.ORDINAL_POSITION ASC
            ";

            IDictionary<string, string> dic = new Dictionary<string, string>();
            dic["TABLE_NAME"] = sTableName;//PpostgreSQl的表都是小写的。但转换SQL的工具MyPeachNet会把所有SQL转换为大写？？？
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
                dr[DBColumnEntity.SqlString.KeyType] = drS["COLUMN_KEY"];
                dr[DBColumnEntity.SqlString.NameCN] = drS["COLUMN_CN"];
                dr[DBColumnEntity.SqlString.Extra] = drS["COLUMN_EXTRA"];

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
        #endregion
    }
}
