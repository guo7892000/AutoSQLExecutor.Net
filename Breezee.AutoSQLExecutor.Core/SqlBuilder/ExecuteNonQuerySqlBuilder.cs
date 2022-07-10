using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 非执行类的SQL构造类
    /// </summary>
    public class ExecuteNonQuerySqlBuilder : BaseSqlBuilder
    {
        public override SqlExecuteType SqlExecuteType { get { return SqlExecuteType.ExecuteNonQuery; } }

        public ExecuteNonQuerySqlBuilder(IDataAccess iDataAccess):base(iDataAccess)
        {
        }

        public ExecuteNonQuerySqlBuilder New(IDataAccess iDataAccess)
        {
            return new ExecuteNonQuerySqlBuilder(iDataAccess);
        }

        #region 查询数据
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public int ExecuteNonQuery(List<BaseFuncParam> listParam = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                string sSql;
                List<BaseFuncParam> realParam;
                GetSqlParam(listParam, out sSql, out realParam);
                //调用接口方法
                return DataAccess.ExecuteNonQueryHadParam(sSql, realParam, conn, dbTran); 
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
        public int ExecuteNonQuery(IDictionary<string, string> dicQuery = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                string sSql;
                List<BaseFuncParam> realParam;
                GetSqlParam(dicQuery, out sSql, out realParam);
                //调用接口方法
                return DataAccess.ExecuteNonQueryHadParam(sSql, realParam, conn, dbTran);
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
        /// <param name="dicQuery"></param>
        /// <param name="conn"></param>
        /// <param name="dbTran"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(IDictionary<string, string> dicQuery = null, DbConnection conn = null, DbTransaction dbTran = null,params string[] paramArr)
        {
            try
            {
                string sSql;
                List<BaseFuncParam> realParam = new List<BaseFuncParam>();
                GetSqlParam(dicQuery, out sSql, out realParam);

                foreach (var param in paramArr)
                {
                    if(realParam.Where(p=>p.Code == param).Count()==0)
                    {
                        realParam.Add(new BaseFuncParam(param, FuncParamType.String, FuncParamInputType.MustNotNull, dicQuery[param]));
                    }
                }
                //调用接口方法
                return DataAccess.ExecuteNonQueryHadParam(sSql, realParam, conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
