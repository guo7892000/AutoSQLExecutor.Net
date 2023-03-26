using Breezee.AutoSQLExecutor.Core;
using Breezee.AutoSQLExecutor.MySql;
using Breezee.AutoSQLExecutor.Oracle;
using Breezee.AutoSQLExecutor.PostgreSQL;
using Breezee.AutoSQLExecutor.SQLite;
using Breezee.AutoSQLExecutor.SqlServer;
using Breezee.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*********************************************************************
 * 对象名称：SQL执行器
 * 对象类别：类
 * 创建作者：黄国辉
 * 创建日期：2022/7/6 23:24:40
 * 对象说明：用于执行SQL
 * 电邮地址：guo7892000@126.com
 * 微 信 号：BreezeeHui
 * 修改历史：
 *      2022/7/6 23:24:40 新建 黄国辉 
 *******************************************************************/
namespace Breezee.AutoSQLExecutor.Common
{
    /// <summary>
    /// SQL执行器
    /// </summary>
    public class AutoSQLExecutors
    {
        public static IDataAccess Connect(DbServerInfo server)
        {
            IDataAccess dbAccess = null;
            switch (server.DatabaseType)
            {
                case DataBaseType.SqlServer:
                    dbAccess =  new BSqlServerDataAccess(server);
                    break;
                case DataBaseType.Oracle:
                    dbAccess = new BOracleDataAccess(server);
                    break;
                case DataBaseType.MySql:
                    dbAccess = new BMySqlDataAccess(server);
                    break;
                case DataBaseType.SQLite:
                    dbAccess = new BSQLiteDataAccess(server);
                    break;
                case DataBaseType.PostgreSql:
                    dbAccess = new BPostgreSqlDataAccess(server);
                    break;
                default:
                    throw new Exception("暂不支持该数据库类型！");
            }
            return dbAccess;
        }
        
    }
}
