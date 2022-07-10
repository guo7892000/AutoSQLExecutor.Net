// See https://aka.ms/new-console-template for more information

using Breezee.AutoSQLExecutor.Common;
using Breezee.AutoSQLExecutor.Core;
using System.Data;
using Newtonsoft.Json;

DbServerInfo dbServer = new DbServerInfo();
//dbServer.DbType(DataBaseType.SqlServer).Server("localhost").User("sa").Pwd("sa123456").Port("1443").Db("PeachBase");//测试通过
//dbServer.DbType(DataBaseType.MySql).Server("localhost").User("root").Pwd("root").Port("3306").Db("mis_main");//测试通过
dbServer.DbType(DataBaseType.SQLite).Server(@"C:\Users\Administrator\Desktop\SQLite_MISFramework.db");//测试通过

IDictionary<string, string> dicQuery = new Dictionary<string, string>();
DataTable dt;
//dt = SqlExecutor.Connect(dbServer).QuerySingleTableData("BAS_CITY", dicQuery);//测试通过

string sSql = "SELECT * FROM BAS_CITY T WHERE T.CITY_ID = @CITY_ID";
//dicQuery["CITY_ID"] = "10";
//dt = SqlExecutor.Connect(dbServer).QueryHadParamSqlData(sSql, dicQuery);//测试通过

//
//sSql = "SELECT * FROM BAS_CITY T WHERE T.CITY_ID = #CITY_ID#";
sSql = "SELECT * FROM sys_dll T WHERE T.DLL_ID = #DLL_ID#";//SQL中没有发现键会报错，是否允许？？配置输出SQL日志？？
dt = AutoSQLExecutor.Connect(dbServer).QueryAutoParamData(sSql, dicQuery);//测试通过

Console.WriteLine(dt.Rows.Count);
Console.WriteLine(JsonConvert.SerializeObject(dt.Rows[0].ItemArray));
