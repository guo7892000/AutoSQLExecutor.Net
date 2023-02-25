using Breezee.Core.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 查询SQL构造类
    /// </summary>
    public class QuerySqlBuilder: BaseSqlBuilder
    {
        public override SqlExecuteType SqlExecuteType { get { return SqlExecuteType.Query; } }

        public QuerySqlBuilder(IDataAccess iDataAccess):base(iDataAccess)
        {
        }

        public QuerySqlBuilder New(IDataAccess iDataAccess)
        {
            return new QuerySqlBuilder(iDataAccess);
        }

        #region 查询数据
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public DataTable Query(List<FuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                string sSql;
                List<FuncParam> realParam;
                GetSqlParam(listParam, out sSql, out realParam);
                //调用接口方法
                return DataAccess.QueryHadParamSqlData(sSql, realParam, conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 查询数据(兼容旧方式)
        /// <summary>
        /// 查询数据(兼容旧方式)
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public DataTable Query(IDictionary<string, string> dicQuery, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                string sSql;
                List<FuncParam> realParam;
                GetSqlParam(dicQuery, out sSql, out realParam);
                //调用接口方法
                return DataAccess.QueryHadParamSqlData(sSql, realParam, conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
