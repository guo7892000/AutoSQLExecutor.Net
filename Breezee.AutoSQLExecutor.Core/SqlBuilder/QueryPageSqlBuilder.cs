using Breezee.Core.Interface;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Breezee.AutoSQLExecutor.Core
{
    /// <summary>
    /// 分页SQL构造类
    /// </summary>
    public class QueryPageSqlBuilder: BaseSqlBuilder
    {
        public static PageParam PageParam = new PageParam();

        public override SqlExecuteType SqlExecuteType { get { return SqlExecuteType.QueryPage; } }
        public QueryPageSqlBuilder(IDataAccess iDataAccess):base(iDataAccess)
        {
        }

        public QueryPageSqlBuilder New(IDataAccess iDataAccess)
        {
            return new QueryPageSqlBuilder(iDataAccess);
        }

        #region AddPageSortString方法
        /// <summary>
        /// 增加分页的排序字符
        /// </summary>
        /// <param name="dicKey">键，对应字典键和绑定变量名字</param>
        /// <param name="dicValue">参数值</param>
        /// <param name="dbType">数据类型，默认为Varchar2<</param>
        /// <returns>返回SqlQueryParser的实例</returns>
        public QueryPageSqlBuilder AddPageSortString(string SortString)
        {
            PageParam.PageOrderString = SortString.Trim();
            return this;
        }
        #endregion

        #region 查询分页数据
        /// <summary>
        /// 查询分页数据
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public IDictionary<string, object> QueryPage(PageParam pParam, List<FuncParam> listParam = null, string TotalString = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                string sSql;
                List<FuncParam> realParam;
                GetSqlParam(listParam, out sSql, out realParam);
                //调用接口方法
                return DataAccess.QueryPageHadParamSqlData(sSql, pParam, realParam, TotalString, conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 查询分页数据(兼容旧方式)
        /// <summary>
        /// 查询分页数据(兼容旧方式)
        /// </summary>
        /// <param name="con">数据库连接</param>
        /// <param name="sXPath">配置文件路径</param>
        /// <param name="sKeyValue">查询条件键值</param>
        /// <returns></returns>
        public IDictionary<string, object> QueryPage(PageParam pParam, IDictionary<string, string> dicQuery = null, string TotalString = null, DbConnection conn = null, DbTransaction dbTran = null)
        {
            try
            {
                string sSql;
                List<FuncParam> realParam;
                GetSqlParam(dicQuery, out sSql, out realParam);
                //调用接口方法
                return DataAccess.QueryPageHadParamSqlData(sSql, pParam, realParam, TotalString, conn, dbTran);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public PageParam GetPageParam(IDictionary<string, string> dicQuery)
        {
            int pageSize = 50;
            if (!int.TryParse(dicQuery.SafeGet(AutoSQLCoreStaticConstant.PAGE_SIZE), out pageSize))
            {
                pageSize = 50;
            }

            int pageNo = 1;
            if (!int.TryParse(dicQuery.SafeGet(AutoSQLCoreStaticConstant.PAGE_NO), out pageNo))
            {
                pageNo = 1;
            }

            PageParam.PageSize = pageSize;
            PageParam.PageNO = pageNo;

            return PageParam;
        }
        
    }
}
