// See https://aka.ms/new-console-template for more information

using Breezee.AutoSQLExecutor.Common;
using Breezee.AutoSQLExecutor.Core;
using System.Data;
using Newtonsoft.Json;

DbServerInfo dbServer = new DbServerInfo();
//连接SqlServer：
//dbServer.DbType(DataBaseType.SqlServer).Server("localhost").User("sa").Pwd("sa123456").Port("1443").Db("PeachBase");//测试通过

//连接MariaDB数据库：
//dbServer.DbType(DataBaseType.MySql).Server("localhost").User("root").Pwd("root").Port("3306").Db("mis_main");//测试通过

//连接MSQLite数据库：主要是文件形式
//dbServer.DbType(DataBaseType.SQLite).Server(@"C:\Users\Administrator\Desktop\SQLite_MISFramework.db");//测试通过

//连接Oralce数据库：数据库在Win7的虚拟机中，这里使用主机名
//dbServer.DbType(DataBaseType.Oracle).Server("//USER-20220625SI:1521/orcl").User("hui").Pwd("hui");//测试通过,不知为啥用orcl的TNS名会报错？？

//连接PostgreSQL数据库
dbServer.DbType(DataBaseType.PostgreSql).Server("localhost").Db("MISMain").User("postgres").Pwd("postgres");//测试通过

IDictionary<string, string> dicQuery = new Dictionary<string, string>();
DataTable dt;
//dt = SqlExecutor.Connect(dbServer).QuerySingleTableData("BAS_CITY", dicQuery);//测试通过

string sSql = "SELECT * FROM BAS_CITY T WHERE T.CITY_ID = @CITY_ID";
//dicQuery["CITY_ID"] = "10";
//dt = SqlExecutor.Connect(dbServer).QueryHadParamSqlData(sSql, dicQuery);//测试通过

//
//sSql = "SELECT * FROM BAS_CITY T WHERE T.CITY_ID = #CITY_ID#";
//dicQuery["TYPE_ID"] = "0015da9e-593a-45fd-826d-9ebe26500f4d";
sSql = "SELECT * FROM BAS_TYPE T WHERE T.TYPE_ID = '#TYPE_ID#'";//
dt = AutoSQLExecutor.Connect(dbServer).QueryAutoParamData(sSql, dicQuery);

Console.WriteLine(dt.Rows.Count);
Console.WriteLine(JsonConvert.SerializeObject(dt.Rows[0].ItemArray));
