using Autofac;
using Breezee.AutoSQLExecutor.Core;
using Breezee.AutoSQLExecutor.MySql;
using Breezee.AutoSQLExecutor.Oracle;
using Breezee.AutoSQLExecutor.PostgreSQL;
using Breezee.AutoSQLExecutor.SQLite;
using Breezee.AutoSQLExecutor.SqlServer;


/*********************************************************************	
 * 对象名称：	
 * 对象类别：类	
 * 创建作者：黄国辉	
 * 创建日期：2022/11/26 22:15:46	
 * 对象说明：	
 * 电邮地址: guo7892000@126.com	
 * 微信号: BreezeeHui	
 * 修改历史：	
 *      2022/11/26 22:15:46 新建 黄国辉 	
 * ******************************************************************/
namespace Breezee.Framework.Mini.BLL
{
    /// <summary>
    /// 类
    /// </summary>
    public class AutoSqlExecutorIocModuleAutofac : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BSqlServerDataAccess>().As<IDataAccess>().Named<IDataAccess>("ISqlServerDataAccess");
            builder.RegisterType<BMySqlDataAccess>().As<IDataAccess>().Named<IDataAccess>("IMySqlDataAccess");
            builder.RegisterType<BOracleDataAccess>().As<IDataAccess>().Named<IDataAccess>("IOracleDataAccess");
            builder.RegisterType<BSQLiteDataAccess>().As<IDataAccess>().Named<IDataAccess>("ISQLiteDataAccess");
            builder.RegisterType<BPostgreSqlDataAccess>().As<IDataAccess>().Named<IDataAccess>("IPostgreSqlDataAccess");
        }
    }
}