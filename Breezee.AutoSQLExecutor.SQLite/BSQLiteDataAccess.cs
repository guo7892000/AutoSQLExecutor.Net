using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;
using System.Xml;
using Breezee.AutoSQLExecutor.Core;
using System.Collections;
using Breezee.Core.Interface;

namespace Breezee.AutoSQLExecutor.SQLite
{
    public class BSQLiteDataAccess: IDataAccess
    {
        #region 属性
        public override DataBaseType DataBaseType
        {
            get { return DataBaseType.SQLite; }
        }

        private string _ConnectionString;
        public override string ConnectionString
        {
            get { return _ConnectionString; }
            protected set { _ConnectionString = value; }
        }

        ISqlDifferent _SqlDiff = new BSQLiteSqlDifferent();
        public override ISqlDifferent SqlDiff { get => _SqlDiff; protected set => _SqlDiff = value; }

        public override List<string> CharLengthTypes { get => (new string[] { SQLiteColumnType.Text.character, SQLiteColumnType.Text.nchar, SQLiteColumnType.Text.nvarchar,
        SQLiteColumnType.Text.varchar,SQLiteColumnType.Text.varyingCharacter}).ToList(); }
        public override List<string> PrecisonTypes { get => (new string[] { SQLiteColumnType.Numeric.numeric, SQLiteColumnType.Numeric.Decimal }).ToList(); }
        #endregion

        #region 有参构造函数
        /// <summary>
        /// 有参构造函数
        /// </summary>
        public BSQLiteDataAccess(string sConstr) : base(sConstr)
        {
            _ConnectionString = sConstr;
            SqlParsers.properties.ParamPrefix = "@"; //注：SQLite是使用@作为参数前缀
        }
        public BSQLiteDataAccess(DbServerInfo server) : base(server)
        {
            SqlParsers.properties.ParamPrefix = "@"; //注：SQLite是使用@作为参数前缀
        }
        #endregion

        #region 创建数据库连接
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sConstr">连接字符串：注名字要跟Main.config配置文件中的连接字符串配置字符保持一致</param>
        public override DbConnection GetCurrentConnection()
        {
            return new SQLiteConnection(_ConnectionString);
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
            /*连接字符串示例：Data Source=WorkHelper.db;Version=3;Pooling=True;Max Pool Size=100;Password=myPassword; */
            _ConnectionString = server.UseConnString ? server.ConnString : string.Format("Data Source={0};Version=3;Pooling=True;Max Pool Size=100", server.ServerName);
            if (!server.UseConnString && !string.IsNullOrEmpty(server.Password))
            {
                _ConnectionString += string.Format(";Password={0}", server.Password);//加密后能打开连接，但查询不了数据
            }
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
                SQLiteCommand sqlCommon;
                if (dbTran == null)
                {
                    sqlCommon = new SQLiteCommand(sHadParaSql, (SQLiteConnection)conn);
                }
                else
                {
                    sqlCommon = new SQLiteCommand(sHadParaSql, (SQLiteConnection)conn, (SQLiteTransaction)dbTran);
                }
                //构造适配器
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(sqlCommon);
                if (listParam != null)
                {
                    foreach (FuncParam item in listParam)
                    {
                        SQLiteParameter sp = new SQLiteParameter(item.Code, item.Value);
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
            string pageDataSql = string.Format("SELECT ROWID AS ROWNUM, * FROM ( {0} ) AS MYTABLE ORDER BY {1} LIMIT {2} OFFSET {3}", 
                sHadParaSql, pParam.PageOrderString, endRow, beginRow);
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
            SQLiteCommand sqlCommon;
            if (dbTran == null)
            {
                sqlCommon = new SQLiteCommand(sHadParaSql, (SQLiteConnection)conn);
            }
            else
            {
                sqlCommon = new SQLiteCommand(sHadParaSql, (SQLiteConnection)conn, (SQLiteTransaction)dbTran);
            }

            if (listParam != null)
            {
                foreach (FuncParam item in listParam)
                {
                    SQLiteParameter sp = new SQLiteParameter(item.Code, item.Value);
                    if (item.FuncParamType == FuncParamType.DateTime)
                    {
                        sp.DbType = DbType.DateTime;
                    }
                    sqlCommon.Parameters.Add(sp);
                }
            }

            if (conn.State == ConnectionState.Closed) conn.Open();
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
                DataRow[] dcPKList = dtTableInfo.Select(DBColumnEntity.SqlString.KeyType + " = 'PK'");
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
                            strInsertEnd.Append(sDouHao + "datetime('now')");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=datetime('now')");
                        }
                        else if (tcy == DbDefaultValueType.TimeStamp)
                        {
                            strInsertEnd.Append(sDouHao + "datetime('now')");
                            strUpdate.Append(sUpdateDouHao + dc.ColumnName + "=datetime('now')");
                        }
                        else if (tcy == DbDefaultValueType.Guid)
                        {
                            string sGuid = Guid.NewGuid().ToString("N");
                            strInsertEnd.Append(sDouHao + "'" + sGuid + "'");
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
                SQLiteDataAdapter adapter = new SQLiteDataAdapter();
                SQLiteConnection con = (SQLiteConnection)conn;
                //"INSERT INTO dbo.t6( com_id ,usr_id ) VALUES( @com_id ,@usr_id)"
                string strInsert = strInsertPre.ToString() + strInsertEnd.ToString();
                adapter.InsertCommand = new SQLiteCommand(strInsert, con, (SQLiteTransaction)dbTran);
                adapter.InsertCommand.CommandType = CommandType.Text;

                string strUpdateSql = strUpdate.ToString() + strWhere.ToString();
                adapter.UpdateCommand = new SQLiteCommand(strUpdateSql, con, (SQLiteTransaction)dbTran); //"update t6 set usr_id=@usr_id where com_id=@com_idand usr_id=@usr_id1"
                adapter.UpdateCommand.CommandType = CommandType.Text;

                string strDeleteSql = strDelete.ToString() + strWhere.ToString();
                adapter.DeleteCommand = new SQLiteCommand(strDeleteSql, con, (SQLiteTransaction)dbTran); //"delete from t6 where com_id=@com_idand usr_id=@usr_id"
                adapter.DeleteCommand.CommandType = CommandType.Text;

                foreach (DataColumn dc in dt.Columns)
                {
                    DataRow[] drCol = dtTableInfo.Select(DBColumnEntity.SqlString.Name + "='" + dc.ColumnName + "'");
                    
                    Int32 iLen = 0;
                    if (!string.IsNullOrEmpty(drCol[0][SQLiteSchemaString.Column.CharacterMaximumLength].ToString()))
                    {
                        iLen = Int32.Parse(drCol[0][SQLiteSchemaString.Column.CharacterMaximumLength].ToString());
                    }
                    DbType ot = TransToDbType(drCol[0][SQLiteSchemaString.Column.DataType].ToString()); //这里要修改
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

        #region 将数据中的表类型转为DbType
        /// <summary>
        /// 将数据中的表类型转为SqlDbType
        /// </summary>
        /// <param name="strSQLiteDbType">数据库中的类型名称</param>
        /// <returns>DbType</returns>
        public DbType TransToDbType(string strSQLiteDbType)
        {
            /*SQLite数据库类型：
            INT、INTEGER、TINYINT、SMALLINT、MEDIUMINT、BIGINT、UNSIGNED BIG INT、INT2、INT8、CHARACTER(20)、VARCHAR(255)、
            VARYING CHARACTER(255)、NCHAR(55)、NATIVE CHARACTER(70)、NVARCHAR(100)、TEXT、CLOB、BLOB、REAL、
            DOUBLE、DOUBLE PRECISION、FLOAT、NUMERIC、DECIMAL(10,5)、BOOLEAN、DATE、DATETIME
            */
            DbType dType = DbType.String;
            strSQLiteDbType = strSQLiteDbType.ToUpper(); //注：要转换为大写来判断
            #region 字符类
            if (strSQLiteDbType == "NVARCHAR" || strSQLiteDbType == "CHARACTER")
            {
                return dType;
            }
            else if (strSQLiteDbType == "VARCHAR" || strSQLiteDbType == "VARYING CHARACTER")
            {
                return DbType.String;
            }
            else if(strSQLiteDbType == "NATIVE CHARACTER")
            {
                return DbType.String;
            }
            else if(strSQLiteDbType == "NCHAR")
            {
                return DbType.String;
            }
            else if(strSQLiteDbType == "CLOB")
            {
                return DbType.String;
            }
            else if(strSQLiteDbType == "TEXT")
            {
                return DbType.String;
            }
            #endregion

            else if(strSQLiteDbType == "BLOB")
            {
                return DbType.Binary;
            }

            #region 数值类
            else if(strSQLiteDbType == "TINYINT" || strSQLiteDbType == "INT2" || strSQLiteDbType == "INT8")
            {
                return DbType.Int16;
            }
            else if(strSQLiteDbType == "SMALLINT")
            {
                return DbType.Int16;
            }
            else if (strSQLiteDbType == "INT")
            {
                return DbType.Int32;
            }
            else if (strSQLiteDbType == "INTEGER")
            {
                return DbType.Int32;
            }
            else if(strSQLiteDbType == "BIGINT"|| strSQLiteDbType == "UNSIGNED BIG INT")
            {
                return DbType.Int64;
            }
            else if(strSQLiteDbType == "DOUBLE")
            {
                return DbType.Double;
            }
            else if(strSQLiteDbType == "DOUBLE PRECISION")
            {
                return DbType.Double;
            }
            else if(strSQLiteDbType == "FLOAT")
            {
                return DbType.Double;
            }
            else if (strSQLiteDbType == "REAL")
            {
                return DbType.Double;
            }
            else if(strSQLiteDbType == "DECIMAL")
            {
                return DbType.Decimal;
            }
            else if(strSQLiteDbType == "NUMERIC")
            {
                return DbType.VarNumeric;
            }

            #endregion

            #region 日期时间类
            else if (strSQLiteDbType == "DATE")
            {
                return DbType.DateTime;
            }
            else if(strSQLiteDbType == "DATETIME")
            {
                return DbType.DateTime;
            }
            else if(strSQLiteDbType == "TIMESTAMP")
            {
                return DbType.DateTime2;
            }
            #endregion

            else if (strSQLiteDbType == "BOOLEAN")
            {
                return DbType.Boolean;
            }

            return dType;
        }
        #endregion

        #region 增加SQLite参数
        public override DbParameter AddParam(string[] sParameterArr, DbCommand dbCommand, DbParameter sqlParameter, int i, string[] sProperties, string parameterCode, string executeType, string parameterType)
        {
            SQLiteCommand sqlCommand = dbCommand as SQLiteCommand;
            if (parameterType == "BIGINT")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, DbType.Int64);
                sqlParameter.Value = Int64.Parse(sParameterArr[i]);
            }
            else if (parameterType == "INT")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, DbType.UInt32);
                if (executeType != "RETURNVALUE")
                    sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "DECIMAL")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, DbType.Decimal);
                sqlParameter.Value = Decimal.Parse(sParameterArr[i]);
            }
            else if (parameterType == "VARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, DbType.String, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "NVARCHAR")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, DbType.StringFixedLength, int.Parse(sProperties[3]));
                sqlParameter.Value = sParameterArr[i];
            }
            else if (parameterType == "DATETIME")
            {
                sqlParameter = sqlCommand.Parameters.Add("@" + parameterCode, DbType.DateTime);
                sqlParameter.Value = DateTime.Parse(sParameterArr[i]);
            }
            else
            {
                throw new Exception("未定义的存储过程的参数类型：" + executeType);
            }
            return sqlParameter;
        }
        #endregion

        #region 设置存储过程命令_重写方法
        public override void SetProdureCommond(string sSql, DbConnection con, DbTransaction tran, out DbCommand dbCommand, out DbDataAdapter adpater)
        {
            if (tran == null)
            {
                dbCommand = new SQLiteCommand(sSql, (SQLiteConnection)con);
            }
            else
            {
                dbCommand = new SQLiteCommand(sSql, (SQLiteConnection)con, (SQLiteTransaction)tran);
            }

            adpater = new SQLiteDataAdapter();
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
            using (SQLiteConnection con = (SQLiteConnection)GetCurrentConnection())
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
            using (SQLiteConnection con = (SQLiteConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                IDictionary<string, string> dic = new Dictionary<string, string>();
                string sDBSql = "PRAGMA database_list;";
                DataTable dtSource = QueryHadParamSqlData(sDBSql, dic);
                //返回标准的结果表
                DataTable dtReturn = DT_DataBase;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBDataBaseEntity.SqlString.Name] = drS[SQLiteSchemaString.DataBase.Name];
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
            using (SQLiteConnection con = (SQLiteConnection)GetCurrentConnection())
            {
                if (con.State == ConnectionState.Closed) con.Open();
                //DataTable res = con.GetSchema(DBSchemaString.Restrictions);//查询GetSchema第二个参数的含义说明：SQLite不支持
                DataTable dtSource = con.GetSchema(DBSchemaString.Tables);
                //返回标准的结果表
                DataTable dtReturn = DT_SchemaTable;
                if (sTableName != null && dtSource.Rows.Count > 0)
                {
                    DataRow[] rows = dtSource.Select(SQLiteSchemaString.Table.TableName + "='" + sTableName + "'");
                    foreach (DataRow drS in rows)
                    {
                        FetchRow(dtReturn, drS);
                    }
                    return dtReturn;
                }
                foreach (DataRow drS in dtSource.Rows)
                {
                    FetchRow(dtReturn, drS);
                }
                return dtReturn;
            }
        }

        private static void FetchRow(DataTable dtReturn, DataRow drS)
        {
            DataRow dr = dtReturn.NewRow();
            dr[DBTableEntity.SqlString.Schema] = drS[SQLiteSchemaString.Table.TableSchema];
            dr[DBTableEntity.SqlString.Name] = drS[SQLiteSchemaString.Table.TableName];
            dr[DBTableEntity.SqlString.NameUpper] = drS[SQLiteSchemaString.Table.TableName].ToString().FirstLetterUpper();
            dr[DBTableEntity.SqlString.NameLower] = drS[SQLiteSchemaString.Table.TableName].ToString().FirstLetterUpper(false);
            //SqlSchemaCommon.SetComment(dr, drS["TABLE_COMMENT"].ToString());
            dr[DBTableEntity.SqlString.Owner] = drS[SQLiteSchemaString.Table.TableCatalog];

            dtReturn.Rows.Add(dr);
        }

        /// <summary>
        /// 查询DB表元数据信息
        /// </summary>
        /// <param name="sTableName">表名</param>
        /// <returns></returns>
        public override DataTable GetSchemaTableColumns(string sTableName)
        {
            using (SQLiteConnection con = (SQLiteConnection)GetCurrentConnection())
            {
                //利用连接的获取Schema方法
                con.Open();
                DataTable dtSource = con.GetSchema(DBSchemaString.Columns, new string[] { null, null, sTableName });
                DataTable dtReturn = DT_SchemaTableColumn;
                foreach (DataRow drS in dtSource.Rows)
                {
                    DataRow dr = dtReturn.NewRow();
                    dr[DBColumnEntity.SqlString.TableSchema] = drS[SQLiteSchemaString.Column.TableSchema];//Schema跟数据库名称一样
                    dr[DBColumnEntity.SqlString.TableName] = drS[SQLiteSchemaString.Column.TableName];
                    dr[DBColumnEntity.SqlString.TableNameUpper] = drS[SQLiteSchemaString.Column.TableName].ToString().FirstLetterUpper();
                    dr[DBColumnEntity.SqlString.TableNameLower] = drS[SQLiteSchemaString.Column.TableName].ToString().FirstLetterUpper(false);
                    dr[DBColumnEntity.SqlString.SortNum] = drS[SQLiteSchemaString.Column.OrdinalPosition];
                    dr[DBColumnEntity.SqlString.Name] = drS[SQLiteSchemaString.Column.ColumnName];
                    dr[DBColumnEntity.SqlString.NameUpper] = drS[SQLiteSchemaString.Column.ColumnName].ToString().FirstLetterUpper();
                    dr[DBColumnEntity.SqlString.NameLower] = drS[SQLiteSchemaString.Column.ColumnName].ToString().FirstLetterUpper(false);
                    //dr[SqlColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"];
                    dr[DBColumnEntity.SqlString.Default] = drS[SQLiteSchemaString.Column.ColumnDefault];
                    dr[DBColumnEntity.SqlString.NotNull] = drS[SQLiteSchemaString.Column.IsNullable].ToString().ToUpper().Equals("FALSE") ? "1" : "";
                    dr[DBColumnEntity.SqlString.DataType] = drS[SQLiteSchemaString.Column.DataType];
                    dr[DBColumnEntity.SqlString.DataLength] = drS[SQLiteSchemaString.Column.CharacterMaximumLength];
                    dr[DBColumnEntity.SqlString.DataPrecision] = drS[SQLiteSchemaString.Column.Numeric_Precision];
                    dr[DBColumnEntity.SqlString.DataScale] = drS[SQLiteSchemaString.Column.Numeric_Scale];
                    dr[DBColumnEntity.SqlString.KeyType] = drS[SQLiteSchemaString.Column.PrimaryKey].ToString().ToUpper().Equals("TRUE") ? "PK" : "";
                    //dr[SqlColumnEntity.SqlString.DataTypeFull] = drS["PRIMARY_KEY"];
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
            //SQLite表名区分大小写
            IDictionary<string, string> dic = new Dictionary<string, string>();
            string sDBSql = "PRAGMA database_list;";
            DataTable dtDBList = QueryHadParamSqlData(sDBSql, dic);
            string sSql;
            string sDBName = "main";
            if (!string.IsNullOrEmpty(sSchema) && !sSchema.ToLower().Equals("main"))
            {
                bool hasDB = false;
                foreach (DataRow dr in dtDBList.Rows)
                {
                    if(dr["name"].ToString().ToLower().Equals(sSchema.ToLower()))
                    {
                        hasDB = true;
                        break;
                    }
                }
                if(!hasDB)
                {
                    throw new Exception(string.Format("GetSqlSchemaTables方法传入的Schema（{0}）不存在！", sSchema));
                }
                sDBName = sSchema;
            }
            //TYPE,NAME,TBL_NAME,ROOTPAGE,SQL：注这里的表名区分大小写，必须两边都转换为大写然后比较！！
            sSql = string.Format(@"SELECT NAME AS TABLE_NAME
                FROM {0}.SQLITE_MASTER 
                WHERE TYPE = 'table' 
                 AND UPPER(NAME)= '#TABLE_NAME#'
                ", sDBName);
            //不支持备注信息和架构
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
                dr[DBTableEntity.SqlString.DBName] = sDBName;
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
            //SQLite的列名区分大小写
            DataTable dtReturn = DT_SchemaTableColumn;
            //当传空时，查询全部表的全部列
            if (listTableName.Count == 0)
            {
                string sSql = "select name from sqlite_master where type='table' order by name";
                DataTable dtSource = QueryHadParamSqlData(sSql,new Dictionary<string,string>());
                foreach (DataRow dr in dtSource.Rows)
                {
                    listTableName.Add(dr[0].ToString());
                }
            }
            
            foreach (string sTableName in listTableName)
            {
                string sSql = string.Format(@"PRAGMA TABLE_INFO('{0}')", sTableName);
                IDictionary<string, string> dic = new Dictionary<string, string>();
                dic[DBColumnEntity.SqlString.TableName] = sTableName;
                DataTable dtQuery = GetColumnTable(sSql, dic);
                dtReturn.CopyExistColumnData(dtQuery);
            }
            return dtReturn;
        }

        private DataTable GetColumnTable(string sSql, IDictionary<string, string> dic)
        {
            DataTable dtSource = QueryHadParamSqlData(sSql, dic);
            DataTable dtReturn = DT_SchemaTableColumn;
            foreach (DataRow drS in dtSource.Rows)
            {
                DataRow dr = dtReturn.NewRow();
                //dr[DBColumnEntity.SqlString.TableSchema] = drS["TABLE_SCHEMA"];//Schema跟数据库名称一样
                dr[DBColumnEntity.SqlString.TableName] = dic[DBColumnEntity.SqlString.TableName];
                dr[DBColumnEntity.SqlString.TableNameUpper] = dic[DBColumnEntity.SqlString.TableName].FirstLetterUpper();
                dr[DBColumnEntity.SqlString.TableNameLower] = dic[DBColumnEntity.SqlString.TableName].FirstLetterUpper(false);
                dr[DBColumnEntity.SqlString.SortNum] = drS["cid"];
                dr[DBColumnEntity.SqlString.Name] = drS["name"]; //区分大小写，这里没转换为大写
                dr[DBColumnEntity.SqlString.NameUpper] = drS["name"].ToString().FirstLetterUpper();
                dr[DBColumnEntity.SqlString.NameLower] = drS["name"].ToString().FirstLetterUpper(false);
                //dr[DBColumnEntity.SqlString.Comments] = drS["COLUMN_COMMENT"]; //不支持
                dr[DBColumnEntity.SqlString.Default] = drS["dflt_value"];
                dr[DBColumnEntity.SqlString.NotNull] = drS["notnull"].Equals("1") ? "1" : "";
                dr[DBColumnEntity.SqlString.KeyType] = drS["pk"].ToString().Equals("1") ? "PK" : "";
                //dr[DBColumnEntity.SqlString.NameCN] = drS["COLUMN_CN"]; //不支持
                //dr[DBColumnEntity.SqlString.Extra] = drS["COLUMN_EXTRA"]; //不支持
                //类型处理
                string sFullType = drS["type"].ToString();
                dr[DBColumnEntity.SqlString.DataTypeFull] = sFullType;
                string[] arrType = sFullType.Split(new char[] { '(', ',', ')' });
                dr[DBColumnEntity.SqlString.DataType] = arrType[0];
                dr[DBColumnEntity.SqlString.DataLength] = arrType.Length > 1 ? arrType[1] : "";
                if (arrType.Length > 2 && !string.IsNullOrEmpty(arrType[2]))
                {
                    dr[DBColumnEntity.SqlString.DataPrecision] = arrType[1];
                    dr[DBColumnEntity.SqlString.DataScale] = arrType[2];

                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }
        #endregion
    }
}
