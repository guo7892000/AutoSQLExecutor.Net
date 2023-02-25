using Breezee.AutoSQLExecutor.Core;
using Breezee.AutoSQLExecutor.MySql;
using Breezee.AutoSQLExecutor.Oracle;
using Breezee.AutoSQLExecutor.PostgreSQL;
using Breezee.AutoSQLExecutor.SQLite;
using Breezee.AutoSQLExecutor.SqlServer;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;


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
    public class AutoSqlExecutorIocInstallerCastle: IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IDataAccess>().ImplementedBy<BSqlServerDataAccess>().Named("ISqlServerDataAccess"));
            container.Register(Component.For<IDataAccess>().ImplementedBy<BMySqlDataAccess>().Named("IMySqlDataAccess"));
            container.Register(Component.For<IDataAccess>().ImplementedBy<BOracleDataAccess>().Named("IOracleDataAccess"));
            container.Register(Component.For<IDataAccess>().ImplementedBy<BSQLiteDataAccess>().Named("ISQLiteDataAccess"));
            container.Register(Component.For<IDataAccess>().ImplementedBy<BPostgreSqlDataAccess>().Named("IPostgreSqlDataAccess"));
        }
    }
}